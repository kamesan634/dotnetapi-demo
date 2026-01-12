namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 訂單狀態列舉
/// </summary>
/// <remarks>
/// 定義訂單從建立到完成的各種狀態
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    /// 草稿 - 訂單建立中，尚未確認
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 待處理 - 訂單已確認，等待處理
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 處理中 - 訂單正在處理
    /// </summary>
    Processing = 2,

    /// <summary>
    /// 已出貨 - 訂單已出貨
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// 已完成 - 訂單完成
    /// </summary>
    Completed = 4,

    /// <summary>
    /// 已取消 - 訂單已取消
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// 已退貨 - 訂單已退貨
    /// </summary>
    Returned = 6,

    /// <summary>
    /// 部分退貨 - 訂單部分退貨
    /// </summary>
    PartiallyReturned = 7
}
