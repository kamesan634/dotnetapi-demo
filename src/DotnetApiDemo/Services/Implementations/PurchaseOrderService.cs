using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 採購單服務實作
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseOrderService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PurchaseOrderService(ApplicationDbContext context, ILogger<PurchaseOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PurchaseOrderListDto>> GetPurchaseOrdersAsync(
        PaginationRequest request,
        int? supplierId = null)
    {
        var query = _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.Items)
            .AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(po => po.PoNo.Contains(request.Search) ||
                                      po.Supplier.Name.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "pono" => request.IsDescending ? query.OrderByDescending(po => po.PoNo) : query.OrderBy(po => po.PoNo),
            "orderdate" => request.IsDescending ? query.OrderByDescending(po => po.OrderDate) : query.OrderBy(po => po.OrderDate),
            "totalamount" => request.IsDescending ? query.OrderByDescending(po => po.TotalAmount) : query.OrderBy(po => po.TotalAmount),
            "status" => request.IsDescending ? query.OrderByDescending(po => po.Status) : query.OrderBy(po => po.Status),
            _ => query.OrderByDescending(po => po.OrderDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(po => new PurchaseOrderListDto
            {
                Id = po.Id,
                PoNo = po.PoNo,
                SupplierName = po.Supplier.Name,
                WarehouseName = po.Warehouse.Name,
                Status = po.Status,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDate,
                TotalAmount = po.TotalAmount,
                ItemCount = po.Items.Count
            })
            .ToListAsync();

        return new PaginatedResponse<PurchaseOrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderByIdAsync(int id)
    {
        return await GetPurchaseOrderDetailAsync(po => po.Id == id);
    }

    /// <inheritdoc />
    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderByPoNoAsync(string poNo)
    {
        return await GetPurchaseOrderDetailAsync(po => po.PoNo == poNo);
    }

    private async Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(
        System.Linq.Expressions.Expression<Func<PurchaseOrder, bool>> predicate)
    {
        return await _context.PurchaseOrders
            .Where(predicate)
            .Select(po => new PurchaseOrderDetailDto
            {
                Id = po.Id,
                PoNo = po.PoNo,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier.Name,
                WarehouseId = po.WarehouseId,
                WarehouseName = po.Warehouse.Name,
                Status = po.Status,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDate,
                TotalQuantity = po.Items.Sum(i => i.Quantity),
                TotalAmount = po.TotalAmount,
                Notes = po.Notes,
                Items = po.Items.Select(i => new PurchaseOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Subtotal,
                    Notes = i.Notes
                }),
                CreatedByName = po.Buyer.RealName ?? po.Buyer.UserName,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 驗證供應商是否存在
            var supplier = await _context.Suppliers.FindAsync(request.SupplierId);
            if (supplier == null)
            {
                _logger.LogWarning("建立採購單失敗：供應商不存在 - SupplierId: {SupplierId}", request.SupplierId);
                return null;
            }

            // 驗證倉庫是否存在
            var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
            if (warehouse == null)
            {
                _logger.LogWarning("建立採購單失敗：倉庫不存在 - WarehouseId: {WarehouseId}", request.WarehouseId);
                return null;
            }

            // 產生採購單號
            var poNo = await GeneratePoNoAsync();

            var purchaseOrder = new PurchaseOrder
            {
                PoNo = poNo,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpectedDate = request.ExpectedDeliveryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                PaymentTerms = supplier.PaymentTerms,
                Currency = "TWD",
                TaxType = TaxType.Taxable,
                BuyerId = userId,
                Status = PurchaseOrderStatus.Pending,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            decimal subtotal = 0;
            decimal taxAmount = 0;

            // 取得商品資訊
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Include(p => p.Unit)
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var itemRequest in request.Items)
            {
                if (!products.TryGetValue(itemRequest.ProductId, out var product))
                {
                    _logger.LogWarning("建立採購單失敗：商品不存在 - ProductId: {ProductId}", itemRequest.ProductId);
                    return null;
                }

                var lineAmount = itemRequest.UnitPrice * itemRequest.Quantity;
                var lineTaxAmount = Math.Round(lineAmount * 0.05m, 2); // 假設 5% 稅率
                var lineSubtotal = lineAmount + lineTaxAmount;

                var purchaseOrderItem = new PurchaseOrderItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    Unit = product.Unit?.Name ?? "個",
                    UnitPrice = itemRequest.UnitPrice,
                    TaxAmount = lineTaxAmount,
                    Subtotal = lineSubtotal,
                    ReceivedQuantity = 0,
                    Notes = itemRequest.Notes
                };

                purchaseOrder.Items.Add(purchaseOrderItem);
                subtotal += lineAmount;
                taxAmount += lineTaxAmount;
            }

            purchaseOrder.Subtotal = subtotal;
            purchaseOrder.TaxAmount = taxAmount;
            purchaseOrder.TotalAmount = subtotal + taxAmount;

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立採購單成功 - {PoNo}", poNo);
            return purchaseOrder.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立採購單失敗");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApprovePurchaseOrderAsync(int id, int userId)
    {
        var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
        if (purchaseOrder == null)
        {
            _logger.LogWarning("核准採購單失敗：採購單不存在 - Id: {Id}", id);
            return false;
        }

        // 只有待審核狀態的採購單可以核准
        if (purchaseOrder.Status != PurchaseOrderStatus.Pending)
        {
            _logger.LogWarning("核准採購單失敗：採購單狀態不正確 - Id: {Id}, Status: {Status}", id, purchaseOrder.Status);
            return false;
        }

        purchaseOrder.Status = PurchaseOrderStatus.Approved;
        purchaseOrder.ApprovedBy = userId;
        purchaseOrder.ApprovedAt = DateTime.UtcNow;
        purchaseOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("核准採購單成功 - Id: {Id}, ApprovedBy: {UserId}", id, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CancelPurchaseOrderAsync(int id, int userId)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
        {
            _logger.LogWarning("取消採購單失敗：採購單不存在 - Id: {Id}", id);
            return false;
        }

        // 已完成或已取消的採購單不能取消
        if (purchaseOrder.Status == PurchaseOrderStatus.Completed ||
            purchaseOrder.Status == PurchaseOrderStatus.Cancelled ||
            purchaseOrder.Status == PurchaseOrderStatus.Closed)
        {
            _logger.LogWarning("取消採購單失敗：採購單狀態不正確 - Id: {Id}, Status: {Status}", id, purchaseOrder.Status);
            return false;
        }

        // 如果有已入庫的商品，不能取消
        if (purchaseOrder.Items.Any(i => i.ReceivedQuantity > 0))
        {
            _logger.LogWarning("取消採購單失敗：已有商品入庫 - Id: {Id}", id);
            return false;
        }

        purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
        purchaseOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("取消採購單成功 - Id: {Id}", id);
        return true;
    }

    /// <summary>
    /// 產生採購單號
    /// </summary>
    /// <returns>採購單號 (格式: PO + 日期 + 序號)</returns>
    private async Task<string> GeneratePoNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PO{today}";

        var lastPurchaseOrder = await _context.PurchaseOrders
            .Where(po => po.PoNo.StartsWith(prefix))
            .OrderByDescending(po => po.PoNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastPurchaseOrder != null && lastPurchaseOrder.PoNo.Length > prefix.Length)
        {
            if (int.TryParse(lastPurchaseOrder.PoNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
