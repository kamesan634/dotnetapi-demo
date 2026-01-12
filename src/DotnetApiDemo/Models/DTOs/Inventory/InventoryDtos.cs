using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Inventory;

/// <summary>
/// 庫存列表 DTO
/// </summary>
public class InventoryListDto
{
    /// <summary>
    /// 庫存 ID
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
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 庫存數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 保留數量
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// 可用數量
    /// </summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;

    /// <summary>
    /// 安全庫存量
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 是否低於安全庫存
    /// </summary>
    public bool IsBelowSafetyStock => Quantity < SafetyStock;
}

/// <summary>
/// 庫存異動記錄 DTO
/// </summary>
public class InventoryMovementDto
{
    /// <summary>
    /// 異動 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 異動類型
    /// </summary>
    public InventoryMovementType MovementType { get; set; }

    /// <summary>
    /// 異動數量
    /// </summary>
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
    /// 來源單號
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 異動時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 操作人員
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// 庫存調整請求 DTO
/// </summary>
public class StockAdjustmentRequest
{
    /// <summary>
    /// 倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "倉庫為必填")]
    public int WarehouseId { get; set; }

    /// <summary>
    /// 調整原因
    /// </summary>
    [Required(ErrorMessage = "調整原因為必填")]
    public AdjustmentReason Reason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 調整明細
    /// </summary>
    [Required(ErrorMessage = "調整明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆調整明細")]
    public IEnumerable<StockAdjustmentItemRequest> Items { get; set; } = Enumerable.Empty<StockAdjustmentItemRequest>();
}

/// <summary>
/// 庫存調整明細請求 DTO
/// </summary>
public class StockAdjustmentItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 調整數量 (正數增加，負數減少)
    /// </summary>
    [Required(ErrorMessage = "調整數量為必填")]
    public int AdjustQuantity { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 庫存調撥單 DTO
/// </summary>
public class StockTransferDto
{
    /// <summary>
    /// 調撥單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 調撥單號
    /// </summary>
    public string TransferNo { get; set; } = string.Empty;

    /// <summary>
    /// 來源倉庫名稱
    /// </summary>
    public string FromWarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 目的倉庫名稱
    /// </summary>
    public string ToWarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 狀態
    /// </summary>
    public StockTransferStatus Status { get; set; }

    /// <summary>
    /// 調撥日期
    /// </summary>
    public DateOnly TransferDate { get; set; }

    /// <summary>
    /// 總數量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 庫存調撥單詳細 DTO
/// </summary>
public class StockTransferDetailDto : StockTransferDto
{
    /// <summary>
    /// 來源倉庫 ID
    /// </summary>
    public int FromWarehouseId { get; set; }

    /// <summary>
    /// 目的倉庫 ID
    /// </summary>
    public int ToWarehouseId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 調撥明細
    /// </summary>
    public IEnumerable<StockTransferItemDto> Items { get; set; } = Enumerable.Empty<StockTransferItemDto>();

    /// <summary>
    /// 建立人員
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 庫存調撥明細 DTO
/// </summary>
public class StockTransferItemDto
{
    /// <summary>
    /// 明細 ID
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
    /// 調撥數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立庫存調撥單請求 DTO
/// </summary>
public class CreateStockTransferRequest
{
    /// <summary>
    /// 來源倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "來源倉庫為必填")]
    public int FromWarehouseId { get; set; }

    /// <summary>
    /// 目的倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "目的倉庫為必填")]
    public int ToWarehouseId { get; set; }

    /// <summary>
    /// 調撥日期
    /// </summary>
    public DateOnly? TransferDate { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 調撥明細
    /// </summary>
    [Required(ErrorMessage = "調撥明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆調撥明細")]
    public IEnumerable<CreateStockTransferItemRequest> Items { get; set; } = Enumerable.Empty<CreateStockTransferItemRequest>();
}

/// <summary>
/// 庫存調撥明細請求 DTO
/// </summary>
public class CreateStockTransferItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 調撥數量
    /// </summary>
    [Required(ErrorMessage = "調撥數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "調撥數量需大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 庫存盤點單 DTO
/// </summary>
public class StockCountDto
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
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 盤點類型
    /// </summary>
    public StockCountType CountType { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public StockCountStatus Status { get; set; }

    /// <summary>
    /// 盤點日期
    /// </summary>
    public DateOnly CountDate { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 低庫存警示 DTO
/// </summary>
public class LowStockAlertDto
{
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
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 目前庫存
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// 安全庫存量
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 缺口數量
    /// </summary>
    public int Shortage => SafetyStock - CurrentStock;
}

/// <summary>
/// 補貨建議 DTO
/// </summary>
public class ReplenishmentSuggestionDto
{
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int SafetyStock { get; set; }
    public int SuggestedQuantity { get; set; }
    public int? PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
    public decimal? LastPurchasePrice { get; set; }
}
