namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 入庫單狀態列舉
/// </summary>
/// <remarks>
/// 定義入庫單的各種狀態
/// </remarks>
public enum GoodsReceiptStatus
{
    /// <summary>
    /// 草稿 - 建立中
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已完成 - 入庫完成
    /// </summary>
    Completed = 1,

    /// <summary>
    /// 已取消 - 入庫取消
    /// </summary>
    Cancelled = 2
}
