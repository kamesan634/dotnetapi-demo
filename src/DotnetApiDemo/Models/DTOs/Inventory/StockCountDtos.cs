using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Inventory;

/// <summary>
/// 盤點單列表 DTO
/// </summary>
/// <remarks>
/// 用於列表顯示的盤點單摘要資訊
/// </remarks>
public class StockCountListDto
{
    /// <summary>
    /// 盤點單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 盤點單號
    /// </summary>
    public string CountNo { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 盤點類型
    /// </summary>
    public StockCountType CountType { get; set; }

    /// <summary>
    /// 盤點日期
    /// </summary>
    public DateOnly CountDate { get; set; }

    /// <summary>
    /// 總盤點項數
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// 已盤點項數
    /// </summary>
    public int CountedItems { get; set; }

    /// <summary>
    /// 差異項數
    /// </summary>
    public int VarianceItems { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public StockCountStatus Status { get; set; }

    /// <summary>
    /// 負責人名稱
    /// </summary>
    public string AssigneeName { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 盤點單詳細 DTO
/// </summary>
/// <remarks>
/// 包含盤點單的完整資訊及明細項目
/// </remarks>
public class StockCountDetailDto
{
    /// <summary>
    /// 盤點單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 盤點單號
    /// </summary>
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
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 盤點日期
    /// </summary>
    public DateOnly CountDate { get; set; }

    /// <summary>
    /// 盤點範圍設定
    /// </summary>
    public string? CountScope { get; set; }

    /// <summary>
    /// 是否凍結庫存
    /// </summary>
    public bool FreezeInventory { get; set; }

    /// <summary>
    /// 總盤點項數
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// 已盤點項數
    /// </summary>
    public int CountedItems { get; set; }

    /// <summary>
    /// 差異項數
    /// </summary>
    public int VarianceItems { get; set; }

    /// <summary>
    /// 差異金額
    /// </summary>
    public decimal VarianceAmount { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public StockCountStatus Status { get; set; }

    /// <summary>
    /// 負責人 ID
    /// </summary>
    public int AssignedTo { get; set; }

    /// <summary>
    /// 負責人名稱
    /// </summary>
    public string AssigneeName { get; set; } = string.Empty;

    /// <summary>
    /// 核准人名稱
    /// </summary>
    public string? ApproverName { get; set; }

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
    /// 建立人員名稱
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 盤點明細列表
    /// </summary>
    public IEnumerable<StockCountItemDto> Items { get; set; } = Enumerable.Empty<StockCountItemDto>();
}

/// <summary>
/// 盤點明細 DTO
/// </summary>
/// <remarks>
/// 盤點單中的各項商品盤點結果
/// </remarks>
public class StockCountItemDto
{
    /// <summary>
    /// 盤點明細 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 系統庫存數量
    /// </summary>
    public int SystemQuantity { get; set; }

    /// <summary>
    /// 實盤數量
    /// </summary>
    public int? CountedQuantity { get; set; }

    /// <summary>
    /// 差異數量
    /// </summary>
    public int? VarianceQuantity { get; set; }

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
    /// 盤點人員名稱
    /// </summary>
    public string? CountedByName { get; set; }

    /// <summary>
    /// 盤點時間
    /// </summary>
    public DateTime? CountedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立盤點單請求 DTO
/// </summary>
/// <remarks>
/// 用於建立新的盤點單
/// </remarks>
public class CreateStockCountRequest
{
    /// <summary>
    /// 盤點類型
    /// </summary>
    [Required(ErrorMessage = "盤點類型為必填")]
    public StockCountType CountType { get; set; }

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "倉庫為必填")]
    public int WarehouseId { get; set; }

    /// <summary>
    /// 盤點日期
    /// </summary>
    public DateOnly? CountDate { get; set; }

    /// <summary>
    /// 盤點範圍設定 (JSON 格式)
    /// </summary>
    [StringLength(1000, ErrorMessage = "盤點範圍設定長度不可超過 1000 字元")]
    public string? CountScope { get; set; }

    /// <summary>
    /// 是否凍結庫存
    /// </summary>
    public bool FreezeInventory { get; set; } = false;

    /// <summary>
    /// 負責人 ID
    /// </summary>
    [Required(ErrorMessage = "負責人為必填")]
    public int AssignedTo { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 盤點明細 (可選，若未提供則自動產生)
    /// </summary>
    public IEnumerable<CreateStockCountItemRequest>? Items { get; set; }
}

/// <summary>
/// 建立盤點明細請求 DTO
/// </summary>
/// <remarks>
/// 用於指定盤點的特定商品
/// </remarks>
public class CreateStockCountItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 更新盤點明細請求 DTO
/// </summary>
/// <remarks>
/// 用於更新盤點結果 (實盤數量、差異原因等)
/// </remarks>
public class UpdateStockCountItemRequest
{
    /// <summary>
    /// 實盤數量
    /// </summary>
    [Required(ErrorMessage = "實盤數量為必填")]
    [Range(0, int.MaxValue, ErrorMessage = "實盤數量不可為負數")]
    public int CountedQuantity { get; set; }

    /// <summary>
    /// 差異原因
    /// </summary>
    public AdjustmentReason? VarianceReason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}
