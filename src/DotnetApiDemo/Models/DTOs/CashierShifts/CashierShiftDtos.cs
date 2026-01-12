using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.CashierShifts;

public class CashierShiftListDto
{
    public int Id { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalTransactions { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CashierShiftDetailDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
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
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OpenShiftRequest
{
    [Required] public int StoreId { get; set; }
    [Required] public decimal OpeningAmount { get; set; }
    public string? Notes { get; set; }
}

public class CloseShiftRequest
{
    [Required] public decimal ClosingAmount { get; set; }
    public string? Notes { get; set; }
}
