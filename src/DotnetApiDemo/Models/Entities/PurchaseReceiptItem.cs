using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購驗收單明細實體
/// </summary>
/// <remarks>
/// 記錄驗收單中的各項商品驗收結果
/// </remarks>
public class PurchaseReceiptItem
{
    /// <summary>
    /// 驗收明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 驗收單 ID
    /// </summary>
    public int ReceiptId { get; set; }

    /// <summary>
    /// 採購單明細 ID
    /// </summary>
    public int PoItemId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 採購數量
    /// </summary>
    public int OrderedQuantity { get; set; }

    /// <summary>
    /// 之前已入庫數量
    /// </summary>
    public int PreviouslyReceived { get; set; }

    /// <summary>
    /// 待驗收數量
    /// </summary>
    public int PendingQuantity { get; set; }

    /// <summary>
    /// 本次到貨數量
    /// </summary>
    public int ArrivedQuantity { get; set; }

    /// <summary>
    /// 本次入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; }

    /// <summary>
    /// 驗退數量
    /// </summary>
    public int RejectedQuantity { get; set; } = 0;

    /// <summary>
    /// 驗退原因
    /// </summary>
    public ReturnReason? RejectionReason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 所屬驗收單
    /// </summary>
    public virtual PurchaseReceipt PurchaseReceipt { get; set; } = null!;

    /// <summary>
    /// 採購單明細
    /// </summary>
    public virtual PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
