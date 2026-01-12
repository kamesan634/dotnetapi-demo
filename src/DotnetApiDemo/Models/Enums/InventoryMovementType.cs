namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 庫存異動類型列舉
/// </summary>
/// <remarks>
/// 定義所有可能的庫存異動類型，用於追蹤庫存變化的原因
/// </remarks>
public enum InventoryMovementType
{
    /// <summary>
    /// 採購入庫 - 依採購單進貨
    /// </summary>
    PurchaseIn = 0,

    /// <summary>
    /// 銷售出庫 - POS 銷售扣庫
    /// </summary>
    SalesOut = 1,

    /// <summary>
    /// 退貨入庫 - 客戶退貨
    /// </summary>
    ReturnIn = 2,

    /// <summary>
    /// 退貨出庫 - 退還供應商
    /// </summary>
    ReturnOut = 3,

    /// <summary>
    /// 調撥入庫 - 從其他倉庫調入
    /// </summary>
    TransferIn = 4,

    /// <summary>
    /// 調撥出庫 - 調往其他倉庫
    /// </summary>
    TransferOut = 5,

    /// <summary>
    /// 調整入庫 - 手動增加庫存
    /// </summary>
    AdjustIn = 6,

    /// <summary>
    /// 調整出庫 - 手動減少庫存
    /// </summary>
    AdjustOut = 7,

    /// <summary>
    /// 盤盈入庫 - 盤點發現多餘
    /// </summary>
    CountIn = 8,

    /// <summary>
    /// 盤虧出庫 - 盤點發現短缺
    /// </summary>
    CountOut = 9,

    /// <summary>
    /// 報廢出庫 - 商品報廢
    /// </summary>
    ScrapOut = 10,

    /// <summary>
    /// 其他入庫 - 其他原因入庫
    /// </summary>
    OtherIn = 11,

    /// <summary>
    /// 其他出庫 - 其他原因出庫
    /// </summary>
    OtherOut = 12
}
