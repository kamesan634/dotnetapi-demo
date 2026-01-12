namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 商品分類實體
/// </summary>
/// <remarks>
/// 支援多層級分類結構，透過 ParentId 建立樹狀結構
/// </remarks>
public class Category
{
    /// <summary>
    /// 分類 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分類代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "CAT001"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分類 ID
    /// </summary>
    /// <remarks>
    /// 為 null 表示為頂層分類
    /// </remarks>
    public int? ParentId { get; set; }

    /// <summary>
    /// 分類層級
    /// </summary>
    /// <remarks>
    /// 頂層為 1，子層遞增
    /// </remarks>
    public int Level { get; set; } = 1;

    /// <summary>
    /// 分類路徑
    /// </summary>
    /// <remarks>
    /// 完整路徑，如 "1/2/3"
    /// </remarks>
    public string? Path { get; set; }

    /// <summary>
    /// 分類說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分類圖片 URL
    /// </summary>
    public string? ImageUrl { get; set; }

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
    /// 父分類
    /// </summary>
    public virtual Category? Parent { get; set; }

    /// <summary>
    /// 子分類列表
    /// </summary>
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();

    /// <summary>
    /// 分類下的商品列表
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
