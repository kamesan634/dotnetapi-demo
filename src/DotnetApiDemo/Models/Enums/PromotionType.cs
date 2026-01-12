namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 促銷類型列舉
/// </summary>
/// <remarks>
/// 定義各種促銷活動類型
/// </remarks>
public enum PromotionType
{
    /// <summary>
    /// 折扣 - 依比例折扣
    /// </summary>
    Discount = 0,

    /// <summary>
    /// 滿額折 - 滿足金額條件折抵
    /// </summary>
    AmountOff = 1,

    /// <summary>
    /// 買 N 送 M - 買 N 件送 M 件
    /// </summary>
    BuyNGetM = 2,

    /// <summary>
    /// 加價購 - 滿額可加價購
    /// </summary>
    AddOnPurchase = 3,

    /// <summary>
    /// 組合價 - 商品組合特價
    /// </summary>
    Bundle = 4,

    /// <summary>
    /// 會員專屬 - 會員專屬優惠
    /// </summary>
    MemberOnly = 5
}
