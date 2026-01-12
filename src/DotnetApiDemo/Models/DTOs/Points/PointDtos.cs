using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Points;

public class PointTransactionListDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Points { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PointBalanceDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public int TotalEarned { get; set; }
    public int TotalRedeemed { get; set; }
    public int ExpiringPoints { get; set; }
    public DateTime? NextExpiryDate { get; set; }
}

public class EarnPointsRequest
{
    [Required] public int CustomerId { get; set; }
    [Required] public int Points { get; set; }
    public int? OrderId { get; set; }
    public string? Description { get; set; }
    public int? ExpiryDays { get; set; }
}

public class RedeemPointsRequest
{
    [Required] public int CustomerId { get; set; }
    [Required] public int Points { get; set; }
    public int? OrderId { get; set; }
    public string? Description { get; set; }
}

public class AdjustPointsRequest
{
    [Required] public int CustomerId { get; set; }
    [Required] public int Points { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
}
