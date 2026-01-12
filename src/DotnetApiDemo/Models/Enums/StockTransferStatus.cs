namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 調撥單狀態列舉
/// </summary>
/// <remarks>
/// 定義庫存調撥作業的各種狀態
/// </remarks>
public enum StockTransferStatus
{
    /// <summary>
    /// 草稿 - 可編輯
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 待審核 - 等待審核
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已核准 - 審核通過
    /// </summary>
    Approved = 2,

    /// <summary>
    /// 出庫中 - 來源倉庫已出庫
    /// </summary>
    Shipping = 3,

    /// <summary>
    /// 在途中 - 運送中
    /// </summary>
    InTransit = 4,

    /// <summary>
    /// 已入庫 - 目的倉庫已入庫
    /// </summary>
    Received = 5,

    /// <summary>
    /// 已完成 - 調撥完成
    /// </summary>
    Completed = 6,

    /// <summary>
    /// 已取消 - 調撥取消
    /// </summary>
    Cancelled = 7
}
