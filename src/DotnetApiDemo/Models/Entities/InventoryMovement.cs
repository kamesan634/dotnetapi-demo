using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存異動記錄實體
/// </summary>
/// <remarks>
/// 記錄所有庫存的進出異動
/// </remarks>
public class InventoryMovement
{
    /// <summary>
    /// 異動記錄 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 異動類型
    /// </summary>
    public InventoryMovementType MovementType { get; set; }

    /// <summary>
    /// 異動數量
    /// </summary>
    /// <remarks>
    /// 正數表示入庫，負數表示出庫
    /// </remarks>
    public int Quantity { get; set; }

    /// <summary>
    /// 異動前數量
    /// </summary>
    public int BeforeQuantity { get; set; }

    /// <summary>
    /// 異動後數量
    /// </summary>
    public int AfterQuantity { get; set; }

    /// <summary>
    /// 單位成本
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// 關聯單據類型
    /// </summary>
    /// <remarks>
    /// 如：Order、PurchaseOrder、Transfer 等
    /// </remarks>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// 關聯單據 ID
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// 關聯單據編號
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 建立者 ID
    /// </summary>
    public int? CreatedBy { get; set; }

    // 導航屬性

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// 建立者
    /// </summary>
    public virtual ApplicationUser? Creator { get; set; }
}
