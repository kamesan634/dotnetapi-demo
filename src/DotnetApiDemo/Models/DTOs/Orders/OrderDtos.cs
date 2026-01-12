using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Orders;

/// <summary>
/// 訂單列表 DTO
/// </summary>
public class OrderListDto
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 訂單編號
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 訂單總額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 應付金額
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// 訂單日期
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// 商品數量
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>
/// 訂單詳細資訊 DTO
/// </summary>
public class OrderDetailDto
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 訂單編號
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 會員編號
    /// </summary>
    public string? MemberNo { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 訂單小計
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 訂單總額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 應付金額
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// 已付金額
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// 待付金額
    /// </summary>
    public decimal DueAmount => FinalAmount - PaidAmount;

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 訂單明細
    /// </summary>
    public IEnumerable<OrderItemDto> Items { get; set; } = Enumerable.Empty<OrderItemDto>();

    /// <summary>
    /// 付款記錄
    /// </summary>
    public IEnumerable<PaymentDto> Payments { get; set; } = Enumerable.Empty<PaymentDto>();

    /// <summary>
    /// 訂單日期
    /// </summary>
    public DateTime OrderDate { get; set; }

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
/// 訂單明細 DTO
/// </summary>
public class OrderItemDto
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
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

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
/// 付款記錄 DTO
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// 付款 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 付款方式
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 付款金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 付款狀態
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// 付款時間
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 交易編號
    /// </summary>
    public string? TransactionNo { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立訂單請求 DTO
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// 門市 ID
    /// </summary>
    [Required(ErrorMessage = "門市為必填")]
    public int StoreId { get; set; }

    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 訂單明細
    /// </summary>
    [Required(ErrorMessage = "訂單明細為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆訂單明細")]
    public IEnumerable<CreateOrderItemRequest> Items { get; set; } = Enumerable.Empty<CreateOrderItemRequest>();
}

/// <summary>
/// 建立訂單明細請求 DTO
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    [Required(ErrorMessage = "數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "數量需大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單價 (可選，未指定則使用商品售價)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "單價不可為負數")]
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "折扣金額不可為負數")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 新增付款請求 DTO
/// </summary>
public class AddPaymentRequest
{
    /// <summary>
    /// 付款方式
    /// </summary>
    [Required(ErrorMessage = "付款方式為必填")]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 付款金額
    /// </summary>
    [Required(ErrorMessage = "付款金額為必填")]
    [Range(0.01, double.MaxValue, ErrorMessage = "付款金額需大於 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 交易編號
    /// </summary>
    [StringLength(50, ErrorMessage = "交易編號長度不可超過 50 字元")]
    public string? TransactionNo { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200, ErrorMessage = "備註長度不可超過 200 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 促銷活動列表 DTO
/// </summary>
public class PromotionListDto
{
    /// <summary>
    /// 促銷 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 促銷代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 促銷名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 促銷類型
    /// </summary>
    public PromotionType PromotionType { get; set; }

    /// <summary>
    /// 折扣值
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public PromotionStatus Status { get; set; }

    /// <summary>
    /// 是否進行中
    /// </summary>
    public bool IsActive => Status == PromotionStatus.Active &&
                           DateTime.UtcNow >= StartDate &&
                           DateTime.UtcNow <= EndDate;
}

/// <summary>
/// 促銷活動詳細 DTO
/// </summary>
public class PromotionDetailDto : PromotionListDto
{
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    public decimal? MinPurchaseAmount { get; set; }

    /// <summary>
    /// 最大折扣金額
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// 已使用次數
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 建立促銷活動請求 DTO
/// </summary>
public class CreatePromotionRequest
{
    /// <summary>
    /// 促銷代碼
    /// </summary>
    [Required(ErrorMessage = "促銷代碼為必填")]
    [StringLength(20, ErrorMessage = "促銷代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 促銷名稱
    /// </summary>
    [Required(ErrorMessage = "促銷名稱為必填")]
    [StringLength(100, ErrorMessage = "促銷名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 促銷類型
    /// </summary>
    [Required(ErrorMessage = "促銷類型為必填")]
    public PromotionType PromotionType { get; set; }

    /// <summary>
    /// 折扣值
    /// </summary>
    [Required(ErrorMessage = "折扣值為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "折扣值不可為負數")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    [Required(ErrorMessage = "開始日期為必填")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    [Required(ErrorMessage = "結束日期為必填")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "最低消費金額不可為負數")]
    public decimal? MinPurchaseAmount { get; set; }

    /// <summary>
    /// 最大折扣金額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "最大折扣金額不可為負數")]
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 使用次數上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "使用次數上限需大於 0")]
    public int? UsageLimit { get; set; }
}

/// <summary>
/// 待處理訂單列表 DTO
/// </summary>
public class PendingOrderDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount => FinalAmount - PaidAmount;
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public int WaitingMinutes { get; set; }
    public string Priority { get; set; } = "Normal";
}

/// <summary>
/// 待處理訂單統計 DTO
/// </summary>
public class PendingOrderSummaryDto
{
    public int TotalPending { get; set; }
    public int TotalProcessing { get; set; }
    public int TotalAwaitingPayment { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public int AverageWaitingMinutes { get; set; }
    public int LongWaitingCount { get; set; }
}

/// <summary>
/// 處理訂單請求 DTO
/// </summary>
public class ProcessOrderRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// 訂單優先級請求 DTO
/// </summary>
public class UpdateOrderPriorityRequest
{
    [Required]
    public string Priority { get; set; } = "Normal";
}
