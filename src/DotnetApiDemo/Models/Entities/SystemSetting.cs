namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 系統設定實體
/// </summary>
/// <remarks>
/// 儲存系統設定項目
/// </remarks>
public class SystemSetting
{
    /// <summary>
    /// 設定 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 設定分類
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 設定鍵值
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 設定值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 設定說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 值類型 (string, int, bool, decimal, json)
    /// </summary>
    public string ValueType { get; set; } = "string";

    /// <summary>
    /// 是否為系統設定 (不可刪除)
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者 ID
    /// </summary>
    public int? UpdatedById { get; set; }

    // 導航屬性

    /// <summary>
    /// 更新者
    /// </summary>
    public virtual ApplicationUser? UpdatedBy { get; set; }
}
