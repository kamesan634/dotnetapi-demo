using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.SalesReturns;

/// <summary>
/// 退貨單列表 DTO
/// </summary>
public class SalesReturnListDto
{
    /// <summary>
    /// 退貨單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單號
    /// </summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>
    /// 原訂單號
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// 退貨狀態
    /// </summary>
    public SalesReturnStatus Status { get; set; }

    /// <summary>
    /// 退貨總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 是否已退款
    /// </summary>
    public bool IsRefunded { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 退貨單詳細資訊 DTO
/// </summary>
public class SalesReturnDetailDto
{
    /// <summary>
    /// 退貨單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單號
    /// </summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>
    /// 原訂單 ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 原訂單號
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// 退貨原因說明
    /// </summary>
    public string? ReasonDescription { get; set; }

    /// <summary>
    /// 退貨狀態
    /// </summary>
    public SalesReturnStatus Status { get; set; }

    /// <summary>
    /// 退貨總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 退款金額
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 是否已退款
    /// </summary>
    public bool IsRefunded { get; set; }

    /// <summary>
    /// 退款日期
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// 退款方式
    /// </summary>
    public string? RefundMethod { get; set; }

    /// <summary>
    /// 處理人員名稱
    /// </summary>
    public string? ProcessedByName { get; set; }

    /// <summary>
    /// 處理日期
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 退貨明細
    /// </summary>
    public IEnumerable<SalesReturnItemDto> Items { get; set; } = Enumerable.Empty<SalesReturnItemDto>();
}

/// <summary>
/// 退貨明細 DTO
/// </summary>
public class SalesReturnItemDto
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
    /// 商品 SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

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
    /// 小計
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立退貨單請求 DTO
/// </summary>
public class CreateSalesReturnRequest
{
    /// <summary>
    /// 原訂單 ID
    /// </summary>
    [Required(ErrorMessage = "原訂單 ID 為必填")]
    public int OrderId { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    [Required(ErrorMessage = "退貨原因為必填")]
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// 退貨原因說明
    /// </summary>
    [StringLength(500, ErrorMessage = "退貨原因說明長度不可超過 500 字元")]
    public string? ReasonDescription { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(1000, ErrorMessage = "備註長度不可超過 1000 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 退貨明細
    /// </summary>
    [Required(ErrorMessage = "退貨明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆退貨明細")]
    public List<CreateSalesReturnItemRequest> Items { get; set; } = new();
}

/// <summary>
/// 建立退貨明細請求 DTO
/// </summary>
public class CreateSalesReturnItemRequest
{
    /// <summary>
    /// 原訂單明細 ID
    /// </summary>
    [Required(ErrorMessage = "原訂單明細 ID 為必填")]
    public int OrderItemId { get; set; }

    /// <summary>
    /// 退貨數量
    /// </summary>
    [Required(ErrorMessage = "退貨數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "退貨數量必須大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 退貨原因
    /// </summary>
    public ReturnReason? Reason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 處理退貨請求 DTO
/// </summary>
public class ProcessSalesReturnRequest
{
    /// <summary>
    /// 核准或拒絕
    /// </summary>
    [Required(ErrorMessage = "請指定核准或拒絕")]
    public bool Approve { get; set; }

    /// <summary>
    /// 處理備註
    /// </summary>
    [StringLength(500, ErrorMessage = "處理備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 退款請求 DTO
/// </summary>
public class RefundRequest
{
    /// <summary>
    /// 退款金額
    /// </summary>
    [Required(ErrorMessage = "退款金額為必填")]
    [Range(0.01, double.MaxValue, ErrorMessage = "退款金額必須大於 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 退款方式
    /// </summary>
    [Required(ErrorMessage = "退款方式為必填")]
    [StringLength(50, ErrorMessage = "退款方式長度不可超過 50 字元")]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}
