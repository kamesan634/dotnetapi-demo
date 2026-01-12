using System.ComponentModel.DataAnnotations;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Models.DTOs.Products;

/// <summary>
/// 商品分類列表 DTO
/// </summary>
public class CategoryListDto
{
    /// <summary>
    /// 分類 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分類代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分類 ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 父分類名稱
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 子分類數量
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// 商品數量
    /// </summary>
    public int ProductCount { get; set; }
}

/// <summary>
/// 商品分類樹狀結構 DTO
/// </summary>
public class CategoryTreeDto
{
    /// <summary>
    /// 分類 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分類代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 子分類列表
    /// </summary>
    public IEnumerable<CategoryTreeDto> Children { get; set; } = Enumerable.Empty<CategoryTreeDto>();
}

/// <summary>
/// 商品列表 DTO
/// </summary>
public class ProductListDto
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 計量單位
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// 售價
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// 成本
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 總庫存量
    /// </summary>
    public int TotalStock { get; set; }
}

/// <summary>
/// 商品詳細資訊 DTO
/// </summary>
public class ProductDetailDto
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分類 ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 計量單位 ID
    /// </summary>
    public int UnitId { get; set; }

    /// <summary>
    /// 計量單位名稱
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType TaxType { get; set; }

    /// <summary>
    /// 售價
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// 成本
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// 安全庫存量
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 最低訂購量
    /// </summary>
    public int MinOrderQuantity { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 條碼列表
    /// </summary>
    public IEnumerable<ProductBarcodeDto> Barcodes { get; set; } = Enumerable.Empty<ProductBarcodeDto>();

    /// <summary>
    /// 各倉庫庫存
    /// </summary>
    public IEnumerable<ProductInventoryDto> Inventories { get; set; } = Enumerable.Empty<ProductInventoryDto>();

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
/// 商品條碼 DTO
/// </summary>
public class ProductBarcodeDto
{
    /// <summary>
    /// 條碼 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 條碼
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// 是否為主條碼
    /// </summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// 商品庫存 DTO
/// </summary>
public class ProductInventoryDto
{
    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 庫存數量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 保留數量
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// 可用數量
    /// </summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;
}

/// <summary>
/// 建立商品分類請求 DTO
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// 分類代碼
    /// </summary>
    [Required(ErrorMessage = "分類代碼為必填")]
    [StringLength(20, ErrorMessage = "分類代碼長度不可超過 20 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    [Required(ErrorMessage = "分類名稱為必填")]
    [StringLength(100, ErrorMessage = "分類名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分類 ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
    public string? Description { get; set; }
}

/// <summary>
/// 更新商品分類請求 DTO
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// 分類名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "分類名稱長度不可超過 100 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 父分類 ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 建立商品請求 DTO
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// 商品編號
    /// </summary>
    [Required(ErrorMessage = "商品編號為必填")]
    [StringLength(50, ErrorMessage = "商品編號長度不可超過 50 字元")]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    [Required(ErrorMessage = "商品名稱為必填")]
    [StringLength(200, ErrorMessage = "商品名稱長度不可超過 200 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "商品描述長度不可超過 1000 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 分類 ID
    /// </summary>
    [Required(ErrorMessage = "商品分類為必填")]
    public int CategoryId { get; set; }

    /// <summary>
    /// 計量單位 ID
    /// </summary>
    [Required(ErrorMessage = "計量單位為必填")]
    public int UnitId { get; set; }

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType TaxType { get; set; } = TaxType.Taxable;

    /// <summary>
    /// 售價
    /// </summary>
    [Required(ErrorMessage = "售價為必填")]
    [Range(0, double.MaxValue, ErrorMessage = "售價不可為負數")]
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// 成本
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "成本不可為負數")]
    public decimal Cost { get; set; } = 0;

    /// <summary>
    /// 安全庫存量
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "安全庫存量不可為負數")]
    public int SafetyStock { get; set; } = 0;

    /// <summary>
    /// 最低訂購量
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "最低訂購量需大於 0")]
    public int MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// 主條碼
    /// </summary>
    [StringLength(50, ErrorMessage = "條碼長度不可超過 50 字元")]
    public string? Barcode { get; set; }
}

/// <summary>
/// 更新商品請求 DTO
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// 商品名稱
    /// </summary>
    [StringLength(200, ErrorMessage = "商品名稱長度不可超過 200 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "商品描述長度不可超過 1000 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 分類 ID
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// 計量單位 ID
    /// </summary>
    public int? UnitId { get; set; }

    /// <summary>
    /// 稅別
    /// </summary>
    public TaxType? TaxType { get; set; }

    /// <summary>
    /// 售價
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "售價不可為負數")]
    public decimal? SellingPrice { get; set; }

    /// <summary>
    /// 成本
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "成本不可為負數")]
    public decimal? Cost { get; set; }

    /// <summary>
    /// 安全庫存量
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "安全庫存量不可為負數")]
    public int? SafetyStock { get; set; }

    /// <summary>
    /// 最低訂購量
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "最低訂購量需大於 0")]
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 計量單位 DTO
/// </summary>
public class UnitDto
{
    /// <summary>
    /// 單位 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 單位代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 單位名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 建立計量單位請求 DTO
/// </summary>
public class CreateUnitRequest
{
    /// <summary>
    /// 單位代碼
    /// </summary>
    [Required(ErrorMessage = "單位代碼為必填")]
    [StringLength(10, ErrorMessage = "單位代碼長度不可超過 10 字元")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 單位名稱
    /// </summary>
    [Required(ErrorMessage = "單位名稱為必填")]
    [StringLength(50, ErrorMessage = "單位名稱長度不可超過 50 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 單位說明
    /// </summary>
    [StringLength(200, ErrorMessage = "說明長度不可超過 200 字元")]
    public string? Description { get; set; }
}

/// <summary>
/// 更新計量單位請求 DTO
/// </summary>
public class UpdateUnitRequest
{
    /// <summary>
    /// 單位名稱
    /// </summary>
    [StringLength(50, ErrorMessage = "單位名稱長度不可超過 50 字元")]
    public string? Name { get; set; }

    /// <summary>
    /// 單位說明
    /// </summary>
    [StringLength(200, ErrorMessage = "說明長度不可超過 200 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 計量單位列表 DTO
/// </summary>
public class UnitListDto
{
    /// <summary>
    /// 單位 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 單位代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 單位名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 單位說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統單位
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
}

/// <summary>
/// 計量單位詳細 DTO
/// </summary>
public class UnitDetailDto
{
    /// <summary>
    /// 單位 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 單位代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 單位名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 單位說明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否為系統單位
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
/// 商品匯入項目 DTO
/// </summary>
public class ProductImportItemDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public string? TaxType { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal Cost { get; set; }
    public int SafetyStock { get; set; }
    public int MinOrderQuantity { get; set; } = 1;
    public string? Barcode { get; set; }
}

/// <summary>
/// 商品匯入請求 DTO
/// </summary>
public class ProductImportRequest
{
    [Required]
    public IEnumerable<ProductImportItemDto> Products { get; set; } = Enumerable.Empty<ProductImportItemDto>();
    public bool UpdateExisting { get; set; } = false;
}

/// <summary>
/// 商品匯入結果 DTO
/// </summary>
public class ProductImportResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int CreatedCount { get; set; }
    public IEnumerable<ProductImportErrorDto> Errors { get; set; } = Enumerable.Empty<ProductImportErrorDto>();
}

/// <summary>
/// 商品匯入錯誤 DTO
/// </summary>
public class ProductImportErrorDto
{
    public int RowNumber { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// 商品匯出 DTO
/// </summary>
public class ProductExportDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal Cost { get; set; }
    public int SafetyStock { get; set; }
    public int MinOrderQuantity { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; }
}
