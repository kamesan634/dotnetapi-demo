using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購單實體
/// </summary>
/// <remarks>
/// 代表向供應商採購商品的訂單
/// </remarks>
public class PurchaseOrder
{
    /// <summary>
    /// 採購單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 採購單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PO202512310001"
    /// </remarks>
    public string PoNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 採購日期
    /// </summary>
    public DateOnly OrderDate { get; set; }

    /// <summary>
    /// 預計到貨日
    /// </summary>
    public DateOnly ExpectedDate { get; set; }

    /// <summary>
    /// 入庫倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 付款條件
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType TaxType { get; set; } = TaxType.Taxable;

    /// <summary>
    /// 採購人員 ID
    /// </summary>
    public int BuyerId { get; set; }

    /// <summary>
    /// 未稅金額
    /// </summary>
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// 含稅總額
    /// </summary>
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// 狀態
    /// </summary>
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    /// <summary>
    /// 核准人 ID
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// 核准時間
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 審核意見
    /// </summary>
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性

    /// <summary>
    /// 供應商
    /// </summary>
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// 入庫倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// 採購人員
    /// </summary>
    public virtual ApplicationUser Buyer { get; set; } = null!;

    /// <summary>
    /// 核准人
    /// </summary>
    public virtual ApplicationUser? Approver { get; set; }

    /// <summary>
    /// 採購明細列表
    /// </summary>
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

    /// <summary>
    /// 驗收單列表
    /// </summary>
    public virtual ICollection<PurchaseReceipt> Receipts { get; set; } = new List<PurchaseReceipt>();
}
