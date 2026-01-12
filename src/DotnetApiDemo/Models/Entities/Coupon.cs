namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 優惠券實體
/// </summary>
/// <remarks>
/// 代表可使用的優惠券
/// </remarks>
public class Coupon
{
    /// <summary>
    /// 優惠券 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 優惠券代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，消費者輸入使用
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 關聯促銷活動 ID
    /// </summary>
    public int PromotionId { get; set; }

    /// <summary>
    /// 持有客戶 ID
    /// </summary>
    /// <remarks>
    /// 為空表示通用券
    /// </remarks>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 有效開始日期
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// 有效結束日期
    /// </summary>
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// 使用日期
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// 使用訂單 ID
    /// </summary>
    public int? UsedOrderId { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 導航屬性

    /// <summary>
    /// 關聯促銷活動
    /// </summary>
    public virtual Promotion Promotion { get; set; } = null!;

    /// <summary>
    /// 持有客戶
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// 使用訂單
    /// </summary>
    public virtual Order? UsedOrder { get; set; }
}
