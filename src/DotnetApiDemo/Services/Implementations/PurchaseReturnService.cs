using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 採購退貨服務實作
/// </summary>
public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseReturnService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="context">資料庫上下文</param>
    /// <param name="logger">日誌服務</param>
    public PurchaseReturnService(ApplicationDbContext context, ILogger<PurchaseReturnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PurchaseReturnListDto>> GetPurchaseReturnsAsync(
        PaginationRequest request,
        int? supplierId = null)
    {
        var query = _context.PurchaseReturns
            .Include(pr => pr.Supplier)
            .Include(pr => pr.Items)
            .AsQueryable();

        // 供應商篩選
        if (supplierId.HasValue)
        {
            query = query.Where(pr => pr.SupplierId == supplierId.Value);
        }

        // 搜尋 (退貨單號、原採購單號)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(pr =>
                pr.ReturnNo.Contains(request.Search) ||
                (pr.OriginalPoNo != null && pr.OriginalPoNo.Contains(request.Search)));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "returnno" => request.IsDescending ? query.OrderByDescending(pr => pr.ReturnNo) : query.OrderBy(pr => pr.ReturnNo),
            "returndate" => request.IsDescending ? query.OrderByDescending(pr => pr.ReturnDate) : query.OrderBy(pr => pr.ReturnDate),
            "totalamount" => request.IsDescending ? query.OrderByDescending(pr => pr.TotalAmount) : query.OrderBy(pr => pr.TotalAmount),
            "status" => request.IsDescending ? query.OrderByDescending(pr => pr.Status) : query.OrderBy(pr => pr.Status),
            _ => query.OrderByDescending(pr => pr.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pr => new PurchaseReturnListDto
            {
                Id = pr.Id,
                ReturnNo = pr.ReturnNo,
                SupplierName = pr.Supplier.Name,
                OriginalPoNo = pr.OriginalPoNo,
                ReturnDate = pr.ReturnDate,
                ReturnReason = pr.ReturnReason,
                Status = pr.Status,
                TotalQuantity = pr.TotalQuantity,
                TotalAmount = pr.TotalAmount
            })
            .ToListAsync();

        return new PaginatedResponse<PurchaseReturnListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PurchaseReturnDetailDto?> GetPurchaseReturnByIdAsync(int id)
    {
        return await _context.PurchaseReturns
            .Where(pr => pr.Id == id)
            .Select(pr => new PurchaseReturnDetailDto
            {
                Id = pr.Id,
                ReturnNo = pr.ReturnNo,
                SupplierId = pr.SupplierId,
                SupplierName = pr.Supplier.Name,
                OriginalPoNo = pr.OriginalPoNo,
                OriginalReceiptNo = pr.OriginalReceiptNo,
                WarehouseId = pr.WarehouseId,
                WarehouseName = pr.Warehouse.Name,
                ReturnDate = pr.ReturnDate,
                ReturnReason = pr.ReturnReason,
                HandlingMethod = pr.HandlingMethod,
                Status = pr.Status,
                TotalQuantity = pr.TotalQuantity,
                TotalAmount = pr.TotalAmount,
                ReasonNotes = pr.ReasonNotes,
                Items = pr.Items.Select(i => new PurchaseReturnItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Amount,
                    Notes = i.Notes
                }),
                RequesterName = pr.Requester.RealName ?? pr.Requester.UserName ?? string.Empty,
                ApproverName = pr.Approver != null ? (pr.Approver.RealName ?? pr.Approver.UserName) : null,
                ApprovedAt = pr.ApprovedAt,
                CreatedAt = pr.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreatePurchaseReturnAsync(CreatePurchaseReturnRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 產生退貨單號
            var returnNo = await GenerateReturnNoAsync();

            var purchaseReturn = new PurchaseReturn
            {
                ReturnNo = returnNo,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                OriginalPoNo = request.OriginalPoNo,
                OriginalReceiptNo = request.OriginalReceiptNo,
                ReturnDate = request.ReturnDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ReturnReason = request.ReturnReason,
                HandlingMethod = request.HandlingMethod,
                ReasonNotes = request.ReasonNotes,
                Status = PurchaseReturnStatus.Pending,
                RequestedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            int totalQuantity = 0;
            decimal totalAmount = 0;

            // 驗證商品是否存在
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var itemRequest in request.Items)
            {
                if (!products.TryGetValue(itemRequest.ProductId, out var product))
                {
                    _logger.LogWarning("建立退貨單失敗：商品不存在 - ProductId: {ProductId}", itemRequest.ProductId);
                    return null;
                }

                var amount = itemRequest.Quantity * itemRequest.UnitPrice;

                var returnItem = new PurchaseReturnItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    Amount = amount,
                    Notes = itemRequest.Notes
                };

                purchaseReturn.Items.Add(returnItem);
                totalQuantity += itemRequest.Quantity;
                totalAmount += amount;
            }

            purchaseReturn.TotalQuantity = totalQuantity;
            purchaseReturn.TotalAmount = totalAmount;

            _context.PurchaseReturns.Add(purchaseReturn);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立退貨單成功 - {ReturnNo}", returnNo);
            return purchaseReturn.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立退貨單失敗");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApprovePurchaseReturnAsync(int id, int userId)
    {
        var purchaseReturn = await _context.PurchaseReturns.FindAsync(id);
        if (purchaseReturn == null)
        {
            _logger.LogWarning("核准退貨單失敗：退貨單不存在 - Id: {Id}", id);
            return false;
        }

        // 只有待審核狀態才能核准
        if (purchaseReturn.Status != PurchaseReturnStatus.Pending)
        {
            _logger.LogWarning("核准退貨單失敗：狀態不正確 - Id: {Id}, Status: {Status}", id, purchaseReturn.Status);
            return false;
        }

        purchaseReturn.Status = PurchaseReturnStatus.Approved;
        purchaseReturn.ApprovedBy = userId;
        purchaseReturn.ApprovedAt = DateTime.UtcNow;
        purchaseReturn.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("核准退貨單 - ReturnId: {ReturnId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CompletePurchaseReturnAsync(int id, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var purchaseReturn = await _context.PurchaseReturns
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseReturn == null)
            {
                _logger.LogWarning("完成退貨失敗：退貨單不存在 - Id: {Id}", id);
                return false;
            }

            // 只有已核准狀態才能完成
            if (purchaseReturn.Status != PurchaseReturnStatus.Approved &&
                purchaseReturn.Status != PurchaseReturnStatus.Shipped &&
                purchaseReturn.Status != PurchaseReturnStatus.Confirmed)
            {
                _logger.LogWarning("完成退貨失敗：狀態不正確 - Id: {Id}, Status: {Status}", id, purchaseReturn.Status);
                return false;
            }

            // 扣減庫存
            foreach (var item in purchaseReturn.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == purchaseReturn.WarehouseId);

                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // 建立庫存異動記錄
                    var movement = new InventoryMovement
                    {
                        ProductId = item.ProductId,
                        WarehouseId = purchaseReturn.WarehouseId,
                        MovementType = InventoryMovementType.ReturnOut,
                        Quantity = -item.Quantity,
                        BeforeQuantity = inventory.Quantity + item.Quantity,
                        AfterQuantity = inventory.Quantity,
                        ReferenceType = "PurchaseReturn",
                        ReferenceId = purchaseReturn.Id,
                        ReferenceNo = purchaseReturn.ReturnNo,
                        Notes = $"採購退貨 - {purchaseReturn.ReturnNo}",
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);
                }
                else
                {
                    _logger.LogWarning("完成退貨：庫存記錄不存在 - ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                        item.ProductId, purchaseReturn.WarehouseId);
                }
            }

            purchaseReturn.Status = PurchaseReturnStatus.Completed;
            purchaseReturn.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("完成退貨 - ReturnId: {ReturnId}", id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "完成退貨失敗 - ReturnId: {ReturnId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelPurchaseReturnAsync(int id, int userId)
    {
        var purchaseReturn = await _context.PurchaseReturns.FindAsync(id);
        if (purchaseReturn == null)
        {
            _logger.LogWarning("取消退貨單失敗：退貨單不存在 - Id: {Id}", id);
            return false;
        }

        // 已完成的退貨單不能取消
        if (purchaseReturn.Status == PurchaseReturnStatus.Completed)
        {
            _logger.LogWarning("取消退貨單失敗：退貨單已完成 - Id: {Id}", id);
            return false;
        }

        // 已取消的退貨單不能重複取消
        if (purchaseReturn.Status == PurchaseReturnStatus.Cancelled)
        {
            _logger.LogWarning("取消退貨單失敗：退貨單已取消 - Id: {Id}", id);
            return false;
        }

        purchaseReturn.Status = PurchaseReturnStatus.Cancelled;
        purchaseReturn.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("取消退貨單 - ReturnId: {ReturnId}", id);
        return true;
    }

    /// <summary>
    /// 產生退貨單號
    /// </summary>
    /// <returns>退貨單號 (格式：PR + 日期 + 4位序號)</returns>
    private async Task<string> GenerateReturnNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PR{today}";

        var lastReturn = await _context.PurchaseReturns
            .Where(pr => pr.ReturnNo.StartsWith(prefix))
            .OrderByDescending(pr => pr.ReturnNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastReturn != null && lastReturn.ReturnNo.Length > prefix.Length)
        {
            if (int.TryParse(lastReturn.ReturnNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
