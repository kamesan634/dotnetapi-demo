using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Stores;

/// <summary>
/// 門市列表 DTO
/// </summary>
public class StoreListDto
{
    /// <summary>
    /// 門市 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 門市代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 倉庫數量
    /// </summary>
    public int WarehouseCount { get; set; }
}

/// <summary>
/// 門市詳細資訊 DTO
/// </summary>
public class StoreDetailDto
{
    /// <summary>
    /// 門市 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 門市代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 門市名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 倉庫列表
    /// </summary>
    public IEnumerable<WarehouseListDto> Warehouses { get; set; } = Enumerable.Empty<WarehouseListDto>();

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
/// 倉庫列表 DTO
/// </summary>
public class WarehouseListDto
{
    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 倉庫代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所屬門市 ID
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// 所屬門市名稱
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 倉庫詳細資訊 DTO
/// </summary>
public class WarehouseDetailDto
{
    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 倉庫代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 所屬門市 ID
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// 所屬門市名稱
    /// </summary>
    public string? StoreName { get; set; }

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
}

/// <summary>
/// 建立門市請求 DTO
/// </summary>
public class CreateStoreRequest
{
    /// <summary>
    /// 門市代碼
    /// </summary>
    [Required(ErrorMessage = "門市代碼為必填")]
    [StringLength(20, ErrorMessage = "門市代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 門市名稱
    /// </summary>
    [Required(ErrorMessage = "門市名稱為必填")]
    [StringLength(100, ErrorMessage = "門市名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }
}

/// <summary>
/// 更新門市請求 DTO
/// </summary>
public class UpdateStoreRequest
{
    /// <summary>
    /// 門市名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "門市名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 建立倉庫請求 DTO
/// </summary>
public class CreateWarehouseRequest
{
    /// <summary>
    /// 倉庫代碼
    /// </summary>
    [Required(ErrorMessage = "倉庫代碼為必填")]
    [StringLength(20, ErrorMessage = "倉庫代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    [Required(ErrorMessage = "倉庫名稱為必填")]
    [StringLength(100, ErrorMessage = "倉庫名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所屬門市 ID
    /// </summary>
    [Required(ErrorMessage = "所屬門市為必填")]
    public int StoreId { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }
}

/// <summary>
/// 更新倉庫請求 DTO
/// </summary>
public class UpdateWarehouseRequest
{
    /// <summary>
    /// 倉庫名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "倉庫名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(200, ErrorMessage = "地址長度不可超過 200 字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}
