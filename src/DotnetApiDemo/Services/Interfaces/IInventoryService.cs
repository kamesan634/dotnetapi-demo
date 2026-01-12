using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 庫存服務介面
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// 取得庫存列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="productId">商品 ID (可選)</param>
    /// <returns>分頁庫存列表</returns>
    Task<PaginatedResponse<InventoryListDto>> GetInventoriesAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? productId = null);

    /// <summary>
    /// 取得商品在各倉庫的庫存
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns>庫存列表</returns>
    Task<IEnumerable<InventoryListDto>> GetProductInventoriesAsync(int productId);

    /// <summary>
    /// 取得倉庫所有庫存
    /// </summary>
    /// <param name="warehouseId">倉庫 ID</param>
    /// <returns>庫存列表</returns>
    Task<IEnumerable<InventoryListDto>> GetWarehouseInventoriesAsync(int warehouseId);

    /// <summary>
    /// 取得低庫存警示
    /// </summary>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <returns>低庫存警示列表</returns>
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int? warehouseId = null);

    /// <summary>
    /// 取得庫存異動記錄
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="productId">商品 ID (可選)</param>
    /// <returns>分頁庫存異動記錄</returns>
    Task<PaginatedResponse<InventoryMovementDto>> GetInventoryMovementsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? productId = null);

    /// <summary>
    /// 建立庫存調整單
    /// </summary>
    /// <param name="request">庫存調整請求</param>
    /// <param name="userId">操作使用者 ID</param>
    /// <returns>調整單 ID</returns>
    Task<int?> CreateStockAdjustmentAsync(StockAdjustmentRequest request, int userId);

    /// <summary>
    /// 取得補貨建議
    /// </summary>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <returns>補貨建議列表</returns>
    Task<IEnumerable<ReplenishmentSuggestionDto>> GetReplenishmentSuggestionsAsync(int? warehouseId = null);
}

/// <summary>
/// 庫存調撥服務介面
/// </summary>
public interface IStockTransferService
{
    /// <summary>
    /// 取得調撥單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁調撥單列表</returns>
    Task<PaginatedResponse<StockTransferDto>> GetStockTransfersAsync(PaginationRequest request);

    /// <summary>
    /// 取得調撥單詳細資訊
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>調撥單詳細資訊</returns>
    Task<StockTransferDetailDto?> GetStockTransferByIdAsync(int id);

    /// <summary>
    /// 建立調撥單
    /// </summary>
    /// <param name="request">建立調撥單請求</param>
    /// <param name="userId">操作使用者 ID</param>
    /// <returns>建立的調撥單 ID</returns>
    Task<int?> CreateStockTransferAsync(CreateStockTransferRequest request, int userId);

    /// <summary>
    /// 核准調撥單
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <param name="userId">核准人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ApproveStockTransferAsync(int id, int userId);

    /// <summary>
    /// 完成調撥 (出庫)
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ShipStockTransferAsync(int id, int userId);

    /// <summary>
    /// 完成調撥 (入庫)
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ReceiveStockTransferAsync(int id, int userId);

    /// <summary>
    /// 取消調撥單
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelStockTransferAsync(int id, int userId);
}
