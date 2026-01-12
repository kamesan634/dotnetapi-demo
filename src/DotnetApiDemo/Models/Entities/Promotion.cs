using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 促銷活動實體
/// </summary>
/// <remarks>
/// 定義促銷活動及其規則
/// </remarks>
public class Promotion
{
    /// <summary>
    /// 促銷活動 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 促銷代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PROMO001"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 促銷名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 促銷說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 促銷類型
    /// </summary>
    public PromotionType PromotionType { get; set; }

    /// <summary>
    /// 促銷狀態
    /// </summary>
    public PromotionStatus Status { get; set; } = PromotionStatus.Draft;

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 折扣值
    /// </summary>
    /// <remarks>
    /// 依促銷類型有不同意義：
    /// - Discount: 折扣率 (如 90 表示 9 折)
    /// - AmountOff: 折抵金額
    /// </remarks>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    /// <remarks>
    /// 達到此金額才能使用促銷
    /// </remarks>
    public decimal MinPurchaseAmount { get; set; } = 0;

    /// <summary>
    /// 最高折抵金額
    /// </summary>
    /// <remarks>
    /// 折扣上限
    /// </remarks>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    /// <remarks>
    /// 0 表示不限次數
    /// </remarks>
    public int MaxUsageCount { get; set; } = 0;

    /// <summary>
    /// 已使用次數
    /// </summary>
    public int UsedCount { get; set; } = 0;

    /// <summary>
    /// 每人使用次數上限
    /// </summary>
    public int MaxUsagePerCustomer { get; set; } = 0;

    /// <summary>
    /// 適用門市 ID 列表
    /// </summary>
    /// <remarks>
    /// 為空表示全門市適用，JSON 格式
    /// </remarks>
    public string? ApplicableStoreIds { get; set; }

    /// <summary>
    /// 適用商品分類 ID 列表
    /// </summary>
    /// <remarks>
    /// 為空表示全分類適用，JSON 格式
    /// </remarks>
    public string? ApplicableCategoryIds { get; set; }

    /// <summary>
    /// 適用商品 ID 列表
    /// </summary>
    /// <remarks>
    /// 為空表示全商品適用，JSON 格式
    /// </remarks>
    public string? ApplicableProductIds { get; set; }

    /// <summary>
    /// 適用會員等級 ID 列表
    /// </summary>
    /// <remarks>
    /// 為空表示全會員適用，JSON 格式
    /// </remarks>
    public string? ApplicableLevelIds { get; set; }

    /// <summary>
    /// 是否可與其他促銷併用
    /// </summary>
    public bool CanCombine { get; set; } = false;

    /// <summary>
    /// 優先順序
    /// </summary>
    /// <remarks>
    /// 數字越小優先順序越高
    /// </remarks>
    public int Priority { get; set; } = 0;

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
}
