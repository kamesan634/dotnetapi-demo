namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 付款狀態列舉
/// </summary>
/// <remarks>
/// 定義付款的各種狀態
/// </remarks>
public enum PaymentStatus
{
    /// <summary>
    /// 待付款
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已付款
    /// </summary>
    Paid = 1,

    /// <summary>
    /// 部分付款
    /// </summary>
    PartiallyPaid = 2,

    /// <summary>
    /// 付款失敗
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已退款
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// 部分退款
    /// </summary>
    PartiallyRefunded = 5,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 6
}
