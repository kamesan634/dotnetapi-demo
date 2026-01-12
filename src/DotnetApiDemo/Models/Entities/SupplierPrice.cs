namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 供應商商品價格實體
/// </summary>
/// <remarks>
/// 記錄各供應商對商品的報價
/// </remarks>
public class SupplierPrice
{
    /// <summary>
    /// 供應商價格 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 供應商料號
    /// </summary>
    /// <remarks>
    /// 供應商的商品編號
    /// </remarks>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 包裝規格
    /// </summary>
    /// <remarks>
    /// 每箱/每包的數量
    /// </remarks>
    public int? PackSize { get; set; }

    /// <summary>
    /// 前置天數
    /// </summary>
    /// <remarks>
    /// 交貨所需天數
    /// </remarks>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// 生效日期
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// 失效日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// 是否為主要供應商
    /// </summary>
    /// <remarks>
    /// 此商品的主要進貨來源
    /// </remarks>
    public bool IsPrimary { get; set; } = false;

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
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
