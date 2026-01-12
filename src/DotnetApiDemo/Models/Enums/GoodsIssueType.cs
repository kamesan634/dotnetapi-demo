namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 出庫單類型列舉
/// </summary>
/// <remarks>
/// 定義出庫作業的各種類型
/// </remarks>
public enum GoodsIssueType
{
    /// <summary>
    /// 銷售出庫 - POS 銷售自動扣庫
    /// </summary>
    SalesOut = 0,

    /// <summary>
    /// 退貨出庫 - 退還供應商
    /// </summary>
    ReturnOut = 1,

    /// <summary>
    /// 調撥出庫 - 倉庫間調撥
    /// </summary>
    TransferOut = 2,

    /// <summary>
    /// 盤虧出庫 - 盤點短缺
    /// </summary>
    CountOut = 3,

    /// <summary>
    /// 報廢出庫 - 商品報廢
    /// </summary>
    ScrapOut = 4,

    /// <summary>
    /// 其他出庫 - 樣品、贈送等
    /// </summary>
    OtherOut = 5
}
