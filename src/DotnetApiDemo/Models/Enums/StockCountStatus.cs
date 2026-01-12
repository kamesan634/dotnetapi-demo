namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 盤點單狀態列舉
/// </summary>
/// <remarks>
/// 定義庫存盤點作業的各種狀態
/// </remarks>
public enum StockCountStatus
{
    /// <summary>
    /// 草稿 - 可編輯盤點範圍
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 盤點中 - 執行盤點作業
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 待審核 - 等待主管審核差異
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// 已核准 - 審核通過
    /// </summary>
    Approved = 3,

    /// <summary>
    /// 已完成 - 庫存已調整
    /// </summary>
    Completed = 4,

    /// <summary>
    /// 已取消 - 盤點取消
    /// </summary>
    Cancelled = 5
}
