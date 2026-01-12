using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 庫存盤點服務介面
/// </summary>
/// <remarks>
/// 處理庫存盤點單的建立、執行、完成等作業
/// </remarks>
public interface IStockCountService
{
    /// <summary>
    /// 取得盤點單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="status">狀態篩選 (可選)</param>
    /// <returns>分頁盤點單列表</returns>
    Task<PaginatedResponse<StockCountListDto>> GetStockCountsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        Models.Enums.StockCountStatus? status = null);

    /// <summary>
    /// 取得盤點單詳細資訊
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <returns>盤點單詳細資訊，若不存在則回傳 null</returns>
    Task<StockCountDetailDto?> GetStockCountByIdAsync(int id);

    /// <summary>
    /// 建立盤點單
    /// </summary>
    /// <param name="request">建立盤點單請求</param>
    /// <param name="userId">建立者 ID</param>
    /// <returns>建立的盤點單 ID，若失敗則回傳 null</returns>
    Task<int?> CreateStockCountAsync(CreateStockCountRequest request, int userId);

    /// <summary>
    /// 更新盤點明細
    /// </summary>
    /// <param name="stockCountId">盤點單 ID</param>
    /// <param name="itemId">明細 ID</param>
    /// <param name="request">更新請求</param>
    /// <param name="userId">操作者 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateStockCountItemAsync(int stockCountId, int itemId, UpdateStockCountItemRequest request, int userId);

    /// <summary>
    /// 開始盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <param name="userId">操作者 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> StartStockCountAsync(int id, int userId);

    /// <summary>
    /// 完成盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <param name="userId">操作者 ID</param>
    /// <returns>是否成功</returns>
    /// <remarks>
    /// 完成盤點時會根據差異產生庫存調整
    /// </remarks>
    Task<bool> CompleteStockCountAsync(int id, int userId);

    /// <summary>
    /// 取消盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <param name="userId">操作者 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelStockCountAsync(int id, int userId);
}
