namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 付款方式實體
/// </summary>
/// <remarks>
/// 代表系統支援的付款方式
/// </remarks>
public class PaymentMethod
{
    /// <summary>
    /// 付款方式 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 付款方式代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 CASH, CREDIT_CARD, LINE_PAY
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 付款方式名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 說明描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為預設付款方式
    /// </summary>
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
    /// 手續費率 (%)
    /// </summary>
    public decimal FeeRate { get; set; } = 0;

    /// <summary>
    /// 圖示 URL
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
