namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 稅別列舉
/// </summary>
/// <remarks>
/// 定義商品或交易的稅務類型
/// </remarks>
public enum TaxType
{
    /// <summary>
    /// 應稅 (含 5% 營業稅)
    /// </summary>
    Taxable = 0,

    /// <summary>
    /// 免稅
    /// </summary>
    TaxFree = 1,

    /// <summary>
    /// 零稅率
    /// </summary>
    ZeroRate = 2
}
