using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存調整單實體
/// </summary>
/// <remarks>
/// 手動調整庫存數量，處理盤點差異或特殊狀況
/// </remarks>
public class StockAdjustment
{
    /// <summary>
    /// 調整單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 調整單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "ADJ202512310001"
    /// </remarks>
    public string AdjustmentNo { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 調整日期
    /// </summary>
    public DateOnly AdjustmentDate { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 調整前數量
    /// </summary>
    public int BeforeQuantity { get; set; }

    /// <summary>
    /// 調整後數量
    /// </summary>
    public int AfterQuantity { get; set; }

    /// <summary>
    /// 調整數量
    /// </summary>
    /// <remarks>
    /// 正數為增加，負數為減少
    /// </remarks>
    public int AdjustmentQuantity { get; set; }

    /// <summary>
    /// 調整原因
    /// </summary>
    public AdjustmentReason AdjustmentReason { get; set; }

    /// <summary>
    /// 原因說明
    /// </summary>
    public string ReasonNotes { get; set; } = string.Empty;

    /// <summary>
    /// 狀態
    /// </summary>
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

    /// <summary>
    /// 申請人 ID
    /// </summary>
    public int RequestedBy { get; set; }

    /// <summary>
    /// 核准人 ID
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// 核准時間
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

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
    /// 倉庫
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 申請人
    /// </summary>
    public virtual ApplicationUser Requester { get; set; } = null!;

    /// <summary>
    /// 核准人
    /// </summary>
    public virtual ApplicationUser? Approver { get; set; }
}
