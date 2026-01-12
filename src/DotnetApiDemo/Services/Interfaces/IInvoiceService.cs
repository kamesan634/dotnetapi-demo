using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Invoices;

namespace DotnetApiDemo.Services.Interfaces;

public interface IInvoiceService
{
    Task<PaginatedResponse<InvoiceListDto>> GetInvoicesAsync(PaginationRequest request);
    Task<InvoiceDetailDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDetailDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<InvoiceDetailDto?> GetInvoiceByOrderAsync(int orderId);
    Task<int?> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<bool> VoidInvoiceAsync(int id, VoidInvoiceRequest request);
}
