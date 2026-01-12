namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 編號規則實體
/// </summary>
public class NumberRule
{
    public int Id { get; set; }
    public string RuleType { get; set; } = string.Empty; // Order, PurchaseOrder, SalesReturn, etc.
    public string Prefix { get; set; } = string.Empty;
    public string DateFormat { get; set; } = "yyyyMMdd";
    public int SequenceLength { get; set; } = 4;
    public int CurrentSequence { get; set; } = 0;
    public string? LastDate { get; set; }
    public bool ResetDaily { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
