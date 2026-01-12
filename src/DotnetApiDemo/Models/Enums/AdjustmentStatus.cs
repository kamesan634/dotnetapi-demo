namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 庫存調整狀態列舉
/// </summary>
/// <remarks>
/// 定義庫存調整單的各種狀態
/// </remarks>
public enum AdjustmentStatus
{
    /// <summary>
    /// 待審核
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
    /// 已完成
    /// </summary>
    Completed = 3
}
