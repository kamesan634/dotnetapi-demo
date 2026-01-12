using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Purchasing;

/// <summary>
/// 採購單列表 DTO
/// </summary>
public class PurchaseOrderListDto
{
    /// <summary>
    /// 採購單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 採購單號
    /// </summary>
    public string PoNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 入庫倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 狀態
    /// </summary>
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>
    /// 訂單日期
    /// </summary>
    public DateOnly OrderDate { get; set; }

    /// <summary>
    /// 預計交貨日期
    /// </summary>
    public DateOnly? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 品項數量
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>
/// 採購單詳細資訊 DTO
/// </summary>
public class PurchaseOrderDetailDto
{
    /// <summary>
    /// 採購單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 採購單號
    /// </summary>
    public string PoNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 入庫倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 入庫倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 狀態
    /// </summary>
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>
    /// 訂單日期
    /// </summary>
    public DateOnly OrderDate { get; set; }

    /// <summary>
    /// 預計交貨日期
    /// </summary>
    public DateOnly? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// 總數量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 採購明細
    /// </summary>
    public IEnumerable<PurchaseOrderItemDto> Items { get; set; } = Enumerable.Empty<PurchaseOrderItemDto>();

    /// <summary>
    /// 建立人員
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
}

/// <summary>
/// 採購單明細 DTO
/// </summary>
public class PurchaseOrderItemDto
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
    /// 訂購數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 已入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; }

    /// <summary>
    /// 待入庫數量
    /// </summary>
    public int PendingQuantity => Quantity - ReceivedQuantity;

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立採購單請求 DTO
/// </summary>
public class CreatePurchaseOrderRequest
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    [Required(ErrorMessage = "供應商為必填")]
    public int SupplierId { get; set; }

    /// <summary>
    /// 入庫倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "入庫倉庫為必填")]
    public int WarehouseId { get; set; }

    /// <summary>
    /// 預計交貨日期
    /// </summary>
    public DateOnly? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 採購明細
    /// </summary>
    [Required(ErrorMessage = "採購明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆採購明細")]
    public IEnumerable<CreatePurchaseOrderItemRequest> Items { get; set; } = Enumerable.Empty<CreatePurchaseOrderItemRequest>();
}

/// <summary>
/// 採購單明細請求 DTO
/// </summary>
public class CreatePurchaseOrderItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 訂購數量
    /// </summary>
    [Required(ErrorMessage = "訂購數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "訂購數量需大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Required(ErrorMessage = "單價為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "單價不可為負數")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 採購驗收單列表 DTO
/// </summary>
public class PurchaseReceiptListDto
{
    /// <summary>
    /// 驗收單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 驗收單號
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>
    /// 採購單號
    /// </summary>
    public string PoNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 驗收日期
    /// </summary>
    public DateOnly ReceiptDate { get; set; }

    /// <summary>
    /// 入庫總數量
    /// </summary>
    public int TotalReceivedQuantity { get; set; }

    /// <summary>
    /// 驗退總數量
    /// </summary>
    public int TotalRejectedQuantity { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 採購驗收單詳細 DTO
/// </summary>
public class PurchaseReceiptDetailDto : PurchaseReceiptListDto
{
    /// <summary>
    /// 採購單 ID
    /// </summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 驗收明細
    /// </summary>
    public IEnumerable<PurchaseReceiptItemDto> Items { get; set; } = Enumerable.Empty<PurchaseReceiptItemDto>();

    /// <summary>
    /// 驗收人員
    /// </summary>
    public string ReceivedByName { get; set; } = string.Empty;
}

/// <summary>
/// 採購驗收明細 DTO
/// </summary>
public class PurchaseReceiptItemDto
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
    /// 採購數量
    /// </summary>
    public int OrderedQuantity { get; set; }

    /// <summary>
    /// 本次到貨數量
    /// </summary>
    public int ArrivedQuantity { get; set; }

    /// <summary>
    /// 本次入庫數量
    /// </summary>
    public int ReceivedQuantity { get; set; }

    /// <summary>
    /// 驗退數量
    /// </summary>
    public int RejectedQuantity { get; set; }

    /// <summary>
    /// 驗退原因
    /// </summary>
    public ReturnReason? RejectionReason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立採購驗收單請求 DTO
/// </summary>
public class CreatePurchaseReceiptRequest
{
    /// <summary>
    /// 採購單 ID
    /// </summary>
    [Required(ErrorMessage = "採購單為必填")]
    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// 驗收日期
    /// </summary>
    public DateOnly? ReceiptDate { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 驗收明細
    /// </summary>
    [Required(ErrorMessage = "驗收明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆驗收明細")]
    public IEnumerable<CreatePurchaseReceiptItemRequest> Items { get; set; } = Enumerable.Empty<CreatePurchaseReceiptItemRequest>();
}

/// <summary>
/// 採購驗收明細請求 DTO
/// </summary>
public class CreatePurchaseReceiptItemRequest
{
    /// <summary>
    /// 採購單明細 ID
    /// </summary>
    [Required(ErrorMessage = "採購單明細為必填")]
    public int PoItemId { get; set; }

    /// <summary>
    /// 到貨數量
    /// </summary>
    [Required(ErrorMessage = "到貨數量為必填")]
    [Range(0, int.MaxValue, ErrorMessage = "到貨數量不可為負數")]
    public int ArrivedQuantity { get; set; }

    /// <summary>
    /// 入庫數量
    /// </summary>
    [Required(ErrorMessage = "入庫數量為必填")]
    [Range(0, int.MaxValue, ErrorMessage = "入庫數量不可為負數")]
    public int ReceivedQuantity { get; set; }

    /// <summary>
    /// 驗退數量
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "驗退數量不可為負數")]
    public int RejectedQuantity { get; set; } = 0;

    /// <summary>
    /// 驗退原因
    /// </summary>
    public ReturnReason? RejectionReason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 採購退貨單列表 DTO
/// </summary>
public class PurchaseReturnListDto
{
    /// <summary>
    /// 退貨單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單號
    /// </summary>
    public string ReturnNo { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 原採購單號
    /// </summary>
    public string? OriginalPoNo { get; set; }

    /// <summary>
    /// 退貨日期
    /// </summary>
    public DateOnly ReturnDate { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason ReturnReason { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public PurchaseReturnStatus Status { get; set; }

    /// <summary>
    /// 退貨總數量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 退貨總金額
    /// </summary>
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 採購退貨單詳細 DTO
/// </summary>
public class PurchaseReturnDetailDto : PurchaseReturnListDto
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 原入庫單號
    /// </summary>
    public string? OriginalReceiptNo { get; set; }

    /// <summary>
    /// 處理方式
    /// </summary>
    public HandlingMethod HandlingMethod { get; set; }

    /// <summary>
    /// 原因說明
    /// </summary>
    public string? ReasonNotes { get; set; }

    /// <summary>
    /// 退貨明細
    /// </summary>
    public IEnumerable<PurchaseReturnItemDto> Items { get; set; } = Enumerable.Empty<PurchaseReturnItemDto>();

    /// <summary>
    /// 申請人
    /// </summary>
    public string RequesterName { get; set; } = string.Empty;

    /// <summary>
    /// 核准人
    /// </summary>
    public string? ApproverName { get; set; }

    /// <summary>
    /// 核准時間
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 採購退貨明細 DTO
/// </summary>
public class PurchaseReturnItemDto
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
    /// 退貨數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立採購退貨單請求 DTO
/// </summary>
public class CreatePurchaseReturnRequest
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    [Required(ErrorMessage = "供應商為必填")]
    public int SupplierId { get; set; }

    /// <summary>
    /// 出庫倉庫 ID
    /// </summary>
    [Required(ErrorMessage = "出庫倉庫為必填")]
    public int WarehouseId { get; set; }

    /// <summary>
    /// 原採購單號
    /// </summary>
    [StringLength(50, ErrorMessage = "原採購單號長度不可超過 50 字元")]
    public string? OriginalPoNo { get; set; }

    /// <summary>
    /// 原入庫單號
    /// </summary>
    [StringLength(50, ErrorMessage = "原入庫單號長度不可超過 50 字元")]
    public string? OriginalReceiptNo { get; set; }

    /// <summary>
    /// 退貨日期
    /// </summary>
    public DateOnly? ReturnDate { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    [Required(ErrorMessage = "退貨原因為必填")]
    public ReturnReason ReturnReason { get; set; }

    /// <summary>
    /// 處理方式
    /// </summary>
    [Required(ErrorMessage = "處理方式為必填")]
    public HandlingMethod HandlingMethod { get; set; }

    /// <summary>
    /// 原因說明
    /// </summary>
    [StringLength(500, ErrorMessage = "原因說明長度不可超過 500 字元")]
    public string? ReasonNotes { get; set; }

    /// <summary>
    /// 退貨明細
    /// </summary>
    [Required(ErrorMessage = "退貨明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆退貨明細")]
    public IEnumerable<CreatePurchaseReturnItemRequest> Items { get; set; } = Enumerable.Empty<CreatePurchaseReturnItemRequest>();
}

/// <summary>
/// 採購退貨明細請求 DTO
/// </summary>
public class CreatePurchaseReturnItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 退貨數量
    /// </summary>
    [Required(ErrorMessage = "退貨數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "退貨數量需大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Required(ErrorMessage = "單價為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "單價不可為負數")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}
