namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 使用者在線狀態服務介面
/// </summary>
public interface IUserPresenceService
{
    /// <summary>
    /// 設定使用者上線
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="ipAddress">IP 位址</param>
    /// <param name="userAgent">瀏覽器資訊</param>
    Task SetOnlineAsync(int userId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// 設定使用者離線
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    Task SetOfflineAsync(int userId);

    /// <summary>
    /// 更新使用者活動時間
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    Task UpdateActivityAsync(int userId);

    /// <summary>
    /// 檢查使用者是否在線
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>是否在線</returns>
    Task<bool> IsOnlineAsync(int userId);

    /// <summary>
    /// 取得在線使用者數量
    /// </summary>
    /// <returns>在線人數</returns>
    Task<int> GetOnlineCountAsync();

    /// <summary>
    /// 取得在線使用者清單
    /// </summary>
    /// <returns>在線使用者資訊清單</returns>
    Task<IEnumerable<OnlineUserInfo>> GetOnlineUsersAsync();

    /// <summary>
    /// 取得使用者的 Session 資訊
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>Session 資訊</returns>
    Task<UserSessionInfo?> GetUserSessionAsync(int userId);
}

/// <summary>
/// 在線使用者資訊
/// </summary>
public class OnlineUserInfo
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// 登入時間
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 最後活動時間
    /// </summary>
    public DateTime LastActiveTime { get; set; }
}

/// <summary>
/// 使用者 Session 資訊
/// </summary>
public class UserSessionInfo
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 登入時間
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 最後活動時間
    /// </summary>
    public DateTime LastActiveTime { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 瀏覽器資訊
    /// </summary>
    public string? UserAgent { get; set; }
}
