namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 門市實體
/// </summary>
/// <remarks>
/// 代表實體店面或銷售據點
/// </remarks>
public class Store
{
    /// <summary>
    /// 門市 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 門市代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "STORE001"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 門市簡稱
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 傳真
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 負責人 ID
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// 營業時間開始
    /// </summary>
    public TimeOnly? OpenTime { get; set; }

    /// <summary>
    /// 營業時間結束
    /// </summary>
    public TimeOnly? CloseTime { get; set; }

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

    // 導航屬性

    /// <summary>
    /// 門市負責人
    /// </summary>
    public virtual ApplicationUser? Manager { get; set; }

    /// <summary>
    /// 門市員工列表
    /// </summary>
    public virtual ICollection<UserStore> UserStores { get; set; } = new List<UserStore>();

    /// <summary>
    /// 門市倉庫列表
    /// </summary>
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
