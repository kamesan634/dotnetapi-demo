using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 掛單實體
/// </summary>
/// <remarks>
/// 代表暫時保存的訂單，可稍後恢復結帳
/// </remarks>
public class SuspendedOrder
{
    /// <summary>
    /// 掛單 ID (主鍵)
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
    /// 收銀員 ID
    /// </summary>
    public int CashierId { get; set; }

    /// <summary>
    /// 客戶 ID (可選)
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
    public SuspendedOrderStatus Status { get; set; } = SuspendedOrderStatus.Pending;

    /// <summary>
    /// 掛單時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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

    // 導航屬性

    /// <summary>
    /// 門市
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// 收銀員
    /// </summary>
    public virtual ApplicationUser Cashier { get; set; } = null!;

    /// <summary>
    /// 客戶
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// 恢復後訂單
    /// </summary>
    public virtual Order? ResumedOrder { get; set; }

    /// <summary>
    /// 掛單商品明細
    /// </summary>
    public virtual ICollection<SuspendedOrderItem> Items { get; set; } = new List<SuspendedOrderItem>();
}
