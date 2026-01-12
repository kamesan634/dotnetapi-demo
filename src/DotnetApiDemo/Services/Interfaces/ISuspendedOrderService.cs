using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 掛單服務介面
/// </summary>
public interface ISuspendedOrderService
{
    /// <summary>
    /// 取得掛單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <param name="pendingOnly">是否只取待處理的掛單</param>
    /// <returns>分頁掛單列表</returns>
    Task<PaginatedResponse<SuspendedOrderListDto>> GetSuspendedOrdersAsync(
        PaginationRequest request,
        int? storeId = null,
        bool pendingOnly = true);

    /// <summary>
    /// 取得掛單詳細資訊
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <returns>掛單詳細資訊</returns>
    Task<SuspendedOrderDetailDto?> GetSuspendedOrderByIdAsync(int id);

    /// <summary>
    /// 根據編號取得掛單
    /// </summary>
    /// <param name="orderNo">掛單編號</param>
    /// <returns>掛單詳細資訊</returns>
    Task<SuspendedOrderDetailDto?> GetSuspendedOrderByNoAsync(string orderNo);

    /// <summary>
    /// 建立掛單
    /// </summary>
    /// <param name="request">建立掛單請求</param>
    /// <param name="cashierId">收銀員 ID</param>
    /// <returns>建立的掛單 ID</returns>
    Task<int?> CreateSuspendedOrderAsync(CreateSuspendedOrderRequest request, int cashierId);

    /// <summary>
    /// 恢復掛單
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <param name="cashierId">收銀員 ID</param>
    /// <returns>恢復的掛單詳細資訊</returns>
    Task<SuspendedOrderDetailDto?> ResumeSuspendedOrderAsync(int id, int cashierId);

    /// <summary>
    /// 取消掛單
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <param name="reason">取消原因</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelSuspendedOrderAsync(int id, string? reason = null);

    /// <summary>
    /// 清理過期掛單
    /// </summary>
    /// <returns>清理的掛單數量</returns>
    Task<int> CleanupExpiredOrdersAsync();

    /// <summary>
    /// 取得門市待處理掛單數量
    /// </summary>
    /// <param name="storeId">門市 ID</param>
    /// <returns>待處理掛單數量</returns>
    Task<int> GetPendingCountAsync(int storeId);
}
