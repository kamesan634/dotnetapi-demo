using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 掛單服務實作
/// </summary>
public class SuspendedOrderService : ISuspendedOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly INumberRuleService _numberRuleService;
    private readonly ILogger<SuspendedOrderService> _logger;

    public SuspendedOrderService(
        ApplicationDbContext context,
        INumberRuleService numberRuleService,
        ILogger<SuspendedOrderService> logger)
    {
        _context = context;
        _numberRuleService = numberRuleService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<SuspendedOrderListDto>> GetSuspendedOrdersAsync(
        PaginationRequest request,
        int? storeId = null,
        bool pendingOnly = true)
    {
        var query = _context.SuspendedOrders
            .Include(o => o.Store)
            .Include(o => o.Cashier)
            .Include(o => o.Items)
            .AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        if (pendingOnly)
        {
            query = query.Where(o => o.Status == SuspendedOrderStatus.Pending);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(o =>
                o.OrderNo.Contains(request.Search) ||
                (o.CustomerName != null && o.CustomerName.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "createdat" => request.IsDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
            "expiresat" => request.IsDescending ? query.OrderByDescending(o => o.ExpiresAt) : query.OrderBy(o => o.ExpiresAt),
            "totalamount" => request.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new SuspendedOrderListDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                StoreId = o.StoreId,
                StoreName = o.Store.Name,
                CashierName = o.Cashier.RealName ?? o.Cashier.UserName ?? "",
                CustomerName = o.CustomerName,
                Reason = o.Reason,
                ItemCount = o.Items.Count,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ExpiresAt = o.ExpiresAt
            })
            .ToListAsync();

        return new PaginatedResponse<SuspendedOrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<SuspendedOrderDetailDto?> GetSuspendedOrderByIdAsync(int id)
    {
        return await _context.SuspendedOrders
            .Include(o => o.Store)
            .Include(o => o.Cashier)
            .Include(o => o.Items)
            .Where(o => o.Id == id)
            .Select(o => new SuspendedOrderDetailDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                StoreId = o.StoreId,
                StoreName = o.Store.Name,
                CashierId = o.CashierId,
                CashierName = o.Cashier.RealName ?? o.Cashier.UserName ?? "",
                CustomerId = o.CustomerId,
                CustomerName = o.CustomerName,
                Reason = o.Reason,
                Notes = o.Notes,
                Subtotal = o.Subtotal,
                DiscountAmount = o.DiscountAmount,
                TaxAmount = o.TaxAmount,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ExpiresAt = o.ExpiresAt,
                ResumedAt = o.ResumedAt,
                ResumedOrderId = o.ResumedOrderId,
                Items = o.Items.Select(i => new SuspendedOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    ProductSku = i.ProductSku,
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    OriginalPrice = i.OriginalPrice,
                    DiscountAmount = i.DiscountAmount,
                    Subtotal = i.Subtotal,
                    Notes = i.Notes
                })
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<SuspendedOrderDetailDto?> GetSuspendedOrderByNoAsync(string orderNo)
    {
        var order = await _context.SuspendedOrders
            .FirstOrDefaultAsync(o => o.OrderNo == orderNo);

        if (order == null) return null;

        return await GetSuspendedOrderByIdAsync(order.Id);
    }

    /// <inheritdoc />
    public async Task<int?> CreateSuspendedOrderAsync(CreateSuspendedOrderRequest request, int cashierId)
    {
        // 產生掛單編號
        var orderNo = await _numberRuleService.GenerateNumberAsync("SUS");

        var suspendedOrder = new SuspendedOrder
        {
            OrderNo = orderNo,
            StoreId = request.StoreId,
            CashierId = cashierId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = SuspendedOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(request.ExpirationHours)
        };

        // 處理商品明細
        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var itemRequest in request.Items)
        {
            var product = await _context.Products
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == itemRequest.ProductId);

            if (product == null)
            {
                _logger.LogWarning("建立掛單失敗：找不到商品 - ProductId: {ProductId}", itemRequest.ProductId);
                return null;
            }

            var unitPrice = itemRequest.UnitPrice ?? product.SellingPrice;
            var originalPrice = product.SellingPrice;
            var itemSubtotal = unitPrice * itemRequest.Quantity - itemRequest.DiscountAmount;

            // 計算稅額：根據商品的稅別和稅率計算
            decimal itemTaxAmount = 0;
            if (product.TaxType == TaxType.Taxable && product.TaxRate > 0)
            {
                // 稅額 = 小計 * 稅率 / (100 + 稅率)，採用內含稅計算方式
                itemTaxAmount = Math.Round(itemSubtotal * product.TaxRate / (100 + product.TaxRate), 2);
            }

            var item = new SuspendedOrderItem
            {
                ProductId = product.Id,
                VariantId = itemRequest.VariantId,
                ProductSku = product.Sku,
                ProductName = product.Name,
                Quantity = itemRequest.Quantity,
                UnitPrice = unitPrice,
                OriginalPrice = originalPrice,
                DiscountAmount = itemRequest.DiscountAmount,
                Subtotal = itemSubtotal,
                TaxAmount = itemTaxAmount,
                Notes = itemRequest.Notes
            };

            // 如果有規格，取得規格名稱並調整價格
            if (itemRequest.VariantId.HasValue)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == itemRequest.VariantId.Value);
                if (variant != null)
                {
                    item.VariantName = variant.VariantName;
                    if (itemRequest.UnitPrice == null)
                    {
                        // 規格價格 = 商品售價 + 規格加價
                        var variantPrice = product.SellingPrice + (variant.AdditionalPrice ?? 0);
                        item.UnitPrice = variantPrice;
                        item.Subtotal = variantPrice * itemRequest.Quantity - itemRequest.DiscountAmount;
                    }
                }
            }

            suspendedOrder.Items.Add(item);
            subtotal += item.Subtotal;
            totalDiscount += itemRequest.DiscountAmount;
            totalTax += item.TaxAmount;
        }

        suspendedOrder.Subtotal = subtotal + totalDiscount;
        suspendedOrder.DiscountAmount = totalDiscount;
        suspendedOrder.TaxAmount = totalTax;
        suspendedOrder.TotalAmount = subtotal;

        _context.SuspendedOrders.Add(suspendedOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立掛單成功 - OrderNo: {OrderNo}, StoreId: {StoreId}, Amount: {Amount}",
            suspendedOrder.OrderNo, suspendedOrder.StoreId, suspendedOrder.TotalAmount);

        return suspendedOrder.Id;
    }

    /// <inheritdoc />
    public async Task<SuspendedOrderDetailDto?> ResumeSuspendedOrderAsync(int id, int cashierId)
    {
        var order = await _context.SuspendedOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("恢復掛單失敗：找不到掛單 - Id: {Id}", id);
            return null;
        }

        if (order.Status != SuspendedOrderStatus.Pending)
        {
            _logger.LogWarning("恢復掛單失敗：掛單狀態不正確 - Id: {Id}, Status: {Status}", id, order.Status);
            return null;
        }

        if (order.ExpiresAt < DateTime.UtcNow)
        {
            order.Status = SuspendedOrderStatus.Expired;
            await _context.SaveChangesAsync();
            _logger.LogWarning("恢復掛單失敗：掛單已過期 - Id: {Id}", id);
            return null;
        }

        // 標記為已恢復
        order.Status = SuspendedOrderStatus.Resumed;
        order.ResumedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("恢復掛單成功 - OrderNo: {OrderNo}, CashierId: {CashierId}",
            order.OrderNo, cashierId);

        return await GetSuspendedOrderByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> CancelSuspendedOrderAsync(int id, string? reason = null)
    {
        var order = await _context.SuspendedOrders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        if (order.Status != SuspendedOrderStatus.Pending)
        {
            _logger.LogWarning("取消掛單失敗：掛單狀態不正確 - Id: {Id}, Status: {Status}", id, order.Status);
            return false;
        }

        order.Status = SuspendedOrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? $"取消原因: {reason}"
                : $"{order.Notes}\n取消原因: {reason}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("取消掛單成功 - OrderNo: {OrderNo}, Reason: {Reason}",
            order.OrderNo, reason);

        return true;
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredOrdersAsync()
    {
        var expiredOrders = await _context.SuspendedOrders
            .Where(o => o.Status == SuspendedOrderStatus.Pending && o.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var order in expiredOrders)
        {
            order.Status = SuspendedOrderStatus.Expired;
        }

        await _context.SaveChangesAsync();

        if (expiredOrders.Count > 0)
        {
            _logger.LogInformation("清理過期掛單完成 - Count: {Count}", expiredOrders.Count);
        }

        return expiredOrders.Count;
    }

    /// <inheritdoc />
    public async Task<int> GetPendingCountAsync(int storeId)
    {
        return await _context.SuspendedOrders
            .CountAsync(o => o.StoreId == storeId &&
                           o.Status == SuspendedOrderStatus.Pending &&
                           o.ExpiresAt > DateTime.UtcNow);
    }
}
