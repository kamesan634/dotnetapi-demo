namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存實體
/// </summary>
/// <remarks>
/// 記錄商品在各倉庫的庫存數量
/// </remarks>
public class Inventory
{
    /// <summary>
    /// 庫存 ID (主鍵)
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
    /// 現有庫存數量
    /// </summary>
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// 預留數量
    /// </summary>
    /// <remarks>
    /// 已被訂單預留但尚未出貨的數量
    /// </remarks>
    public int ReservedQuantity { get; set; } = 0;

    /// <summary>
    /// 可用庫存數量
    /// </summary>
    /// <remarks>
    /// 計算欄位：Quantity - ReservedQuantity
    /// </remarks>
    public int AvailableQuantity => Quantity - ReservedQuantity;

    /// <summary>
    /// 安全庫存量
    /// </summary>
    /// <remarks>
    /// 低於此數量會發出警示
    /// </remarks>
    public int SafetyStock { get; set; } = 0;

    /// <summary>
    /// 最後盤點日期
    /// </summary>
    public DateOnly? LastCountDate { get; set; }

    /// <summary>
    /// 最後異動日期
    /// </summary>
    public DateTime? LastMovementDate { get; set; }

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
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
