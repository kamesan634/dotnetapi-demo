using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 訂單服務介面
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// 取得訂單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <param name="customerId">客戶 ID (可選)</param>
    /// <returns>分頁訂單列表</returns>
    Task<PaginatedResponse<OrderListDto>> GetOrdersAsync(
        PaginationRequest request,
        int? storeId = null,
        int? customerId = null);

    /// <summary>
    /// 取得訂單詳細資訊
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>訂單詳細資訊</returns>
    Task<OrderDetailDto?> GetOrderByIdAsync(int id);

    /// <summary>
    /// 根據訂單編號取得訂單
    /// </summary>
    /// <param name="orderNo">訂單編號</param>
    /// <returns>訂單詳細資訊</returns>
    Task<OrderDetailDto?> GetOrderByOrderNoAsync(string orderNo);

    /// <summary>
    /// 建立訂單
    /// </summary>
    /// <param name="request">建立訂單請求</param>
    /// <param name="userId">建立人 ID</param>
    /// <returns>建立的訂單 ID</returns>
    Task<int?> CreateOrderAsync(CreateOrderRequest request, int userId);

    /// <summary>
    /// 新增付款
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="request">付款請求</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>付款 ID</returns>
    Task<int?> AddPaymentAsync(int orderId, AddPaymentRequest request, int userId);

    /// <summary>
    /// 完成訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CompleteOrderAsync(int id, int userId);

    /// <summary>
    /// 取消訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelOrderAsync(int id, int userId);

    /// <summary>
    /// 取得客戶訂單歷史
    /// </summary>
    /// <param name="customerId">客戶 ID</param>
    /// <param name="request">分頁參數</param>
    /// <returns>訂單列表</returns>
    Task<PaginatedResponse<OrderListDto>> GetCustomerOrdersAsync(int customerId, PaginationRequest request);

    /// <summary>
    /// 取得待處理訂單列表
    /// </summary>
    Task<PaginatedResponse<PendingOrderDto>> GetPendingOrdersAsync(PaginationRequest request, int? storeId = null);

    /// <summary>
    /// 取得待處理訂單統計
    /// </summary>
    Task<PendingOrderSummaryDto> GetPendingOrderSummaryAsync(int? storeId = null);

    /// <summary>
    /// 開始處理訂單
    /// </summary>
    Task<bool> StartProcessingOrderAsync(int orderId, int userId);

    /// <summary>
    /// 完成訂單處理
    /// </summary>
    Task<bool> FinishProcessingOrderAsync(int orderId, int userId, ProcessOrderRequest? request = null);

    /// <summary>
    /// 更新訂單優先級
    /// </summary>
    Task<bool> UpdateOrderPriorityAsync(int orderId, string priority, int userId);
}
