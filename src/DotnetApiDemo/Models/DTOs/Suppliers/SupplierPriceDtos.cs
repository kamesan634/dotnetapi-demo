using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Suppliers;

/// <summary>
/// 供應商報價列表 DTO
/// </summary>
public class SupplierPriceListDto
{
    /// <summary>
    /// 報價 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 供應商料號
    /// </summary>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 生效日期
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// 是否為主要供應商
    /// </summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// 供應商報價詳細 DTO
/// </summary>
public class SupplierPriceDetailDto
{
    /// <summary>
    /// 報價 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 供應商代碼
    /// </summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 供應商料號
    /// </summary>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 包裝規格
    /// </summary>
    public int? PackSize { get; set; }

    /// <summary>
    /// 前置天數
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// 生效日期
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// 是否為主要供應商
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

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
/// 建立供應商報價請求 DTO
/// </summary>
public class CreateSupplierPriceRequest
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    [Required(ErrorMessage = "供應商為必填")]
    public int SupplierId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 供應商料號
    /// </summary>
    [StringLength(50, ErrorMessage = "供應商料號長度不可超過 50 字元")]
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Required(ErrorMessage = "單價為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "單價不可為負數")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    [StringLength(10, ErrorMessage = "幣別長度不可超過 10 字元")]
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 最低訂購量
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "最低訂購量需大於 0")]
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 包裝規格
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "包裝規格需大於 0")]
    public int? PackSize { get; set; }

    /// <summary>
    /// 前置天數
    /// </summary>
    [Range(0, 365, ErrorMessage = "前置天數需在 0-365 天之間")]
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// 生效日期
    /// </summary>
    [Required(ErrorMessage = "生效日期為必填")]
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// 是否為主要供應商
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 更新供應商報價請求 DTO
/// </summary>
public class UpdateSupplierPriceRequest
{
    /// <summary>
    /// 供應商料號
    /// </summary>
    [StringLength(50, ErrorMessage = "供應商料號長度不可超過 50 字元")]
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "單價不可為負數")]
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    [StringLength(10, ErrorMessage = "幣別長度不可超過 10 字元")]
    public string? Currency { get; set; }

    /// <summary>
    /// 最低訂購量
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "最低訂購量需大於 0")]
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 包裝規格
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "包裝規格需大於 0")]
    public int? PackSize { get; set; }

    /// <summary>
    /// 前置天數
    /// </summary>
    [Range(0, 365, ErrorMessage = "前置天數需在 0-365 天之間")]
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// 生效日期
    /// </summary>
    public DateOnly? EffectiveDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// 是否為主要供應商
    /// </summary>
    public bool? IsPrimary { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}
