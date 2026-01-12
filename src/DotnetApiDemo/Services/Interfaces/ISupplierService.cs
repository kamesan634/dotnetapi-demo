using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 供應商服務介面
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// 取得供應商列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁供應商列表</returns>
    Task<PaginatedResponse<SupplierListDto>> GetSuppliersAsync(PaginationRequest request);

    /// <summary>
    /// 取得供應商詳細資訊
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <returns>供應商詳細資訊</returns>
    Task<SupplierDetailDto?> GetSupplierByIdAsync(int id);

    /// <summary>
    /// 建立供應商
    /// </summary>
    /// <param name="request">建立供應商請求</param>
    /// <returns>建立的供應商 ID</returns>
    Task<int?> CreateSupplierAsync(CreateSupplierRequest request);

    /// <summary>
    /// 更新供應商
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <param name="request">更新供應商請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateSupplierAsync(int id, UpdateSupplierRequest request);

    /// <summary>
    /// 刪除供應商
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteSupplierAsync(int id);

    /// <summary>
    /// 設定供應商商品價格
    /// </summary>
    /// <param name="supplierId">供應商 ID</param>
    /// <param name="request">價格設定請求</param>
    /// <returns>是否成功</returns>
    Task<bool> SetSupplierPriceAsync(int supplierId, SupplierPriceRequest request);

    /// <summary>
    /// 取得供應商商品列表
    /// </summary>
    /// <param name="supplierId">供應商 ID</param>
    /// <returns>供應商商品列表</returns>
    Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(int supplierId);
}
