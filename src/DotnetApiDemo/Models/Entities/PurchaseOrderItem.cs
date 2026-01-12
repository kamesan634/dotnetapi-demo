namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購單明細實體
/// </summary>
/// <remarks>
/// 記錄採購單中的各項商品
/// </remarks>
public class PurchaseOrderItem
{
    /// <summary>
    /// 採購明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 採購單 ID
    /// </summary>
    public int PoId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 採購數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// 小計 (含稅)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 已入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; } = 0;

    /// <summary>
    /// 待入庫數量
    /// </summary>
    /// <remarks>
    /// 計算：Quantity - ReceivedQuantity
    /// </remarks>
    public int PendingQuantity => Quantity - ReceivedQuantity;

    /// <summary>
    /// 供應商料號
    /// </summary>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 所屬採購單
    /// </summary>
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
