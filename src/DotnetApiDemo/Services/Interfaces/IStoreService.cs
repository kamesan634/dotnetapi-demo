using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Stores;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 門市服務介面
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// 取得門市列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁門市列表</returns>
    Task<PaginatedResponse<StoreListDto>> GetStoresAsync(PaginationRequest request);

    /// <summary>
    /// 取得門市詳細資訊
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <returns>門市詳細資訊</returns>
    Task<StoreDetailDto?> GetStoreByIdAsync(int id);

    /// <summary>
    /// 建立門市
    /// </summary>
    /// <param name="request">建立門市請求</param>
    /// <returns>建立的門市 ID</returns>
    Task<int?> CreateStoreAsync(CreateStoreRequest request);

    /// <summary>
    /// 更新門市
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <param name="request">更新門市請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateStoreAsync(int id, UpdateStoreRequest request);

    /// <summary>
    /// 刪除門市
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteStoreAsync(int id);
}

/// <summary>
/// 倉庫服務介面
/// </summary>
public interface IWarehouseService
{
    /// <summary>
    /// 取得倉庫列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <returns>分頁倉庫列表</returns>
    Task<PaginatedResponse<WarehouseListDto>> GetWarehousesAsync(PaginationRequest request, int? storeId = null);

    /// <summary>
    /// 取得倉庫詳細資訊
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <returns>倉庫詳細資訊</returns>
    Task<WarehouseDetailDto?> GetWarehouseByIdAsync(int id);

    /// <summary>
    /// 建立倉庫
    /// </summary>
    /// <param name="request">建立倉庫請求</param>
    /// <returns>建立的倉庫 ID</returns>
    Task<int?> CreateWarehouseAsync(CreateWarehouseRequest request);

    /// <summary>
    /// 更新倉庫
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <param name="request">更新倉庫請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateWarehouseAsync(int id, UpdateWarehouseRequest request);

    /// <summary>
    /// 刪除倉庫
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteWarehouseAsync(int id);
}
