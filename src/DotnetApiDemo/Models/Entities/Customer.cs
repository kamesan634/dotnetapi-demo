namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 客戶/會員實體
/// </summary>
/// <remarks>
/// 代表消費的客戶或註冊會員
/// </remarks>
public class Customer
{
    /// <summary>
    /// 客戶 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 會員編號
    /// </summary>
    /// <remarks>
    /// 唯一識別碼，如 "M00001"
    /// </remarks>
    public string MemberNo { get; set; } = string.Empty;

    /// <summary>
    /// 客戶姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 性別
    /// </summary>
    /// <remarks>
    /// M: 男, F: 女, O: 其他
    /// </remarks>
    public string? Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 手機號碼
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 會員等級 ID
    /// </summary>
    public int CustomerLevelId { get; set; }

    /// <summary>
    /// 累積消費金額
    /// </summary>
    public decimal TotalSpent { get; set; } = 0;

    /// <summary>
    /// 累積消費次數
    /// </summary>
    public int TotalOrders { get; set; } = 0;

    /// <summary>
    /// 目前點數
    /// </summary>
    public int CurrentPoints { get; set; } = 0;

    /// <summary>
    /// 累積點數
    /// </summary>
    public int TotalPoints { get; set; } = 0;

    /// <summary>
    /// 已使用點數
    /// </summary>
    public int UsedPoints { get; set; } = 0;

    /// <summary>
    /// 最後消費日期
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }

    /// <summary>
    /// 加入日期
    /// </summary>
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 加入門市 ID
    /// </summary>
    public int? JoinStoreId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

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
    /// 會員等級
    /// </summary>
    public virtual CustomerLevel CustomerLevel { get; set; } = null!;

    /// <summary>
    /// 加入門市
    /// </summary>
    public virtual Store? JoinStore { get; set; }

    /// <summary>
    /// 客戶訂單列表
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
