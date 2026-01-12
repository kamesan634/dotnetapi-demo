namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// Token 黑名單服務介面
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// 將 Token 加入黑名單
    /// </summary>
    /// <param name="jti">JWT ID</param>
    /// <param name="expiration">Token 過期時間</param>
    Task BlacklistTokenAsync(string jti, DateTime expiration);

    /// <summary>
    /// 檢查 Token 是否在黑名單中
    /// </summary>
    /// <param name="jti">JWT ID</param>
    /// <returns>是否在黑名單中</returns>
    Task<bool> IsTokenBlacklistedAsync(string jti);

    /// <summary>
    /// 將使用者的所有 Token 加入黑名單（強制登出）
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    Task BlacklistUserTokensAsync(int userId);

    /// <summary>
    /// 記錄使用者的 Token（用於強制登出時）
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="jti">JWT ID</param>
    /// <param name="expiration">Token 過期時間</param>
    Task TrackUserTokenAsync(int userId, string jti, DateTime expiration);
}
