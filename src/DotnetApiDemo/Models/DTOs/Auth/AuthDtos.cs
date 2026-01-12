using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Auth;

/// <summary>
/// 登入請求 DTO
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Required(ErrorMessage = "使用者名稱為必填")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼為必填")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 是否記住我
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// 註冊請求 DTO
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Required(ErrorMessage = "使用者名稱為必填")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需在 3-50 字元之間")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度需在 6-100 字元之間")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 確認密碼
    /// </summary>
    [Required(ErrorMessage = "確認密碼為必填")]
    [Compare("Password", ErrorMessage = "密碼與確認密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    [Required(ErrorMessage = "姓名為必填")]
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }
}

/// <summary>
/// Token 回應 DTO
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// 存取 Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新 Token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token 類型
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 過期時間 (秒)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 使用者資訊
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// 使用者資訊 DTO
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 角色列表
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}

/// <summary>
/// 刷新 Token 請求 DTO
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// 刷新 Token
    /// </summary>
    [Required(ErrorMessage = "刷新 Token 為必填")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 修改密碼請求 DTO
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 目前密碼
    /// </summary>
    [Required(ErrorMessage = "目前密碼為必填")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密碼
    /// </summary>
    [Required(ErrorMessage = "新密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度需在 6-100 字元之間")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 確認新密碼
    /// </summary>
    [Required(ErrorMessage = "確認新密碼為必填")]
    [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
