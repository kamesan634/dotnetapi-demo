namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 倉庫實體
/// </summary>
/// <remarks>
/// 代表存放商品的實體倉庫
/// </remarks>
public class Warehouse
{
    /// <summary>
    /// 倉庫 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 倉庫代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "WH001"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所屬門市 ID
    /// </summary>
    /// <remarks>
    /// 可為空，表示為獨立倉庫
    /// </remarks>
    public int? StoreId { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 負責人 ID
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// 倉庫類型
    /// </summary>
    /// <remarks>
    /// 如：門市倉、中央倉、退貨倉等
    /// </remarks>
    public string? WarehouseType { get; set; }

    /// <summary>
    /// 是否為預設倉庫
    /// </summary>
    /// <remarks>
    /// 採購入庫時的預設倉庫
    /// </remarks>
    public bool IsDefault { get; set; } = false;

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
    /// 所屬門市
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// 倉庫負責人
    /// </summary>
    public virtual ApplicationUser? Manager { get; set; }

    /// <summary>
    /// 倉庫庫存列表
    /// </summary>
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
