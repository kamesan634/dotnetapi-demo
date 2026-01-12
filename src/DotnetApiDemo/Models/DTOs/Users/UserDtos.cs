using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Users;

/// <summary>
/// 使用者列表 DTO
/// </summary>
public class UserListDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 使用者詳細資訊 DTO
/// </summary>
public class UserDetailDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public IEnumerable<RoleDto> Roles { get; set; } = Enumerable.Empty<RoleDto>();

    /// <summary>
    /// 門市列表
    /// </summary>
    public IEnumerable<UserStoreDto> Stores { get; set; } = Enumerable.Empty<UserStoreDto>();

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// 角色 DTO
/// </summary>
public class RoleDto
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 角色名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 使用者門市關聯 DTO
/// </summary>
public class UserStoreDto
{
    /// <summary>
    /// 門市 ID
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 是否為預設門市
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// 建立使用者請求 DTO
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Required(ErrorMessage = "使用者名稱為必填")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需在 3-50 字元之間")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度需在 6-100 字元之間")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    [Required(ErrorMessage = "姓名為必填")]
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 角色 ID 列表
    /// </summary>
    public IEnumerable<int>? RoleIds { get; set; }

    /// <summary>
    /// 門市 ID 列表
    /// </summary>
    public IEnumerable<int>? StoreIds { get; set; }
}

/// <summary>
/// 更新使用者請求 DTO
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// 電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string? Email { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
    public string? FullName { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 角色 ID 列表
    /// </summary>
    public IEnumerable<int>? RoleIds { get; set; }

    /// <summary>
    /// 門市 ID 列表
    /// </summary>
    public IEnumerable<int>? StoreIds { get; set; }
}
