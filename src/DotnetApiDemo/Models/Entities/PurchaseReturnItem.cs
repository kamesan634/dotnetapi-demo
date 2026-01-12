namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 採購退貨單明細實體
/// </summary>
/// <remarks>
/// 記錄退貨單中的各項商品
/// </remarks>
public class PurchaseReturnItem
{
    /// <summary>
    /// 退貨明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單 ID
    /// </summary>
    public int ReturnId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 退貨數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 所屬退貨單
    /// </summary>
    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
