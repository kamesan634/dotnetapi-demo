namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 退貨原因列舉
/// </summary>
/// <remarks>
/// 定義退貨的各種原因 (適用於採購退貨和銷售退貨)
/// </remarks>
public enum ReturnReason
{
    /// <summary>
    /// 商品瑕疵
    /// </summary>
    Defect = 0,

    /// <summary>
    /// 運送損壞
    /// </summary>
    Damage = 1,

    /// <summary>
    /// 送錯商品
    /// </summary>
    Wrong = 2,

    /// <summary>
    /// 過期商品
    /// </summary>
    Expire = 3,

    /// <summary>
    /// 數量過多
    /// </summary>
    Excess = 4,

    /// <summary>
    /// 品質不符
    /// </summary>
    Quality = 5,

    /// <summary>
    /// 顧客不滿意
    /// </summary>
    CustomerDissatisfied = 6,

    /// <summary>
    /// 尺寸不合
    /// </summary>
    SizeNotFit = 7,

    /// <summary>
    /// 其他原因
    /// </summary>
    Other = 8
}
