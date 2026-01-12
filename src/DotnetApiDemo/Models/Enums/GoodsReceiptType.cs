namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 入庫單類型列舉
/// </summary>
/// <remarks>
/// 定義入庫作業的各種類型
/// </remarks>
public enum GoodsReceiptType
{
    /// <summary>
    /// 採購入庫 - 依採購單進貨
    /// </summary>
    PurchaseIn = 0,

    /// <summary>
    /// 退貨入庫 - 客戶退貨
    /// </summary>
    ReturnIn = 1,

    /// <summary>
    /// 調撥入庫 - 倉庫間調撥
    /// </summary>
    TransferIn = 2,

    /// <summary>
    /// 盤盈入庫 - 盤點盈餘
    /// </summary>
    CountIn = 3,

    /// <summary>
    /// 其他入庫 - 贈品、樣品等
    /// </summary>
    OtherIn = 4
}
