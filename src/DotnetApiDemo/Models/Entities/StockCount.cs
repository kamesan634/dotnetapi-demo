using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存盤點單實體
/// </summary>
/// <remarks>
/// 執行定期或不定期的庫存盤點作業
/// </remarks>
public class StockCount
{
    /// <summary>
    /// 盤點單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 盤點單號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "IV202512310001"
    /// </remarks>
    public string CountNo { get; set; } = string.Empty;

    /// <summary>
    /// 盤點類型
    /// </summary>
    public StockCountType CountType { get; set; }

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 盤點日期
    /// </summary>
    public DateOnly CountDate { get; set; }

    /// <summary>
    /// 盤點範圍設定
    /// </summary>
    /// <remarks>
    /// JSON 格式，可設定分類、商品範圍等
    /// </remarks>
    public string? CountScope { get; set; }

    /// <summary>
    /// 是否凍結庫存
    /// </summary>
    /// <remarks>
    /// 凍結期間暫停該範圍庫存異動
    /// </remarks>
    public bool FreezeInventory { get; set; } = false;

    /// <summary>
    /// 總盤點項數
    /// </summary>
    public int TotalItems { get; set; } = 0;

    /// <summary>
    /// 已盤點項數
    /// </summary>
    public int CountedItems { get; set; } = 0;

    /// <summary>
    /// 差異項數
    /// </summary>
    public int VarianceItems { get; set; } = 0;

    /// <summary>
    /// 差異金額
    /// </summary>
    public decimal VarianceAmount { get; set; } = 0;

    /// <summary>
    /// 狀態
    /// </summary>
    public StockCountStatus Status { get; set; } = StockCountStatus.Draft;

    /// <summary>
    /// 負責人 ID
    /// </summary>
    public int AssignedTo { get; set; }

    /// <summary>
    /// 核准人 ID
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// 核准時間
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 完成時間
    /// </summary>
    public DateTime? CompletedAt { get; set; }

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
    public int CreatedBy { get; set; }

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
    /// 負責人
    /// </summary>
    public virtual ApplicationUser Assignee { get; set; } = null!;

    /// <summary>
    /// 核准人
    /// </summary>
    public virtual ApplicationUser? Approver { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    public virtual ApplicationUser Creator { get; set; } = null!;

    /// <summary>
    /// 盤點明細列表
    /// </summary>
    public virtual ICollection<StockCountItem> Items { get; set; } = new List<StockCountItem>();
}
