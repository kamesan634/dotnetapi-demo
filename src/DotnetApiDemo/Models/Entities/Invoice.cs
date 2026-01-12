namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 發票實體
/// </summary>
public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = "B2C"; // B2C, B2B
    public string? BuyerTaxId { get; set; }
    public string? BuyerName { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Status { get; set; } = "Issued"; // Issued, Voided, Cancelled
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public string? RandomCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
