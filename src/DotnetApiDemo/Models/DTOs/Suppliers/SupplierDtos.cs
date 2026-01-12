using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Suppliers;

/// <summary>
/// 供應商列表 DTO
/// </summary>
public class SupplierListDto
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 聯絡人
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 付款條件
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 供應商詳細資訊 DTO
/// </summary>
public class SupplierDetailDto
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 聯絡人
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    public string? BankAccount { get; set; }

    /// <summary>
    /// 付款條件
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 供應商品項列表
    /// </summary>
    public IEnumerable<SupplierProductDto> Products { get; set; } = Enumerable.Empty<SupplierProductDto>();

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
/// 供應商商品 DTO
/// </summary>
public class SupplierProductDto
{
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
    /// 供應商商品編號
    /// </summary>
    public string? SupplierProductCode { get; set; }

    /// <summary>
    /// 進貨價格
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int MinOrderQuantity { get; set; }

    /// <summary>
    /// 前置時間 (天)
    /// </summary>
    public int LeadTimeDays { get; set; }
}

/// <summary>
/// 建立供應商請求 DTO
/// </summary>
public class CreateSupplierRequest
{
    /// <summary>
    /// 供應商代碼
    /// </summary>
    [Required(ErrorMessage = "供應商代碼為必填")]
    [StringLength(20, ErrorMessage = "供應商代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    [Required(ErrorMessage = "供應商名稱為必填")]
    [StringLength(100, ErrorMessage = "供應商名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 聯絡人
    /// </summary>
    [StringLength(50, ErrorMessage = "聯絡人長度不可超過 50 字元")]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    [StringLength(20, ErrorMessage = "統一編號長度不可超過 20 字元")]
    public string? TaxId { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    [StringLength(50, ErrorMessage = "銀行帳號長度不可超過 50 字元")]
    public string? BankAccount { get; set; }

    /// <summary>
    /// 付款條件
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 更新供應商請求 DTO
/// </summary>
public class UpdateSupplierRequest
{
    /// <summary>
    /// 供應商名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "供應商名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 聯絡人
    /// </summary>
    [StringLength(50, ErrorMessage = "聯絡人長度不可超過 50 字元")]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    [StringLength(20, ErrorMessage = "統一編號長度不可超過 20 字元")]
    public string? TaxId { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    [StringLength(50, ErrorMessage = "銀行帳號長度不可超過 50 字元")]
    public string? BankAccount { get; set; }

    /// <summary>
    /// 付款條件
    /// </summary>
    public PaymentTerms? PaymentTerms { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 供應商價格設定請求 DTO
/// </summary>
public class SupplierPriceRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 供應商商品編號
    /// </summary>
    [StringLength(50, ErrorMessage = "供應商商品編號長度不可超過 50 字元")]
    public string? SupplierProductCode { get; set; }

    /// <summary>
    /// 進貨價格
    /// </summary>
    [Required(ErrorMessage = "進貨價格為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "進貨價格不可為負數")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// 最低訂購量
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "最低訂購量需大於 0")]
    public int MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// 前置時間 (天)
    /// </summary>
    [Range(0, 365, ErrorMessage = "前置時間需在 0-365 天之間")]
    public int LeadTimeDays { get; set; } = 0;
}
