using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Invoices;

public class InvoiceListDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty;
    public string? BuyerTaxId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
}

public class InvoiceDetailDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty;
    public string? BuyerTaxId { get; set; }
    public string? BuyerName { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public string? RandomCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvoiceRequest
{
    [Required] public int OrderId { get; set; }
    public string InvoiceType { get; set; } = "B2C";
    public string? BuyerTaxId { get; set; }
    public string? BuyerName { get; set; }
}

public class VoidInvoiceRequest
{
    [Required] public string Reason { get; set; } = string.Empty;
}
