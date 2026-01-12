namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 付款條件列舉
/// </summary>
/// <remarks>
/// 定義採購或銷售的付款條件
/// </remarks>
public enum PaymentTerms
{
    /// <summary>
    /// 現金 - 即付
    /// </summary>
    Cash = 0,

    /// <summary>
    /// 貨到付款
    /// </summary>
    COD = 1,

    /// <summary>
    /// 月結 30 天
    /// </summary>
    Net30 = 2,

    /// <summary>
    /// 月結 60 天
    /// </summary>
    Net60 = 3,

    /// <summary>
    /// 月結 90 天
    /// </summary>
    Net90 = 4
}
