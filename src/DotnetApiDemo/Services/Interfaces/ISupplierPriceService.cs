using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 供應商報價服務介面
/// </summary>
public interface ISupplierPriceService
{
    /// <summary>
    /// 取得供應商報價列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選篩選)</param>
    /// <param name="productId">商品 ID (可選篩選)</param>
    /// <returns>分頁供應商報價列表</returns>
    Task<PaginatedResponse<SupplierPriceListDto>> GetSupplierPricesAsync(
        PaginationRequest request,
        int? supplierId = null,
        int? productId = null);

    /// <summary>
    /// 取得供應商報價詳細資訊
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <returns>供應商報價詳細資訊</returns>
    Task<SupplierPriceDetailDto?> GetSupplierPriceByIdAsync(int id);

    /// <summary>
    /// 取得商品的所有供應商報價
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns>供應商報價列表</returns>
    Task<IEnumerable<SupplierPriceListDto>> GetPricesByProductAsync(int productId);

    /// <summary>
    /// 取得供應商的所有商品報價
    /// </summary>
    /// <param name="supplierId">供應商 ID</param>
    /// <returns>供應商報價列表</returns>
    Task<IEnumerable<SupplierPriceListDto>> GetPricesBySupplierAsync(int supplierId);

    /// <summary>
    /// 建立供應商報價
    /// </summary>
    /// <param name="request">建立供應商報價請求</param>
    /// <returns>建立的報價 ID，若失敗則返回 null</returns>
    Task<int?> CreateSupplierPriceAsync(CreateSupplierPriceRequest request);

    /// <summary>
    /// 更新供應商報價
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <param name="request">更新供應商報價請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateSupplierPriceAsync(int id, UpdateSupplierPriceRequest request);

    /// <summary>
    /// 刪除供應商報價
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteSupplierPriceAsync(int id);
}
