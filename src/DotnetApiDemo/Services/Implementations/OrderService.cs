using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 訂單服務實作
/// </summary>
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<OrderListDto>> GetOrdersAsync(
        PaginationRequest request,
        int? storeId = null,
        int? customerId = null)
    {
        var query = _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(o => o.OrderNo.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "orderno" => request.IsDescending ? query.OrderByDescending(o => o.OrderNo) : query.OrderBy(o => o.OrderNo),
            "orderdate" => request.IsDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
            "totalamount" => request.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            _ => query.OrderByDescending(o => o.OrderDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                StoreName = o.Store.Name,
                CustomerName = o.Customer != null ? o.Customer.Name : null,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.TotalAmount - o.DiscountAmount,
                OrderDate = o.OrderDate.ToDateTime(TimeOnly.MinValue),
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync();

        return new PaginatedResponse<OrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<OrderDetailDto?> GetOrderByIdAsync(int id)
    {
        return await GetOrderDetailAsync(o => o.Id == id);
    }

    /// <inheritdoc />
    public async Task<OrderDetailDto?> GetOrderByOrderNoAsync(string orderNo)
    {
        return await GetOrderDetailAsync(o => o.OrderNo == orderNo);
    }

    private async Task<OrderDetailDto?> GetOrderDetailAsync(System.Linq.Expressions.Expression<Func<Order, bool>> predicate)
    {
        return await _context.Orders
            .Where(predicate)
            .Select(o => new OrderDetailDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                StoreId = o.StoreId,
                StoreName = o.Store.Name,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Name : null,
                MemberNo = o.Customer != null ? o.Customer.MemberNo : null,
                Status = o.Status,
                SubTotal = o.Subtotal,
                TaxAmount = o.TaxAmount,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.TotalAmount - o.DiscountAmount,
                PaidAmount = o.PaidAmount,
                Notes = o.Remarks,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.ProductSku,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    Amount = i.Subtotal,
                    Notes = i.Remarks
                }),
                Payments = o.Payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaidAt = p.PaymentTime,
                    TransactionNo = p.TransactionRef,
                    Notes = p.Remarks
                }),
                OrderDate = o.OrderDate.ToDateTime(TimeOnly.MinValue),
                CreatedByName = o.SalesPerson.RealName ?? o.SalesPerson.UserName,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateOrderAsync(CreateOrderRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 產生訂單編號
            var orderNo = await GenerateOrderNoAsync();

            var order = new Order
            {
                OrderNo = orderNo,
                StoreId = request.StoreId,
                CustomerId = request.CustomerId,
                Status = OrderStatus.Pending,
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
                OrderTime = DateTime.UtcNow,
                Remarks = request.Notes,
                SalesPersonId = userId,
                CreatedAt = DateTime.UtcNow
            };

            decimal subTotal = 0;
            decimal discountTotal = 0;

            // 取得商品資訊
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var itemRequest in request.Items)
            {
                if (!products.TryGetValue(itemRequest.ProductId, out var product))
                {
                    _logger.LogWarning("建立訂單失敗：商品不存在 - ProductId: {ProductId}", itemRequest.ProductId);
                    return null;
                }

                var unitPrice = itemRequest.UnitPrice ?? product.SellingPrice;
                var sellingPrice = unitPrice;
                var lineAmount = unitPrice * itemRequest.Quantity;
                var lineDiscount = itemRequest.DiscountAmount;
                var taxRate = product.TaxType == TaxType.Taxable ? 0.05m : 0m;
                var lineTaxAmount = Math.Round((lineAmount - lineDiscount) * taxRate, 2);
                var lineSubtotal = lineAmount - lineDiscount + lineTaxAmount;

                // 取得商品的單位
                var unit = await _context.Units.FindAsync(product.UnitId);

                var orderItem = new OrderItem
                {
                    ProductId = itemRequest.ProductId,
                    ProductSku = product.Sku,
                    ProductName = product.Name,
                    Unit = unit?.Name ?? "個",
                    Quantity = itemRequest.Quantity,
                    UnitPrice = unitPrice,
                    SellingPrice = sellingPrice,
                    CostPrice = product.CostPrice,
                    DiscountAmount = lineDiscount,
                    TaxType = product.TaxType,
                    TaxRate = taxRate * 100,
                    TaxAmount = lineTaxAmount,
                    Subtotal = lineSubtotal,
                    WarehouseId = 1, // 預設倉庫
                    Remarks = itemRequest.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
                subTotal += lineAmount;
                discountTotal += lineDiscount;
            }

            // 計算稅額 (假設 5% 稅率)
            var taxAmount = Math.Round(subTotal * 0.05m, 2);

            order.Subtotal = subTotal;
            order.TaxAmount = taxAmount;
            order.TotalAmount = subTotal + taxAmount - discountTotal;
            order.DiscountAmount = discountTotal;
            order.PaidAmount = 0;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立訂單成功 - {OrderNo}", orderNo);
            return order.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立訂單失敗");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int?> AddPaymentAsync(int orderId, AddPaymentRequest request, int userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status == OrderStatus.Cancelled)
        {
            return null;
        }

        // 產生付款編號
        var paymentNo = await GeneratePaymentNoAsync();

        var payment = new Payment
        {
            OrderId = orderId,
            PaymentNo = paymentNo,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            Status = PaymentStatus.Paid,
            PaymentTime = DateTime.UtcNow,
            TransactionRef = request.TransactionNo,
            Remarks = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // 更新訂單已付金額
        order.PaidAmount += request.Amount;
        order.UpdatedAt = DateTime.UtcNow;

        // 檢查是否完全付款 (TotalAmount - DiscountAmount)
        var finalAmount = order.TotalAmount - order.DiscountAmount;
        if (order.PaidAmount >= finalAmount)
        {
            order.Status = OrderStatus.Completed;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("新增付款成功 - OrderId: {OrderId}, Amount: {Amount}", orderId, request.Amount);
        return payment.Id;
    }

    /// <inheritdoc />
    public async Task<bool> CompleteOrderAsync(int id, int userId)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
        {
            return false;
        }

        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("完成訂單 - OrderId: {OrderId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CancelOrderAsync(int id, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || order.Status == OrderStatus.Completed)
        {
            return false;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("取消訂單 - OrderId: {OrderId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<OrderListDto>> GetCustomerOrdersAsync(int customerId, PaginationRequest request)
    {
        return await GetOrdersAsync(request, customerId: customerId);
    }

    private async Task<string> GenerateOrderNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"SO{today}";

        var lastOrder = await _context.Orders
            .Where(o => o.OrderNo.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastOrder != null && lastOrder.OrderNo.Length > prefix.Length)
        {
            if (int.TryParse(lastOrder.OrderNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private async Task<string> GeneratePaymentNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PAY{today}";

        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNo.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastPayment != null && lastPayment.PaymentNo.Length > prefix.Length)
        {
            if (int.TryParse(lastPayment.PaymentNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PendingOrderDto>> GetPendingOrdersAsync(PaginationRequest request, int? storeId = null)
    {
        var pendingStatuses = new[] { OrderStatus.Pending, OrderStatus.Processing };

        var query = _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => pendingStatuses.Contains(o.Status))
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(o => o.StoreId == storeId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(o => o.OrderTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new PendingOrderDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                StoreName = o.Store.Name,
                CustomerName = o.Customer != null ? o.Customer.Name : null,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                FinalAmount = o.TotalAmount - o.DiscountAmount,
                PaidAmount = o.PaidAmount,
                OrderDate = o.OrderDate.ToDateTime(TimeOnly.MinValue),
                ItemCount = o.OrderItems.Count,
                WaitingMinutes = (int)(DateTime.UtcNow - o.OrderTime).TotalMinutes,
                Priority = o.Priority ?? "Normal"
            })
            .ToListAsync();

        return new PaginatedResponse<PendingOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PendingOrderSummaryDto> GetPendingOrderSummaryAsync(int? storeId = null)
    {
        var query = _context.Orders.AsQueryable();

        if (storeId.HasValue)
            query = query.Where(o => o.StoreId == storeId.Value);

        var pendingOrders = await query
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
            .Select(o => new { o.Status, o.TotalAmount, o.DiscountAmount, o.PaidAmount, o.OrderTime })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var waitingMinutes = pendingOrders.Select(o => (int)(now - o.OrderTime).TotalMinutes).ToList();

        return new PendingOrderSummaryDto
        {
            TotalPending = pendingOrders.Count(o => o.Status == OrderStatus.Pending),
            TotalProcessing = pendingOrders.Count(o => o.Status == OrderStatus.Processing),
            TotalAwaitingPayment = pendingOrders.Count(o => (o.TotalAmount - o.DiscountAmount) > o.PaidAmount),
            TotalPendingAmount = pendingOrders.Sum(o => o.TotalAmount - o.DiscountAmount - o.PaidAmount),
            AverageWaitingMinutes = waitingMinutes.Any() ? (int)waitingMinutes.Average() : 0,
            LongWaitingCount = waitingMinutes.Count(m => m > 30)
        };
    }

    /// <inheritdoc />
    public async Task<bool> StartProcessingOrderAsync(int orderId, int userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Pending)
            return false;

        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("開始處理訂單 - OrderId: {OrderId}, UserId: {UserId}", orderId, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> FinishProcessingOrderAsync(int orderId, int userId, ProcessOrderRequest? request = null)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Processing)
            return false;

        // 檢查是否已付款完成
        var finalAmount = order.TotalAmount - order.DiscountAmount;
        if (order.PaidAmount >= finalAmount)
        {
            order.Status = OrderStatus.Completed;
        }
        else
        {
            order.Status = OrderStatus.Pending; // 尚未付款，回到待處理
        }

        if (request?.Notes != null)
        {
            order.Remarks = string.IsNullOrEmpty(order.Remarks)
                ? request.Notes
                : $"{order.Remarks}\n{request.Notes}";
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("完成訂單處理 - OrderId: {OrderId}, UserId: {UserId}", orderId, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateOrderPriorityAsync(int orderId, string priority, int userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        order.Priority = priority;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("更新訂單優先級 - OrderId: {OrderId}, Priority: {Priority}", orderId, priority);
        return true;
    }
}
