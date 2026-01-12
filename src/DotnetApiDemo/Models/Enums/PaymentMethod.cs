namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 付款方式列舉
/// </summary>
/// <remarks>
/// 定義系統支援的各種付款方式
/// </remarks>
public enum PaymentMethod
{
    /// <summary>
    /// 現金
    /// </summary>
    Cash = 0,

    /// <summary>
    /// 信用卡
    /// </summary>
    CreditCard = 1,

    /// <summary>
    /// 金融卡
    /// </summary>
    DebitCard = 2,

    /// <summary>
    /// 行動支付 (如 LINE Pay, Apple Pay 等)
    /// </summary>
    MobilePayment = 3,

    /// <summary>
    /// 儲值金/會員點數
    /// </summary>
    StoredValue = 4,

    /// <summary>
    /// 禮券/禮物卡
    /// </summary>
    GiftCard = 5,

    /// <summary>
    /// 銀行轉帳
    /// </summary>
    BankTransfer = 6,

    /// <summary>
    /// 月結/賒帳
    /// </summary>
    OnAccount = 7
}
