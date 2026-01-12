namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 銷售退貨狀態
/// </summary>
public enum SalesReturnStatus
{
    /// <summary>
    /// 待處理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已核准
    /// </summary>
    Approved = 1,

    /// <summary>
    /// 已拒絕
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// 處理中
    /// </summary>
    Processing = 3,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 4,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 5
}
