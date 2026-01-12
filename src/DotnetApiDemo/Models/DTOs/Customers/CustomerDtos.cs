using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Customers;

/// <summary>
/// 會員等級列表 DTO
/// </summary>
public class CustomerLevelListDto
{
    /// <summary>
    /// 等級 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 等級代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 等級名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 折扣率 (%)
    /// </summary>
    public decimal DiscountRate { get; set; }

    /// <summary>
    /// 最低消費金額
    /// </summary>
    public decimal MinSpendAmount { get; set; }

    /// <summary>
    /// 會員數量
    /// </summary>
    public int CustomerCount { get; set; }
}

/// <summary>
/// 客戶/會員列表 DTO
/// </summary>
public class CustomerListDto
{
    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 會員編號
    /// </summary>
    public string MemberNo { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 會員等級名稱
    /// </summary>
    public string LevelName { get; set; } = string.Empty;

    /// <summary>
    /// 累計消費金額
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// 累計點數
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 客戶/會員詳細資訊 DTO
/// </summary>
public class CustomerDetailDto
{
    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 會員編號
    /// </summary>
    public string MemberNo { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 會員等級 ID
    /// </summary>
    public int LevelId { get; set; }

    /// <summary>
    /// 會員等級名稱
    /// </summary>
    public string LevelName { get; set; } = string.Empty;

    /// <summary>
    /// 累計消費金額
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// 累計點數
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 加入日期
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後消費日期
    /// </summary>
    public DateTime? LastPurchaseAt { get; set; }
}

/// <summary>
/// 建立會員等級請求 DTO
/// </summary>
public class CreateCustomerLevelRequest
{
    /// <summary>
    /// 等級代碼
    /// </summary>
    [Required(ErrorMessage = "等級代碼為必填")]
    [StringLength(20, ErrorMessage = "等級代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 等級名稱
    /// </summary>
    [Required(ErrorMessage = "等級名稱為必填")]
    [StringLength(50, ErrorMessage = "等級名稱長度不可超過 50 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 折扣率 (%)
    /// </summary>
    [Range(0, 100, ErrorMessage = "折扣率需在 0-100 之間")]
    public decimal DiscountRate { get; set; } = 0;

    /// <summary>
    /// 最低消費金額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "最低消費金額不可為負數")]
    public decimal MinSpendAmount { get; set; } = 0;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
    public string? Description { get; set; }
}

/// <summary>
/// 建立客戶/會員請求 DTO
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// 姓名
    /// </summary>
    [Required(ErrorMessage = "姓名為必填")]
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
    public string? Gender { get; set; }

    /// <summary>
    /// 會員等級 ID
    /// </summary>
    public int? LevelId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }
}

/// <summary>
/// 更新客戶/會員請求 DTO
/// </summary>
public class UpdateCustomerRequest
{
    /// <summary>
    /// 姓名
    /// </summary>
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
    public string? Gender { get; set; }

    /// <summary>
    /// 會員等級 ID
    /// </summary>
    public int? LevelId { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string? Notes { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}
