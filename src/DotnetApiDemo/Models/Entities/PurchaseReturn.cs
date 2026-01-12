using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購退貨單實體
/// </summary>
/// <remarks>
/// 處理向供應商退貨的作業
/// </remarks>
public class PurchaseReturn
{
    /// <summary>
    /// 退貨單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PR202512310001"
    /// </remarks>
    public string ReturnNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 原採購單號
    /// </summary>
    public string? OriginalPoNo { get; set; }

    /// <summary>
    /// 原入庫單號
    /// </summary>
    public string? OriginalReceiptNo { get; set; }

    /// <summary>
    /// 退貨日期
    /// </summary>
    public DateOnly ReturnDate { get; set; }

    /// <summary>
    /// 出庫倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason ReturnReason { get; set; }

    /// <summary>
    /// 處理方式
    /// </summary>
    public HandlingMethod HandlingMethod { get; set; }

    /// <summary>
    /// 退貨總數量
    /// </summary>
    public int TotalQuantity { get; set; } = 0;

    /// <summary>
    /// 退貨總金額
    /// </summary>
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// 原因說明
    /// </summary>
    public string? ReasonNotes { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public PurchaseReturnStatus Status { get; set; } = PurchaseReturnStatus.Draft;

    /// <summary>
    /// 申請人 ID
    /// </summary>
    public int RequestedBy { get; set; }

    /// <summary>
    /// 核准人 ID
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// 核准時間
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

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
    /// 出庫倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// 申請人
    /// </summary>
    public virtual ApplicationUser Requester { get; set; } = null!;

    /// <summary>
    /// 核准人
    /// </summary>
    public virtual ApplicationUser? Approver { get; set; }

    /// <summary>
    /// 退貨明細列表
    /// </summary>
    public virtual ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
