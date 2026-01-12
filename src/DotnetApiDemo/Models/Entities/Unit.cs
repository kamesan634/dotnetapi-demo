namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 計量單位實體
/// </summary>
/// <remarks>
/// 定義商品的計量單位，如：件、個、公斤等
/// </remarks>
public class Unit
{
    /// <summary>
    /// 單位 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 單位代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PCS"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 單位名稱
    /// </summary>
    /// <remarks>
    /// 如：件、個、箱、公斤
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 單位說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統單位
    /// </summary>
    /// <remarks>
    /// 系統單位不可刪除
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
