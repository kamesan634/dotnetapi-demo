using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 商品實體
/// </summary>
/// <remarks>
/// 代表可銷售的商品項目
/// </remarks>
public class Product
{
    /// <summary>
    /// 商品 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品編號 (SKU)
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PRD001"
    /// </remarks>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品簡稱
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分類 ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// 規格型號
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 計量單位 ID
    /// </summary>
    public int UnitId { get; set; }

    /// <summary>
    /// 成本價
    /// </summary>
    /// <remarks>
    /// 移動平均成本
    /// </remarks>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 定價 (原價)
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// 售價 (實際銷售價)
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// 會員價
    /// </summary>
    public decimal? MemberPrice { get; set; }

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType TaxType { get; set; } = TaxType.Taxable;

    /// <summary>
    /// 稅率 (%)
    /// </summary>
    /// <remarks>
    /// 如：5 表示 5% 營業稅
    /// </remarks>
    public decimal TaxRate { get; set; } = 5;

    /// <summary>
    /// 安全庫存量
    /// </summary>
    /// <remarks>
    /// 低於此數量會發出警示
    /// </remarks>
    public int SafetyStock { get; set; } = 0;

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// 主要供應商 ID
    /// </summary>
    public int? PrimarySupplierId { get; set; }

    /// <summary>
    /// 商品圖片 URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 是否可銷售
    /// </summary>
    public bool IsSellable { get; set; } = true;

    /// <summary>
    /// 是否可採購
    /// </summary>
    public bool IsPurchasable { get; set; } = true;

    /// <summary>
    /// 是否管控庫存
    /// </summary>
    /// <remarks>
    /// 不管控庫存的商品不會追蹤庫存數量
    /// </remarks>
    public bool TrackInventory { get; set; } = true;

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
    /// 所屬分類
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// 計量單位
    /// </summary>
    public virtual Unit Unit { get; set; } = null!;

    /// <summary>
    /// 主要供應商
    /// </summary>
    public virtual Supplier? PrimarySupplier { get; set; }

    /// <summary>
    /// 商品條碼列表
    /// </summary>
    public virtual ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();

    /// <summary>
    /// 商品庫存列表
    /// </summary>
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    /// <summary>
    /// 供應商價格列表
    /// </summary>
    public virtual ICollection<SupplierPrice> SupplierPrices { get; set; } = new List<SupplierPrice>();
}
