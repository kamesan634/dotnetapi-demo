using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存調撥單實體
/// </summary>
/// <remarks>
/// 處理倉庫/門市之間的庫存移轉
/// </remarks>
public class StockTransfer
{
    /// <summary>
    /// 調撥單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 調撥單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "TR202512310001"
    /// </remarks>
    public string TransferNo { get; set; } = string.Empty;

    /// <summary>
    /// 來源倉庫 ID
    /// </summary>
    public int FromWarehouseId { get; set; }

    /// <summary>
    /// 目的倉庫 ID
    /// </summary>
    public int ToWarehouseId { get; set; }

    /// <summary>
    /// 申請日期
    /// </summary>
    public DateOnly RequestDate { get; set; }

    /// <summary>
    /// 預計出庫日
    /// </summary>
    public DateOnly? ExpectedShipDate { get; set; }

    /// <summary>
    /// 預計入庫日
    /// </summary>
    public DateOnly? ExpectedArrivalDate { get; set; }

    /// <summary>
    /// 實際出庫日
    /// </summary>
    public DateOnly? ActualShipDate { get; set; }

    /// <summary>
    /// 實際入庫日
    /// </summary>
    public DateOnly? ActualArrivalDate { get; set; }

    /// <summary>
    /// 總數量
    /// </summary>
    public int TotalQuantity { get; set; } = 0;

    /// <summary>
    /// 已出庫數量
    /// </summary>
    public int ShippedQuantity { get; set; } = 0;

    /// <summary>
    /// 已入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; } = 0;

    /// <summary>
    /// 調撥原因
    /// </summary>
    public string? TransferReason { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

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
    /// 來源倉庫
    /// </summary>
    public virtual Warehouse FromWarehouse { get; set; } = null!;

    /// <summary>
    /// 目的倉庫
    /// </summary>
    public virtual Warehouse ToWarehouse { get; set; } = null!;

    /// <summary>
    /// 申請人
    /// </summary>
    public virtual ApplicationUser Requester { get; set; } = null!;

    /// <summary>
    /// 核准人
    /// </summary>
    public virtual ApplicationUser? Approver { get; set; }

    /// <summary>
    /// 調撥明細列表
    /// </summary>
    public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
