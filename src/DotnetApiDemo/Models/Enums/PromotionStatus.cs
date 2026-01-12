namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 促銷活動狀態列舉
/// </summary>
/// <remarks>
/// 定義促銷活動的各種狀態
/// </remarks>
public enum PromotionStatus
{
    /// <summary>
    /// 草稿 - 尚未啟用
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 排程中 - 已排程但尚未開始
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// 進行中 - 活動進行中
    /// </summary>
    Active = 2,

    /// <summary>
    /// 已暫停 - 活動暫停
    /// </summary>
    Paused = 3,

    /// <summary>
    /// 已結束 - 活動已結束
    /// </summary>
    Ended = 4,

    /// <summary>
    /// 已取消 - 活動已取消
    /// </summary>
    Cancelled = 5
}
