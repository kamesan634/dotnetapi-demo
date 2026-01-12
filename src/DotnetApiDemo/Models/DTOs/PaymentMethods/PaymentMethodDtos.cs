using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.PaymentMethods;

/// <summary>
/// 付款方式列表 DTO
/// </summary>
public class PaymentMethodListDto
{
    /// <summary>
    /// 付款方式 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 付款方式代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 付款方式名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否為預設付款方式
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 手續費率 (%)
    /// </summary>
    public decimal FeeRate { get; set; }
}

/// <summary>
/// 付款方式詳細資訊 DTO
/// </summary>
public class PaymentMethodDetailDto
{
    /// <summary>
    /// 付款方式 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 付款方式代碼
    /// </summary>
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
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 手續費率 (%)
    /// </summary>
    public decimal FeeRate { get; set; }

    /// <summary>
    /// 圖示 URL
    /// </summary>
    public string? IconUrl { get; set; }

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
/// 建立付款方式請求 DTO
/// </summary>
public class CreatePaymentMethodRequest
{
    /// <summary>
    /// 付款方式代碼
    /// </summary>
    [Required(ErrorMessage = "付款方式代碼為必填")]
    [StringLength(50, ErrorMessage = "付款方式代碼長度不可超過 50 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 付款方式名稱
    /// </summary>
    [Required(ErrorMessage = "付款方式名稱為必填")]
    [StringLength(100, ErrorMessage = "付款方式名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 說明描述
    /// </summary>
    [StringLength(500, ErrorMessage = "說明描述長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否為預設付款方式
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 手續費率 (%)
    /// </summary>
    [Range(0, 100, ErrorMessage = "手續費率需介於 0 到 100 之間")]
    public decimal FeeRate { get; set; } = 0;

    /// <summary>
    /// 圖示 URL
    /// </summary>
    [StringLength(500, ErrorMessage = "圖示 URL 長度不可超過 500 字元")]
    public string? IconUrl { get; set; }
}

/// <summary>
/// 更新付款方式請求 DTO
/// </summary>
public class UpdatePaymentMethodRequest
{
    /// <summary>
    /// 付款方式名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "付款方式名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 說明描述
    /// </summary>
    [StringLength(500, ErrorMessage = "說明描述長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否為預設付款方式
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 手續費率 (%)
    /// </summary>
    [Range(0, 100, ErrorMessage = "手續費率需介於 0 到 100 之間")]
    public decimal? FeeRate { get; set; }

    /// <summary>
    /// 圖示 URL
    /// </summary>
    [StringLength(500, ErrorMessage = "圖示 URL 長度不可超過 500 字元")]
    public string? IconUrl { get; set; }
}
