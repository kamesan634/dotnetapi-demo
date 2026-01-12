using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 採購驗收服務實作
/// </summary>
public class PurchaseReceiptService : IPurchaseReceiptService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseReceiptService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PurchaseReceiptService(ApplicationDbContext context, ILogger<PurchaseReceiptService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PurchaseReceiptListDto>> GetPurchaseReceiptsAsync(
        PaginationRequest request,
        int? purchaseOrderId = null)
    {
        var query = _context.PurchaseReceipts
            .Include(r => r.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
            .Include(r => r.Warehouse)
            .AsQueryable();

        if (purchaseOrderId.HasValue)
        {
            query = query.Where(r => r.PoId == purchaseOrderId.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(r => r.ReceiptNo.Contains(request.Search) ||
                                     r.PurchaseOrder.PoNo.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "receiptno" => request.IsDescending ? query.OrderByDescending(r => r.ReceiptNo) : query.OrderBy(r => r.ReceiptNo),
            "receiptdate" => request.IsDescending ? query.OrderByDescending(r => r.ReceiptDate) : query.OrderBy(r => r.ReceiptDate),
            "pono" => request.IsDescending ? query.OrderByDescending(r => r.PurchaseOrder.PoNo) : query.OrderBy(r => r.PurchaseOrder.PoNo),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new PurchaseReceiptListDto
            {
                Id = r.Id,
                ReceiptNo = r.ReceiptNo,
                PoNo = r.PurchaseOrder.PoNo,
                SupplierName = r.PurchaseOrder.Supplier.Name,
                WarehouseName = r.Warehouse.Name,
                ReceiptDate = r.ReceiptDate,
                TotalReceivedQuantity = r.ReceivedQuantity,
                TotalRejectedQuantity = r.RejectedQuantity,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<PurchaseReceiptListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PurchaseReceiptDetailDto?> GetPurchaseReceiptByIdAsync(int id)
    {
        return await _context.PurchaseReceipts
            .Where(r => r.Id == id)
            .Select(r => new PurchaseReceiptDetailDto
            {
                Id = r.Id,
                ReceiptNo = r.ReceiptNo,
                PoNo = r.PurchaseOrder.PoNo,
                PurchaseOrderId = r.PoId,
                SupplierName = r.PurchaseOrder.Supplier.Name,
                WarehouseId = r.WarehouseId,
                WarehouseName = r.Warehouse.Name,
                ReceiptDate = r.ReceiptDate,
                TotalReceivedQuantity = r.ReceivedQuantity,
                TotalRejectedQuantity = r.RejectedQuantity,
                Notes = r.Notes,
                Items = r.Items.Select(i => new PurchaseReceiptItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    OrderedQuantity = i.OrderedQuantity,
                    ArrivedQuantity = i.ArrivedQuantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    RejectedQuantity = i.RejectedQuantity,
                    RejectionReason = i.RejectionReason,
                    Notes = i.Notes
                }),
                ReceivedByName = r.Receiver.RealName ?? r.Receiver.UserName,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreatePurchaseReceiptAsync(CreatePurchaseReceiptRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 取得採購單
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("建立驗收單失敗：找不到採購單 - PurchaseOrderId: {PurchaseOrderId}", request.PurchaseOrderId);
                return null;
            }

            // 檢查採購單狀態是否允許驗收 (已核准或部分入庫)
            if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                purchaseOrder.Status != PurchaseOrderStatus.Partial)
            {
                _logger.LogWarning("建立驗收單失敗：採購單狀態不允許驗收 - Status: {Status}", purchaseOrder.Status);
                return null;
            }

            // 產生驗收單號
            var receiptNo = await GenerateReceiptNoAsync();

            var receipt = new PurchaseReceipt
            {
                ReceiptNo = receiptNo,
                PoId = request.PurchaseOrderId,
                ReceiptDate = request.ReceiptDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                WarehouseId = purchaseOrder.WarehouseId,
                ReceiverId = userId,
                Status = GoodsReceiptStatus.Completed,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            int totalReceived = 0;
            int totalRejected = 0;
            int totalArrived = 0;

            // 建立採購單明細的字典方便查詢
            var poItemsDict = purchaseOrder.Items.ToDictionary(i => i.Id);

            foreach (var itemRequest in request.Items)
            {
                // 驗證採購單明細是否存在
                if (!poItemsDict.TryGetValue(itemRequest.PoItemId, out var poItem))
                {
                    _logger.LogWarning("建立驗收單失敗：找不到採購單明細 - PoItemId: {PoItemId}", itemRequest.PoItemId);
                    await transaction.RollbackAsync();
                    return null;
                }

                // 驗證驗收數量不超過待驗收數量
                var pendingQty = poItem.Quantity - poItem.ReceivedQuantity;
                if (itemRequest.ReceivedQuantity > pendingQty)
                {
                    _logger.LogWarning("建立驗收單失敗：入庫數量超過待驗收數量 - PoItemId: {PoItemId}, Received: {Received}, Pending: {Pending}",
                        itemRequest.PoItemId, itemRequest.ReceivedQuantity, pendingQty);
                    await transaction.RollbackAsync();
                    return null;
                }

                var receiptItem = new PurchaseReceiptItem
                {
                    PoItemId = itemRequest.PoItemId,
                    ProductId = poItem.ProductId,
                    OrderedQuantity = poItem.Quantity,
                    PreviouslyReceived = poItem.ReceivedQuantity,
                    PendingQuantity = pendingQty,
                    ArrivedQuantity = itemRequest.ArrivedQuantity,
                    ReceivedQuantity = itemRequest.ReceivedQuantity,
                    RejectedQuantity = itemRequest.RejectedQuantity,
                    RejectionReason = itemRequest.RejectionReason,
                    Notes = itemRequest.Notes
                };

                receipt.Items.Add(receiptItem);
                totalReceived += itemRequest.ReceivedQuantity;
                totalRejected += itemRequest.RejectedQuantity;
                totalArrived += itemRequest.ArrivedQuantity;

                // 更新採購單明細的已收貨數量
                poItem.ReceivedQuantity += itemRequest.ReceivedQuantity;

                // 更新庫存
                if (itemRequest.ReceivedQuantity > 0)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == poItem.ProductId &&
                                                  i.WarehouseId == purchaseOrder.WarehouseId);

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = poItem.ProductId,
                            WarehouseId = purchaseOrder.WarehouseId,
                            Quantity = 0,
                            ReservedQuantity = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventory);
                        await _context.SaveChangesAsync();
                    }

                    var beforeQuantity = inventory.Quantity;
                    inventory.Quantity += itemRequest.ReceivedQuantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // 新增庫存異動記錄
                    var movement = new InventoryMovement
                    {
                        ProductId = poItem.ProductId,
                        WarehouseId = purchaseOrder.WarehouseId,
                        MovementType = InventoryMovementType.PurchaseIn,
                        Quantity = itemRequest.ReceivedQuantity,
                        BeforeQuantity = beforeQuantity,
                        AfterQuantity = inventory.Quantity,
                        UnitCost = poItem.UnitPrice,
                        ReferenceType = "PurchaseReceipt",
                        ReferenceNo = receiptNo,
                        Notes = $"採購驗收入庫 - 採購單: {purchaseOrder.PoNo}",
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);
                }
            }

            receipt.TotalQuantity = totalArrived;
            receipt.ReceivedQuantity = totalReceived;
            receipt.RejectedQuantity = totalRejected;

            _context.PurchaseReceipts.Add(receipt);

            // 更新採購單狀態
            var allItemsReceived = purchaseOrder.Items.All(i => i.ReceivedQuantity >= i.Quantity);
            var anyItemsReceived = purchaseOrder.Items.Any(i => i.ReceivedQuantity > 0);

            if (allItemsReceived)
            {
                purchaseOrder.Status = PurchaseOrderStatus.Completed;
            }
            else if (anyItemsReceived)
            {
                purchaseOrder.Status = PurchaseOrderStatus.Partial;
            }

            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立驗收單成功 - {ReceiptNo}, 入庫數量: {ReceivedQuantity}", receiptNo, totalReceived);
            return receipt.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立驗收單失敗");
            throw;
        }
    }

    /// <summary>
    /// 產生驗收單號
    /// </summary>
    /// <returns>驗收單號</returns>
    private async Task<string> GenerateReceiptNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"GR{today}";

        var lastReceipt = await _context.PurchaseReceipts
            .Where(r => r.ReceiptNo.StartsWith(prefix))
            .OrderByDescending(r => r.ReceiptNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastReceipt != null && lastReceipt.ReceiptNo.Length > prefix.Length)
        {
            if (int.TryParse(lastReceipt.ReceiptNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
