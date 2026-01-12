namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 盤點類型列舉
/// </summary>
/// <remarks>
/// 定義不同的盤點作業類型
/// </remarks>
public enum StockCountType
{
    /// <summary>
    /// 全盤 - 盤點倉庫全部商品
    /// </summary>
    Full = 0,

    /// <summary>
    /// 分類盤點 - 盤點特定分類商品
    /// </summary>
    Category = 1,

    /// <summary>
    /// 抽盤 - 隨機抽樣盤點
    /// </summary>
    Spot = 2,

    /// <summary>
    /// 循環盤點 - 依排程盤點不同區域
    /// </summary>
    Cycle = 3
}
