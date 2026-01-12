namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 收銀班別實體
/// </summary>
public class CashierShift
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int CashierId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OpeningAmount { get; set; }
    public decimal ClosingAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal Difference { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalRefunds { get; set; }
    public string Status { get; set; } = "Open"; // Open, Closed
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
    public virtual ApplicationUser Cashier { get; set; } = null!;
}
