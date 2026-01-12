using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.SystemSettings;

/// <summary>
/// 系統設定列表 DTO
/// </summary>
public class SystemSettingListDto
{
    /// <summary>
    /// 設定 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 設定分類
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 設定鍵值
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 設定值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 值類型
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// 是否為系統設定
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 系統設定詳細資訊 DTO
/// </summary>
public class SystemSettingDetailDto
{
    /// <summary>
    /// 設定 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 設定分類
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 設定鍵值
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 設定值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 設定說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 值類型
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// 是否為系統設定
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者名稱
    /// </summary>
    public string? UpdatedByName { get; set; }
}

/// <summary>
/// 建立系統設定請求 DTO
/// </summary>
public class CreateSystemSettingRequest
{
    /// <summary>
    /// 設定分類
    /// </summary>
    [Required(ErrorMessage = "設定分類為必填")]
    [StringLength(100, ErrorMessage = "設定分類長度不可超過 100 字元")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 設定鍵值
    /// </summary>
    [Required(ErrorMessage = "設定鍵值為必填")]
    [StringLength(100, ErrorMessage = "設定鍵值長度不可超過 100 字元")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 設定值
    /// </summary>
    [Required(ErrorMessage = "設定值為必填")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 設定說明
    /// </summary>
    [StringLength(500, ErrorMessage = "設定說明長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 值類型 (string, int, bool, decimal, json)
    /// </summary>
    [StringLength(20, ErrorMessage = "值類型長度不可超過 20 字元")]
    public string ValueType { get; set; } = "string";
}

/// <summary>
/// 更新系統設定請求 DTO
/// </summary>
public class UpdateSystemSettingRequest
{
    /// <summary>
    /// 設定值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 設定說明
    /// </summary>
    [StringLength(500, ErrorMessage = "設定說明長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}
