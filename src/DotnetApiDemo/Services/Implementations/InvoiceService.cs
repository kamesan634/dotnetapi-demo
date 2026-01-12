using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Invoices;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ApplicationDbContext context, ILogger<InvoiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<InvoiceListDto>> GetInvoicesAsync(PaginationRequest request)
    {
        var query = _context.Invoices.Include(i => i.Order).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(request.Search) ||
                                      i.Order.OrderNo.Contains(request.Search) ||
                                      (i.BuyerTaxId != null && i.BuyerTaxId.Contains(request.Search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.IssuedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceListDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                OrderNumber = i.Order.OrderNo,
                InvoiceType = i.InvoiceType,
                BuyerTaxId = i.BuyerTaxId,
                Amount = i.Amount,
                Status = i.Status,
                IssuedAt = i.IssuedAt
            })
            .ToListAsync();

        return new PaginatedResponse<InvoiceListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<InvoiceDetailDto?> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Order)
            .Where(i => i.Id == id)
            .Select(i => new InvoiceDetailDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                OrderNumber = i.Order.OrderNo,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceType = i.InvoiceType,
                BuyerTaxId = i.BuyerTaxId,
                BuyerName = i.BuyerName,
                Amount = i.Amount,
                TaxAmount = i.TaxAmount,
                Status = i.Status,
                IssuedAt = i.IssuedAt,
                VoidedAt = i.VoidedAt,
                VoidReason = i.VoidReason,
                RandomCode = i.RandomCode,
                CreatedAt = i.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<InvoiceDetailDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        if (invoice == null) return null;
        return await GetInvoiceByIdAsync(invoice.Id);
    }

    public async Task<InvoiceDetailDto?> GetInvoiceByOrderAsync(int orderId)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId);
        if (invoice == null) return null;
        return await GetInvoiceByIdAsync(invoice.Id);
    }

    public async Task<int?> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
        {
            _logger.LogWarning("建立發票失敗：訂單不存在 - OrderId: {OrderId}", request.OrderId);
            return null;
        }

        if (await _context.Invoices.AnyAsync(i => i.OrderId == request.OrderId && i.Status != "Voided"))
        {
            _logger.LogWarning("建立發票失敗：訂單已有發票 - OrderId: {OrderId}", request.OrderId);
            return null;
        }

        var random = new Random();
        var randomCode = random.Next(1000, 9999).ToString();
        var invoiceNumber = $"AB{DateTime.UtcNow:yyyyMM}{random.Next(10000000, 99999999)}";

        var invoice = new Invoice
        {
            OrderId = request.OrderId,
            InvoiceNumber = invoiceNumber,
            InvoiceType = request.InvoiceType,
            BuyerTaxId = request.BuyerTaxId,
            BuyerName = request.BuyerName,
            Amount = order.TotalAmount,
            TaxAmount = order.TaxAmount,
            Status = "Issued",
            IssuedAt = DateTime.UtcNow,
            RandomCode = randomCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice.Id;
    }

    public async Task<bool> VoidInvoiceAsync(int id, VoidInvoiceRequest request)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null || invoice.Status != "Issued") return false;

        invoice.Status = "Voided";
        invoice.VoidedAt = DateTime.UtcNow;
        invoice.VoidReason = request.Reason;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
