namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 點數交易記錄實體
/// </summary>
public class PointTransaction
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Earn, Redeem, Adjust, Expire
    public int Points { get; set; }
    public int? OrderId { get; set; }
    public string? Description { get; set; }
    public int? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual ApplicationUser? CreatedBy { get; set; }
}
