using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 商品分類服務介面
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// 取得分類列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁分類列表</returns>
    Task<PaginatedResponse<CategoryListDto>> GetCategoriesAsync(PaginationRequest request);

    /// <summary>
    /// 取得分類樹狀結構
    /// </summary>
    /// <returns>分類樹</returns>
    Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();

    /// <summary>
    /// 取得分類詳細資訊
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <returns>分類詳細資訊</returns>
    Task<CategoryListDto?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// 建立分類
    /// </summary>
    /// <param name="request">建立分類請求</param>
    /// <returns>建立的分類 ID</returns>
    Task<int?> CreateCategoryAsync(CreateCategoryRequest request);

    /// <summary>
    /// 更新分類
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <param name="request">更新分類請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request);

    /// <summary>
    /// 刪除分類
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteCategoryAsync(int id);
}

/// <summary>
/// 商品服務介面
/// </summary>
public interface IProductService
{
    /// <summary>
    /// 取得商品列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="categoryId">分類 ID (可選)</param>
    /// <returns>分頁商品列表</returns>
    Task<PaginatedResponse<ProductListDto>> GetProductsAsync(PaginationRequest request, int? categoryId = null);

    /// <summary>
    /// 取得商品詳細資訊
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>商品詳細資訊</returns>
    Task<ProductDetailDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// 根據 SKU 取得商品
    /// </summary>
    /// <param name="sku">商品編號</param>
    /// <returns>商品詳細資訊</returns>
    Task<ProductDetailDto?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// 根據條碼取得商品
    /// </summary>
    /// <param name="barcode">條碼</param>
    /// <returns>商品詳細資訊</returns>
    Task<ProductDetailDto?> GetProductByBarcodeAsync(string barcode);

    /// <summary>
    /// 建立商品
    /// </summary>
    /// <param name="request">建立商品請求</param>
    /// <returns>建立的商品 ID</returns>
    Task<int?> CreateProductAsync(CreateProductRequest request);

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <param name="request">更新商品請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateProductAsync(int id, UpdateProductRequest request);

    /// <summary>
    /// 刪除商品
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// 取得所有計量單位
    /// </summary>
    /// <returns>計量單位列表</returns>
    Task<IEnumerable<UnitDto>> GetUnitsAsync();

    /// <summary>
    /// 建立計量單位
    /// </summary>
    /// <param name="request">建立計量單位請求</param>
    /// <returns>建立的單位 ID</returns>
    Task<int?> CreateUnitAsync(CreateUnitRequest request);

    /// <summary>
    /// 匯入商品 (JSON)
    /// </summary>
    Task<ProductImportResultDto> ImportProductsAsync(ProductImportRequest request);

    /// <summary>
    /// 從 Excel 匯入商品
    /// </summary>
    /// <param name="excelFile">Excel 檔案串流</param>
    /// <param name="updateExisting">是否更新現有商品</param>
    Task<ProductImportResultDto> ImportProductsFromExcelAsync(Stream excelFile, bool updateExisting = false);

    /// <summary>
    /// 匯出商品為 CSV
    /// </summary>
    Task<byte[]> ExportProductsToCsvAsync(int? categoryId = null);

    /// <summary>
    /// 匯出商品為 Excel
    /// </summary>
    Task<byte[]> ExportProductsToExcelAsync(int? categoryId = null);

    /// <summary>
    /// 取得 CSV 匯入範本
    /// </summary>
    Task<byte[]> GetImportTemplateAsync();

    /// <summary>
    /// 取得 Excel 匯入範本
    /// </summary>
    Task<byte[]> GetImportTemplateExcelAsync();
}
