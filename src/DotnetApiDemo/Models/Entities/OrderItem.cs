using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 訂單明細實體
/// </summary>
/// <remarks>
/// 代表訂單中的一項商品
/// </remarks>
public class OrderItem
{
    /// <summary>
    /// 訂單明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號 (SKU)
    /// </summary>
    /// <remarks>
    /// 冗餘欄位，記錄交易當時的商品編號
    /// </remarks>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    /// <remarks>
    /// 冗餘欄位，記錄交易當時的商品名稱
    /// </remarks>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 單價 (定價)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 實際售價
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// 成本價
    /// </summary>
    /// <remarks>
    /// 記錄交易當時的成本價，用於計算毛利
    /// </remarks>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType TaxType { get; set; }

    /// <summary>
    /// 稅率 (%)
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 小計 (含稅)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 出貨倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 退貨數量
    /// </summary>
    public int ReturnedQuantity { get; set; } = 0;

    /// <summary>
    /// 備註
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 導航屬性

    /// <summary>
    /// 所屬訂單
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 出貨倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
