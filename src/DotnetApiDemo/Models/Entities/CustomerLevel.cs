namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 會員等級實體
/// </summary>
/// <remarks>
/// 定義會員等級及其權益
/// </remarks>
public class CustomerLevel
{
    /// <summary>
    /// 等級 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 等級代碼
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "VIP"
    /// </remarks>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 等級名稱
    /// </summary>
    /// <remarks>
    /// 如：一般會員、銀卡會員、金卡會員
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 等級說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    /// <remarks>
    /// 達到此金額可升級至此等級
    /// </remarks>
    public decimal MinSpendAmount { get; set; } = 0;

    /// <summary>
    /// 折扣率 (%)
    /// </summary>
    /// <remarks>
    /// 如：95 表示 95 折
    /// </remarks>
    public decimal DiscountRate { get; set; } = 100;

    /// <summary>
    /// 點數倍率
    /// </summary>
    /// <remarks>
    /// 如：1.5 表示 1.5 倍點數
    /// </remarks>
    public decimal PointsMultiplier { get; set; } = 1;

    /// <summary>
    /// 等級顏色
    /// </summary>
    /// <remarks>
    /// Hex 色碼，用於 UI 顯示
    /// </remarks>
    public string? Color { get; set; }

    /// <summary>
    /// 是否為預設等級
    /// </summary>
    /// <remarks>
    /// 新會員註冊時的預設等級
    /// </remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性

    /// <summary>
    /// 此等級的會員列表
    /// </summary>
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
