namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 審計日誌佇列服務介面
/// </summary>
public interface IAuditQueueService
{
    /// <summary>
    /// 將審計日誌加入佇列
    /// </summary>
    /// <param name="entry">審計日誌項目</param>
    Task EnqueueAsync(AuditLogEntry entry);

    /// <summary>
    /// 從佇列取出審計日誌
    /// </summary>
    /// <param name="count">取出數量</param>
    /// <returns>審計日誌項目清單</returns>
    Task<IEnumerable<AuditLogEntry>> DequeueAsync(int count = 100);

    /// <summary>
    /// 取得佇列長度
    /// </summary>
    /// <returns>佇列長度</returns>
    Task<long> GetQueueLengthAsync();
}

/// <summary>
/// 審計日誌項目
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// 項目 ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 操作類型
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 模組
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 目標 ID
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 目標類型
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// 舊值（JSON）
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// 新值（JSON）
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 使用者代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
