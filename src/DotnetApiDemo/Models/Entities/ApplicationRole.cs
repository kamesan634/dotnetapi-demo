using Microsoft.AspNetCore.Identity;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 應用程式角色實體
/// </summary>
/// <remarks>
/// 繼承自 ASP.NET Core Identity 的 IdentityRole，
/// 擴充額外的角色屬性
/// </remarks>
public class ApplicationRole : IdentityRole<int>
{
    /// <summary>
    /// 角色說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統角色
    /// </summary>
    /// <remarks>
    /// 系統角色不可刪除
    /// </remarks>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
