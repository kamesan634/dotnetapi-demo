using DotnetApiDemo.Models.DTOs.Auth;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 驗證服務介面
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 使用者登入
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <returns>Token 回應</returns>
    Task<TokenResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 使用者註冊
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <returns>是否成功及錯誤訊息</returns>
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// 刷新 Token
    /// </summary>
    /// <param name="request">刷新 Token 請求</param>
    /// <returns>新的 Token 回應</returns>
    Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// 登出
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> LogoutAsync(int userId);

    /// <summary>
    /// 修改密碼
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="request">修改密碼請求</param>
    /// <returns>是否成功及錯誤訊息</returns>
    Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(int userId, ChangePasswordRequest request);

    /// <summary>
    /// 取得目前使用者資訊
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>使用者資訊</returns>
    Task<UserInfo?> GetCurrentUserAsync(int userId);
}
