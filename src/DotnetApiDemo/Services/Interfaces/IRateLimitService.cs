namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// API 速率限制服務介面
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// 檢查是否允許請求
    /// </summary>
    /// <param name="identifier">識別碼（IP 或 UserId）</param>
    /// <param name="endpoint">API 端點</param>
    /// <param name="limit">限制次數</param>
    /// <param name="window">時間窗口</param>
    /// <returns>是否允許及相關資訊</returns>
    Task<RateLimitResult> CheckRateLimitAsync(string identifier, string endpoint, int limit, TimeSpan window);

    /// <summary>
    /// 取得目前的請求次數
    /// </summary>
    /// <param name="identifier">識別碼</param>
    /// <param name="endpoint">API 端點</param>
    /// <returns>請求次數</returns>
    Task<long> GetCurrentCountAsync(string identifier, string endpoint);
}

/// <summary>
/// 速率限制結果
/// </summary>
public class RateLimitResult
{
    /// <summary>
    /// 是否允許請求
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// 目前請求次數
    /// </summary>
    public long CurrentCount { get; set; }

    /// <summary>
    /// 限制次數
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// 剩餘次數
    /// </summary>
    public long Remaining => Math.Max(0, Limit - CurrentCount);

    /// <summary>
    /// 重置時間（Unix 時間戳）
    /// </summary>
    public long ResetTime { get; set; }

    /// <summary>
    /// 需要等待的秒數（若被限制）
    /// </summary>
    public int RetryAfterSeconds { get; set; }
}
