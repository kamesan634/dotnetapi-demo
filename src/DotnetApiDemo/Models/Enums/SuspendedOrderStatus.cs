namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 掛單狀態
/// </summary>
public enum SuspendedOrderStatus
{
    /// <summary>
    /// 掛單中
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已恢復
    /// </summary>
    Resumed = 1,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// 已過期
    /// </summary>
    Expired = 3
}
