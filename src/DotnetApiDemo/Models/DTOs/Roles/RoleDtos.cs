using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Roles;

/// <summary>
/// 角色列表 DTO
/// </summary>
public class RoleListDto
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
    /// 角色說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統角色
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 角色詳細資訊 DTO
/// </summary>
public class RoleDetailDto
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
    /// 角色說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統角色
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 建立角色請求 DTO
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// 角色名稱
    /// </summary>
    [Required(ErrorMessage = "角色名稱為必填")]
    [StringLength(50, ErrorMessage = "角色名稱長度不可超過 50 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色說明
    /// </summary>
    [StringLength(200, ErrorMessage = "角色說明長度不可超過 200 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 更新角色請求 DTO
/// </summary>
public class UpdateRoleRequest
{
    /// <summary>
    /// 角色名稱
    /// </summary>
    [StringLength(50, ErrorMessage = "角色名稱長度不可超過 50 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 角色說明
    /// </summary>
    [StringLength(200, ErrorMessage = "角色說明長度不可超過 200 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int? SortOrder { get; set; }
}
