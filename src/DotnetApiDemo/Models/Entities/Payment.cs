using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 付款記錄實體
/// </summary>
/// <remarks>
/// 記錄訂單的付款明細，支援多種付款方式組合
/// </remarks>
public class Payment
{
    /// <summary>
    /// 付款記錄 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 付款編號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "PAY202512310001"
    /// </remarks>
    public string PaymentNo { get; set; } = string.Empty;

    /// <summary>
    /// 付款方式
    /// </summary>
    public Enums.PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 付款金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 付款狀態
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// 交易參考號
    /// </summary>
    /// <remarks>
    /// 信用卡授權碼、第三方支付交易號等
    /// </remarks>
    public string? TransactionRef { get; set; }

    /// <summary>
    /// 卡號末四碼
    /// </summary>
    /// <remarks>
    /// 信用卡/金融卡末四碼
    /// </remarks>
    public string? CardLastFour { get; set; }

    /// <summary>
    /// 付款時間
    /// </summary>
    public DateTime PaymentTime { get; set; }

    /// <summary>
    /// 退款金額
    /// </summary>
    public decimal RefundedAmount { get; set; } = 0;

    /// <summary>
    /// 退款時間
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Remarks { get; set; }

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
    /// 所屬訂單
    /// </summary>
    public virtual Order Order { get; set; } = null!;
}
