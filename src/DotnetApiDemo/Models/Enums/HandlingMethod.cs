namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 退貨處理方式列舉
/// </summary>
/// <remarks>
/// 定義退貨的處理方式
/// </remarks>
public enum HandlingMethod
{
    /// <summary>
    /// 折讓沖帳
    /// </summary>
    Credit = 0,

    /// <summary>
    /// 換貨
    /// </summary>
    Exchange = 1,

    /// <summary>
    /// 退款
    /// </summary>
    Refund = 2
}
