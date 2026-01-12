using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Orders;

/// <summary>
/// 掛單列表 DTO
/// </summary>
public class SuspendedOrderListDto
{
    /// <summary>
    /// 掛單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 掛單編號
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
    /// 收銀員名稱
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 掛單原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 商品數量
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public SuspendedOrderStatus Status { get; set; }

    /// <summary>
    /// 掛單時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 過期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 掛單詳細 DTO
/// </summary>
public class SuspendedOrderDetailDto
{
    /// <summary>
    /// 掛單 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 掛單編號
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
    /// 收銀員 ID
    /// </summary>
    public int CashierId { get; set; }

    /// <summary>
    /// 收銀員名稱
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 客戶名稱
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 掛單原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public SuspendedOrderStatus Status { get; set; }

    /// <summary>
    /// 掛單時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 過期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 恢復時間
    /// </summary>
    public DateTime? ResumedAt { get; set; }

    /// <summary>
    /// 恢復後訂單 ID
    /// </summary>
    public int? ResumedOrderId { get; set; }

    /// <summary>
    /// 商品明細
    /// </summary>
    public IEnumerable<SuspendedOrderItemDto> Items { get; set; } = Enumerable.Empty<SuspendedOrderItemDto>();
}

/// <summary>
/// 掛單商品明細 DTO
/// </summary>
public class SuspendedOrderItemDto
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
    /// 商品規格 ID
    /// </summary>
    public int? VariantId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 規格名稱
    /// </summary>
    public string? VariantName { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 原價
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 小計
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 建立掛單請求 DTO
/// </summary>
public class CreateSuspendedOrderRequest
{
    /// <summary>
    /// 門市 ID
    /// </summary>
    [Required(ErrorMessage = "門市 ID 為必填")]
    public int StoreId { get; set; }

    /// <summary>
    /// 客戶 ID (可選)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 客戶名稱 (非會員客戶)
    /// </summary>
    [StringLength(100)]
    public string? CustomerName { get; set; }

    /// <summary>
    /// 掛單原因
    /// </summary>
    [StringLength(200)]
    public string? Reason { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// 過期小時數（預設 24 小時）
    /// </summary>
    [Range(1, 168, ErrorMessage = "過期時間必須在 1 到 168 小時之間")]
    public int ExpirationHours { get; set; } = 24;

    /// <summary>
    /// 商品明細
    /// </summary>
    [Required(ErrorMessage = "至少需要一筆商品")]
    [MinLength(1, ErrorMessage = "至少需要一筆商品")]
    public IEnumerable<CreateSuspendedOrderItemRequest> Items { get; set; } = Enumerable.Empty<CreateSuspendedOrderItemRequest>();
}

/// <summary>
/// 建立掛單商品明細請求 DTO
/// </summary>
public class CreateSuspendedOrderItemRequest
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    [Required(ErrorMessage = "商品 ID 為必填")]
    public int ProductId { get; set; }

    /// <summary>
    /// 商品規格 ID (可選)
    /// </summary>
    public int? VariantId { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    [Required(ErrorMessage = "數量為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "數量必須大於 0")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單價 (可選，不提供則使用商品定價)
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(200)]
    public string? Notes { get; set; }
}

/// <summary>
/// 恢復掛單請求 DTO
/// </summary>
public class ResumeSuspendedOrderRequest
{
    /// <summary>
    /// 掛單 ID
    /// </summary>
    [Required(ErrorMessage = "掛單 ID 為必填")]
    public int SuspendedOrderId { get; set; }
}
