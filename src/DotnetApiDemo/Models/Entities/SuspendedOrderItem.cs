namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 掛單商品明細實體
/// </summary>
public class SuspendedOrderItem
{
    /// <summary>
    /// 明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 掛單 ID
    /// </summary>
    public int SuspendedOrderId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品規格 ID (可選)
    /// </summary>
    public int? VariantId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 規格名稱
    /// </summary>
    public string? VariantName { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 原價
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 掛單
    /// </summary>
    public virtual SuspendedOrder SuspendedOrder { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 商品規格
    /// </summary>
    public virtual ProductVariant? Variant { get; set; }
}
