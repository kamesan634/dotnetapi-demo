using Microsoft.AspNetCore.Identity;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 應用程式使用者實體
/// </summary>
/// <remarks>
/// 繼承自 ASP.NET Core Identity 的 IdentityUser，
/// 擴充額外的使用者屬性
/// </remarks>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// 員工編號
    /// </summary>
    /// <remarks>
    /// 用於內部員工識別的唯一編號
    /// </remarks>
    public string? EmployeeNo { get; set; }

    /// <summary>
    /// 真實姓名
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// 暱稱
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 大頭照 URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    /// <remarks>
    /// 停用後無法登入系統
    /// </remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 最後登入 IP
    /// </summary>
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 建立者 ID
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者 ID
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Refresh Token
    /// </summary>
    /// <remarks>
    /// 用於 JWT 刷新機制
    /// </remarks>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Refresh Token 過期時間
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // 導航屬性

    /// <summary>
    /// 使用者所屬門市列表
    /// </summary>
    public virtual ICollection<UserStore> UserStores { get; set; } = new List<UserStore>();
}
