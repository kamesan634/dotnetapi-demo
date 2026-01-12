using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 訂單實體
/// </summary>
/// <remarks>
/// 代表一筆銷售交易
/// </remarks>
public class Order
{
    /// <summary>
    /// 訂單 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 訂單編號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "SO202512310001"
    /// </remarks>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 訂單日期
    /// </summary>
    public DateOnly OrderDate { get; set; }

    /// <summary>
    /// 訂單時間
    /// </summary>
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// 客戶 ID
    /// </summary>
    /// <remarks>
    /// 可為空，表示非會員消費
    /// </remarks>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 銷售人員 ID
    /// </summary>
    public int SalesPersonId { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// 商品小計 (未稅)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 訂單總額 (含稅)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 已付金額
    /// </summary>
    public decimal PaidAmount { get; set; } = 0;

    /// <summary>
    /// 找零金額
    /// </summary>
    public decimal ChangeAmount { get; set; } = 0;

    /// <summary>
    /// 使用點數
    /// </summary>
    public int UsedPoints { get; set; } = 0;

    /// <summary>
    /// 點數折抵金額
    /// </summary>
    public decimal PointsDiscount { get; set; } = 0;

    /// <summary>
    /// 獲得點數
    /// </summary>
    public int EarnedPoints { get; set; } = 0;

    /// <summary>
    /// 優惠券代碼
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// 優惠券折抵金額
    /// </summary>
    public decimal CouponDiscount { get; set; } = 0;

    /// <summary>
    /// 促銷活動 ID
    /// </summary>
    public int? PromotionId { get; set; }

    /// <summary>
    /// 促銷折抵金額
    /// </summary>
    public decimal PromotionDiscount { get; set; } = 0;

    /// <summary>
    /// 發票號碼
    /// </summary>
    public string? InvoiceNo { get; set; }

    /// <summary>
    /// 發票類型
    /// </summary>
    /// <remarks>
    /// 如：二聯式、三聯式、電子發票
    /// </remarks>
    public string? InvoiceType { get; set; }

    /// <summary>
    /// 買受人統編
    /// </summary>
    public string? BuyerTaxId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// 取消時間
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// 訂單優先級
    /// </summary>
    public string? Priority { get; set; }

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
    /// 門市
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// 客戶
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// 銷售人員
    /// </summary>
    public virtual ApplicationUser SalesPerson { get; set; } = null!;

    /// <summary>
    /// 促銷活動
    /// </summary>
    public virtual Promotion? Promotion { get; set; }

    /// <summary>
    /// 訂單明細列表
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// 付款記錄列表
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
