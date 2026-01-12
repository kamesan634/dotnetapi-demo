using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 銷售退貨單實體
/// </summary>
/// <remarks>
/// 代表客戶退貨記錄
/// </remarks>
public class SalesReturn
{
    /// <summary>
    /// 退貨單 ID (主鍵)
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
    /// 客戶 ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

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
    public SalesReturnStatus Status { get; set; } = SalesReturnStatus.Pending;

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
    public bool IsRefunded { get; set; } = false;

    /// <summary>
    /// 退款日期
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// 退款方式
    /// </summary>
    public string? RefundMethod { get; set; }

    /// <summary>
    /// 處理人員 ID
    /// </summary>
    public int? ProcessedById { get; set; }

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性

    /// <summary>
    /// 原訂單
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// 客戶
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// 門市
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// 處理人員
    /// </summary>
    public virtual ApplicationUser? ProcessedBy { get; set; }

    /// <summary>
    /// 退貨明細
    /// </summary>
    public virtual ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}

/// <summary>
/// 銷售退貨明細實體
/// </summary>
public class SalesReturnItem
{
    /// <summary>
    /// 退貨明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 退貨單 ID
    /// </summary>
    public int SalesReturnId { get; set; }

    /// <summary>
    /// 原訂單明細 ID
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品名稱 (快照)
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

    // 導航屬性

    /// <summary>
    /// 退貨單
    /// </summary>
    public virtual SalesReturn SalesReturn { get; set; } = null!;

    /// <summary>
    /// 原訂單明細
    /// </summary>
    public virtual OrderItem OrderItem { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
