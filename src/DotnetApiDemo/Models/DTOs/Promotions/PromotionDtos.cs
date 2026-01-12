using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Promotions;

/// <summary>
/// 促銷活動列表 DTO
/// </summary>
public class PromotionListDto
{
    /// <summary>
    /// 促銷活動 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 促銷代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 促銷名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 促銷類型
    /// </summary>
    public PromotionType PromotionType { get; set; }

    /// <summary>
    /// 促銷狀態
    /// </summary>
    public PromotionStatus Status { get; set; }

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
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 已使用次數
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    public int MaxUsageCount { get; set; }
}

/// <summary>
/// 促銷活動詳細資訊 DTO
/// </summary>
public class PromotionDetailDto
{
    /// <summary>
    /// 促銷活動 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 促銷代碼
    /// </summary>
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
    public PromotionStatus Status { get; set; }

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
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    public decimal MinPurchaseAmount { get; set; }

    /// <summary>
    /// 最高折抵金額
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// 已使用次數
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// 每人使用次數上限
    /// </summary>
    public int MaxUsagePerCustomer { get; set; }

    /// <summary>
    /// 是否可與其他促銷併用
    /// </summary>
    public bool CanCombine { get; set; }

    /// <summary>
    /// 優先順序
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 建立促銷活動請求 DTO
/// </summary>
public class CreatePromotionRequest
{
    /// <summary>
    /// 促銷代碼
    /// </summary>
    [Required(ErrorMessage = "促銷代碼為必填")]
    [StringLength(20, ErrorMessage = "促銷代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 促銷名稱
    /// </summary>
    [Required(ErrorMessage = "促銷名稱為必填")]
    [StringLength(100, ErrorMessage = "促銷名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 促銷說明
    /// </summary>
    [StringLength(500, ErrorMessage = "促銷說明長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 促銷類型
    /// </summary>
    [Required(ErrorMessage = "促銷類型為必填")]
    public PromotionType PromotionType { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    [Required(ErrorMessage = "開始日期為必填")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    [Required(ErrorMessage = "結束日期為必填")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 折扣值
    /// </summary>
    [Required(ErrorMessage = "折扣值為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "折扣值必須大於等於 0")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "最低消費金額必須大於等於 0")]
    public decimal MinPurchaseAmount { get; set; } = 0;

    /// <summary>
    /// 最高折抵金額
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "使用次數上限必須大於等於 0")]
    public int MaxUsageCount { get; set; } = 0;

    /// <summary>
    /// 每人使用次數上限
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "每人使用次數上限必須大於等於 0")]
    public int MaxUsagePerCustomer { get; set; } = 0;

    /// <summary>
    /// 是否可與其他促銷併用
    /// </summary>
    public bool CanCombine { get; set; } = false;

    /// <summary>
    /// 優先順序
    /// </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>
/// 更新促銷活動請求 DTO
/// </summary>
public class UpdatePromotionRequest
{
    /// <summary>
    /// 促銷名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "促銷名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 促銷說明
    /// </summary>
    [StringLength(500, ErrorMessage = "促銷說明長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 促銷狀態
    /// </summary>
    public PromotionStatus? Status { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 折扣值
    /// </summary>
    public decimal? DiscountValue { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    public decimal? MinPurchaseAmount { get; set; }

    /// <summary>
    /// 最高折抵金額
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// 每人使用次數上限
    /// </summary>
    public int? MaxUsagePerCustomer { get; set; }

    /// <summary>
    /// 是否可與其他促銷併用
    /// </summary>
    public bool? CanCombine { get; set; }

    /// <summary>
    /// 優先順序
    /// </summary>
    public int? Priority { get; set; }
}
