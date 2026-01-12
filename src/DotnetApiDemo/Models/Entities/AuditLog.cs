namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 稽核日誌實體
/// </summary>
/// <remarks>
/// 記錄系統操作日誌
/// </remarks>
public class AuditLog
{
    /// <summary>
    /// 日誌 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 操作類型 (Create, Update, Delete, Login, Logout, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 實體類型
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// 實體 ID
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// 舊值 (JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// 新值 (JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// 操作說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 使用者代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 操作時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 導航屬性

    /// <summary>
    /// 使用者
    /// </summary>
    public virtual ApplicationUser? User { get; set; }
}
