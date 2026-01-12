namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 分散式鎖服務介面
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// 嘗試獲取鎖
    /// </summary>
    /// <param name="resource">資源名稱</param>
    /// <param name="expiry">鎖定時間</param>
    /// <returns>鎖定物件，若失敗則為 null</returns>
    Task<IDistributedLock?> AcquireAsync(string resource, TimeSpan expiry);

    /// <summary>
    /// 嘗試獲取鎖（含等待）
    /// </summary>
    /// <param name="resource">資源名稱</param>
    /// <param name="expiry">鎖定時間</param>
    /// <param name="wait">等待時間</param>
    /// <param name="retry">重試間隔</param>
    /// <returns>鎖定物件，若失敗則為 null</returns>
    Task<IDistributedLock?> AcquireAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry);
}

/// <summary>
/// 分散式鎖介面
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    /// <summary>
    /// 資源名稱
    /// </summary>
    string Resource { get; }

    /// <summary>
    /// 是否已取得鎖
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// 釋放鎖
    /// </summary>
    Task ReleaseAsync();
}
