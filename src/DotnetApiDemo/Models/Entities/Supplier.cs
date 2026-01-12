using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 供應商實體
/// </summary>
/// <remarks>
/// 代表商品的供應來源
/// </remarks>
public class Supplier
{
    /// <summary>
    /// 供應商 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "SUP001"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 供應商簡稱
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// 聯絡人
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 聯絡電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 傳真
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 網站
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 預設付款條件
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    /// <summary>
    /// 預設交貨天數
    /// </summary>
    public int LeadTimeDays { get; set; } = 7;

    /// <summary>
    /// 銀行名稱
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    public string? BankAccount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

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

    // 導航屬性

    /// <summary>
    /// 供應商價格列表
    /// </summary>
    public virtual ICollection<SupplierPrice> SupplierPrices { get; set; } = new List<SupplierPrice>();

    /// <summary>
    /// 採購單列表
    /// </summary>
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
