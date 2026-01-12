using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存盤點單明細實體
/// </summary>
/// <remarks>
/// 記錄盤點單中的各項商品盤點結果
/// </remarks>
public class StockCountItem
{
    /// <summary>
    /// 盤點明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 盤點單 ID
    /// </summary>
    public int CountId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 系統庫存數量
    /// </summary>
    /// <remarks>
    /// 盤點時的帳面庫存
    /// </remarks>
    public int SystemQuantity { get; set; }

    /// <summary>
    /// 實盤數量
    /// </summary>
    public int? CountedQuantity { get; set; }

    /// <summary>
    /// 差異數量
    /// </summary>
    /// <remarks>
    /// 計算：CountedQuantity - SystemQuantity
    /// </remarks>
    public int? VarianceQuantity => CountedQuantity.HasValue
        ? CountedQuantity.Value - SystemQuantity
        : null;

    /// <summary>
    /// 單位成本
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// 差異金額
    /// </summary>
    public decimal? VarianceAmount { get; set; }

    /// <summary>
    /// 差異原因
    /// </summary>
    public AdjustmentReason? VarianceReason { get; set; }

    /// <summary>
    /// 盤點人員 ID
    /// </summary>
    public int? CountedBy { get; set; }

    /// <summary>
    /// 盤點時間
    /// </summary>
    public DateTime? CountedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 所屬盤點單
    /// </summary>
    public virtual StockCount StockCount { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 盤點人員
    /// </summary>
    public virtual ApplicationUser? Counter { get; set; }
}
