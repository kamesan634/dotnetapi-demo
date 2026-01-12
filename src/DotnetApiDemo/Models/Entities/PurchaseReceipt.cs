using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購驗收單實體
/// </summary>
/// <remarks>
/// 依據採購單執行進貨驗收入庫作業
/// </remarks>
public class PurchaseReceipt
{
    /// <summary>
    /// 驗收單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 驗收單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "GR202512310001"
    /// </remarks>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// 採購單 ID
    /// </summary>
    public int PoId { get; set; }

    /// <summary>
    /// 驗收日期
    /// </summary>
    public DateOnly ReceiptDate { get; set; }

    /// <summary>
    /// 入庫倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 送貨單號
    /// </summary>
    /// <remarks>
    /// 供應商送貨單號
    /// </remarks>
    public string? DeliveryNo { get; set; }

    /// <summary>
    /// 驗收人員 ID
    /// </summary>
    public int ReceiverId { get; set; }

    /// <summary>
    /// 到貨總數量
    /// </summary>
    public int TotalQuantity { get; set; } = 0;

    /// <summary>
    /// 已入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; } = 0;

    /// <summary>
    /// 驗退數量
    /// </summary>
    public int RejectedQuantity { get; set; } = 0;

    /// <summary>
    /// 狀態
    /// </summary>
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 完成時間
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // 導航屬性

    /// <summary>
    /// 採購單
    /// </summary>
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>
    /// 入庫倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// 驗收人員
    /// </summary>
    public virtual ApplicationUser Receiver { get; set; } = null!;

    /// <summary>
    /// 驗收明細列表
    /// </summary>
    public virtual ICollection<PurchaseReceiptItem> Items { get; set; } = new List<PurchaseReceiptItem>();
}
