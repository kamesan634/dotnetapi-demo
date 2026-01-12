namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 使用者與門市關聯實體
/// </summary>
/// <remarks>
/// 多對多關聯表，一個使用者可以屬於多個門市
/// </remarks>
public class UserStore
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// 是否為主要門市
    /// </summary>
    /// <remarks>
    /// 使用者的主要工作門市
    /// </remarks>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// 加入時間
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // 導航屬性

    /// <summary>
    /// 使用者
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// 門市
    /// </summary>
    public virtual Store Store { get; set; } = null!;
}
