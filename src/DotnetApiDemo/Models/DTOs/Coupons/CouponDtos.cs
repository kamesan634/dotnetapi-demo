using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Coupons;

/// <summary>
/// 優惠券列表 DTO
/// </summary>
public class CouponListDto
{
    /// <summary>
    /// 優惠券 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 優惠券代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 關聯促銷活動 ID
    /// </summary>
    public int PromotionId { get; set; }

    /// <summary>
    /// 關聯促銷活動名稱
    /// </summary>
    public string PromotionName { get; set; } = string.Empty;

    /// <summary>
    /// 持有客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 持有客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

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
    public bool IsUsed { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 優惠券詳細資訊 DTO
/// </summary>
public class CouponDetailDto
{
    /// <summary>
    /// 優惠券 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 優惠券代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 關聯促銷活動 ID
    /// </summary>
    public int PromotionId { get; set; }

    /// <summary>
    /// 關聯促銷活動名稱
    /// </summary>
    public string PromotionName { get; set; } = string.Empty;

    /// <summary>
    /// 持有客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 持有客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

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
    public bool IsUsed { get; set; }

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
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 建立優惠券請求 DTO
/// </summary>
public class CreateCouponRequest
{
    /// <summary>
    /// 優惠券代碼
    /// </summary>
    [Required(ErrorMessage = "優惠券代碼為必填")]
    [StringLength(50, ErrorMessage = "優惠券代碼長度不可超過 50 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 關聯促銷活動 ID
    /// </summary>
    [Required(ErrorMessage = "關聯促銷活動為必填")]
    public int PromotionId { get; set; }

    /// <summary>
    /// 持有客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 有效開始日期
    /// </summary>
    [Required(ErrorMessage = "有效開始日期為必填")]
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// 有效結束日期
    /// </summary>
    [Required(ErrorMessage = "有效結束日期為必填")]
    public DateTime ValidTo { get; set; }
}

/// <summary>
/// 更新優惠券請求 DTO
/// </summary>
public class UpdateCouponRequest
{
    /// <summary>
    /// 持有客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 有效開始日期
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// 有效結束日期
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}
