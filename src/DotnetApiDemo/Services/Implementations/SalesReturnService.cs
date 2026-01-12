using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SalesReturns;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 銷售退貨服務實作
/// </summary>
public class SalesReturnService : ISalesReturnService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SalesReturnService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SalesReturnService(ApplicationDbContext context, ILogger<SalesReturnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<SalesReturnListDto>> GetSalesReturnsAsync(PaginationRequest request)
    {
        var query = _context.SalesReturns
            .Include(sr => sr.Order)
            .Include(sr => sr.Customer)
            .Include(sr => sr.Store)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(sr => sr.ReturnNumber.Contains(request.Search) ||
                                       sr.Order.OrderNo.Contains(request.Search) ||
                                       (sr.Customer != null && sr.Customer.Name.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "returnnumber" => request.IsDescending ? query.OrderByDescending(sr => sr.ReturnNumber) : query.OrderBy(sr => sr.ReturnNumber),
            "totalamount" => request.IsDescending ? query.OrderByDescending(sr => sr.TotalAmount) : query.OrderBy(sr => sr.TotalAmount),
            "status" => request.IsDescending ? query.OrderByDescending(sr => sr.Status) : query.OrderBy(sr => sr.Status),
            _ => query.OrderByDescending(sr => sr.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sr => new SalesReturnListDto
            {
                Id = sr.Id,
                ReturnNumber = sr.ReturnNumber,
                OrderNumber = sr.Order.OrderNo,
                CustomerName = sr.Customer != null ? sr.Customer.Name : null,
                StoreName = sr.Store.Name,
                Reason = sr.Reason,
                Status = sr.Status,
                TotalAmount = sr.TotalAmount,
                IsRefunded = sr.IsRefunded,
                CreatedAt = sr.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<SalesReturnListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<SalesReturnDetailDto?> GetSalesReturnByIdAsync(int id)
    {
        return await _context.SalesReturns
            .Include(sr => sr.Order)
            .Include(sr => sr.Customer)
            .Include(sr => sr.Store)
            .Include(sr => sr.ProcessedBy)
            .Include(sr => sr.Items)
                .ThenInclude(i => i.Product)
            .Where(sr => sr.Id == id)
            .Select(sr => new SalesReturnDetailDto
            {
                Id = sr.Id,
                ReturnNumber = sr.ReturnNumber,
                OrderId = sr.OrderId,
                OrderNumber = sr.Order.OrderNo,
                CustomerId = sr.CustomerId,
                CustomerName = sr.Customer != null ? sr.Customer.Name : null,
                StoreId = sr.StoreId,
                StoreName = sr.Store.Name,
                Reason = sr.Reason,
                ReasonDescription = sr.ReasonDescription,
                Status = sr.Status,
                TotalAmount = sr.TotalAmount,
                RefundAmount = sr.RefundAmount,
                IsRefunded = sr.IsRefunded,
                RefundedAt = sr.RefundedAt,
                RefundMethod = sr.RefundMethod,
                ProcessedByName = sr.ProcessedBy != null ? sr.ProcessedBy.RealName : null,
                ProcessedAt = sr.ProcessedAt,
                Notes = sr.Notes,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt,
                Items = sr.Items.Select(i => new SalesReturnItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Sku = i.Product.Sku,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                    Reason = i.Reason,
                    Notes = i.Notes
                })
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<SalesReturnDetailDto?> GetSalesReturnByNumberAsync(string returnNumber)
    {
        var salesReturn = await _context.SalesReturns
            .FirstOrDefaultAsync(sr => sr.ReturnNumber == returnNumber);

        if (salesReturn == null)
            return null;

        return await GetSalesReturnByIdAsync(salesReturn.Id);
    }

    /// <inheritdoc />
    public async Task<int?> CreateSalesReturnAsync(CreateSalesReturnRequest request, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("建立退貨單失敗：找不到訂單 - OrderId: {OrderId}", request.OrderId);
            return null;
        }

        if (order.Status != OrderStatus.Completed)
        {
            _logger.LogWarning("建立退貨單失敗：訂單狀態不正確 - OrderId: {OrderId}, Status: {Status}", request.OrderId, order.Status);
            return null;
        }

        // 驗證退貨明細
        foreach (var item in request.Items)
        {
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId);
            if (orderItem == null)
            {
                _logger.LogWarning("建立退貨單失敗：找不到訂單明細 - OrderItemId: {OrderItemId}", item.OrderItemId);
                return null;
            }

            // 檢查可退數量
            var returnedQty = await _context.SalesReturnItems
                .Where(sri => sri.OrderItemId == item.OrderItemId &&
                              sri.SalesReturn.Status != SalesReturnStatus.Cancelled &&
                              sri.SalesReturn.Status != SalesReturnStatus.Rejected)
                .SumAsync(sri => sri.Quantity);

            if (orderItem.Quantity - returnedQty < item.Quantity)
            {
                _logger.LogWarning("建立退貨單失敗：退貨數量超過可退數量 - OrderItemId: {OrderItemId}", item.OrderItemId);
                return null;
            }
        }

        // 產生退貨單號
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var lastReturn = await _context.SalesReturns
            .Where(sr => sr.ReturnNumber.StartsWith($"SR{today}"))
            .OrderByDescending(sr => sr.ReturnNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastReturn != null)
        {
            var lastSeq = lastReturn.ReturnNumber.Substring(10);
            if (int.TryParse(lastSeq, out var parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }
        var returnNumber = $"SR{today}{sequence:D4}";

        var salesReturn = new SalesReturn
        {
            ReturnNumber = returnNumber,
            OrderId = request.OrderId,
            CustomerId = order.CustomerId,
            StoreId = order.StoreId,
            Reason = request.Reason,
            ReasonDescription = request.ReasonDescription,
            Status = SalesReturnStatus.Pending,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;
        foreach (var item in request.Items)
        {
            var orderItem = order.OrderItems.First(oi => oi.Id == item.OrderItemId);
            var product = await _context.Products.FindAsync(orderItem.ProductId);

            var subtotal = orderItem.UnitPrice * item.Quantity;
            totalAmount += subtotal;

            salesReturn.Items.Add(new SalesReturnItem
            {
                OrderItemId = item.OrderItemId,
                ProductId = orderItem.ProductId,
                ProductName = product?.Name ?? orderItem.ProductName,
                Quantity = item.Quantity,
                UnitPrice = orderItem.UnitPrice,
                Subtotal = subtotal,
                Reason = item.Reason ?? request.Reason,
                Notes = item.Notes
            });
        }

        salesReturn.TotalAmount = totalAmount;
        salesReturn.RefundAmount = totalAmount;

        _context.SalesReturns.Add(salesReturn);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立退貨單成功 - {ReturnNumber}", returnNumber);
        return salesReturn.Id;
    }

    /// <inheritdoc />
    public async Task<bool> ProcessSalesReturnAsync(int id, ProcessSalesReturnRequest request, int userId)
    {
        var salesReturn = await _context.SalesReturns
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (salesReturn == null)
        {
            return false;
        }

        if (salesReturn.Status != SalesReturnStatus.Pending)
        {
            _logger.LogWarning("處理退貨單失敗：狀態不正確 - Id: {Id}, Status: {Status}", id, salesReturn.Status);
            return false;
        }

        salesReturn.Status = request.Approve ? SalesReturnStatus.Approved : SalesReturnStatus.Rejected;
        salesReturn.ProcessedById = userId;
        salesReturn.ProcessedAt = DateTime.UtcNow;
        salesReturn.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            salesReturn.Notes = string.IsNullOrWhiteSpace(salesReturn.Notes)
                ? request.Notes
                : $"{salesReturn.Notes}\n[處理備註] {request.Notes}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("處理退貨單成功 - Id: {Id}, Approve: {Approve}", id, request.Approve);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RefundAsync(int id, RefundRequest request, int userId)
    {
        var salesReturn = await _context.SalesReturns
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (salesReturn == null)
        {
            return false;
        }

        if (salesReturn.Status != SalesReturnStatus.Approved)
        {
            _logger.LogWarning("退款失敗：退貨單未核准 - Id: {Id}, Status: {Status}", id, salesReturn.Status);
            return false;
        }

        if (salesReturn.IsRefunded)
        {
            _logger.LogWarning("退款失敗：已退款 - Id: {Id}", id);
            return false;
        }

        // 更新庫存 (將退貨商品加回庫存)
        foreach (var item in salesReturn.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

            if (inventory != null)
            {
                inventory.Quantity += item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
            }
        }

        salesReturn.Status = SalesReturnStatus.Completed;
        salesReturn.RefundAmount = request.Amount;
        salesReturn.RefundMethod = request.Method;
        salesReturn.IsRefunded = true;
        salesReturn.RefundedAt = DateTime.UtcNow;
        salesReturn.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            salesReturn.Notes = string.IsNullOrWhiteSpace(salesReturn.Notes)
                ? request.Notes
                : $"{salesReturn.Notes}\n[退款備註] {request.Notes}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("退款成功 - Id: {Id}, Amount: {Amount}", id, request.Amount);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CancelSalesReturnAsync(int id)
    {
        var salesReturn = await _context.SalesReturns.FindAsync(id);
        if (salesReturn == null)
        {
            return false;
        }

        if (salesReturn.Status == SalesReturnStatus.Completed || salesReturn.IsRefunded)
        {
            _logger.LogWarning("取消退貨單失敗：已完成或已退款 - Id: {Id}", id);
            return false;
        }

        salesReturn.Status = SalesReturnStatus.Cancelled;
        salesReturn.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("取消退貨單成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderItemForReturnDto>> GetReturnableItemsAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return Enumerable.Empty<OrderItemForReturnDto>();
        }

        var result = new List<OrderItemForReturnDto>();

        foreach (var item in order.OrderItems)
        {
            var returnedQty = await _context.SalesReturnItems
                .Where(sri => sri.OrderItemId == item.Id &&
                              sri.SalesReturn.Status != SalesReturnStatus.Cancelled &&
                              sri.SalesReturn.Status != SalesReturnStatus.Rejected)
                .SumAsync(sri => sri.Quantity);

            var returnableQty = item.Quantity - returnedQty;
            if (returnableQty > 0)
            {
                result.Add(new OrderItemForReturnDto
                {
                    OrderItemId = item.Id,
                    ProductId = item.ProductId,
                    Sku = item.Product.Sku,
                    ProductName = item.ProductName,
                    PurchasedQuantity = item.Quantity,
                    ReturnedQuantity = returnedQty,
                    ReturnableQuantity = returnableQty,
                    UnitPrice = item.UnitPrice
                });
            }
        }

        return result;
    }
}
