namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 採購單狀態列舉
/// </summary>
/// <remarks>
/// 定義採購單從建立到完成的各種狀態
/// </remarks>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// 草稿 - 尚未送出，可編輯
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 待審核 - 等待主管審核
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已核准 - 審核通過
    /// </summary>
    Approved = 2,

    /// <summary>
    /// 部分到貨 - 部分商品已入庫
    /// </summary>
    Partial = 3,

    /// <summary>
    /// 已完成 - 全部入庫完成
    /// </summary>
    Completed = 4,

    /// <summary>
    /// 已關閉 - 手動關閉
    /// </summary>
    Closed = 5,

    /// <summary>
    /// 已取消 - 採購取消
    /// </summary>
    Cancelled = 6
}
