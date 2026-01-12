namespace DotnetApiDemo.Models.Enums;

/// <summary>
/// 庫存調整原因列舉
/// </summary>
/// <remarks>
/// 定義庫存調整的各種原因
/// </remarks>
public enum AdjustmentReason
{
    /// <summary>
    /// 損壞報廢
    /// </summary>
    Damage = 0,

    /// <summary>
    /// 過期報廢
    /// </summary>
    Expire = 1,

    /// <summary>
    /// 遺失
    /// </summary>
    Lost = 2,

    /// <summary>
    /// 尋獲
    /// </summary>
    Found = 3,

    /// <summary>
    /// 資料錯誤更正
    /// </summary>
    Error = 4,

    /// <summary>
    /// 贈送
    /// </summary>
    Gift = 5,

    /// <summary>
    /// 樣品
    /// </summary>
    Sample = 6,

    /// <summary>
    /// 其他原因
    /// </summary>
    Other = 7
}
