namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 採購退貨單狀態列舉
/// </summary>
/// <remarks>
/// 定義採購退貨單的各種狀態
/// </remarks>
public enum PurchaseReturnStatus
{
    /// <summary>
    /// 草稿 - 建立中
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
    /// 已出庫 - 商品已退出
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// 供應商確認 - 供應商已收貨
    /// </summary>
    Confirmed = 4,

    /// <summary>
    /// 已完成 - 處理完畢
    /// </summary>
    Completed = 5,

    /// <summary>
    /// 已取消 - 退貨取消
    /// </summary>
    Cancelled = 6
}
