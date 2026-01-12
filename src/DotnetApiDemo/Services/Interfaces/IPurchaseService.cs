using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 採購單服務介面
/// </summary>
public interface IPurchaseOrderService
{
    /// <summary>
    /// 取得採購單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁採購單列表</returns>
    Task<PaginatedResponse<PurchaseOrderListDto>> GetPurchaseOrdersAsync(
        PaginationRequest request,
        int? supplierId = null);

    /// <summary>
    /// 取得採購單詳細資訊
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <returns>採購單詳細資訊</returns>
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderByIdAsync(int id);

    /// <summary>
    /// 根據採購單號取得採購單
    /// </summary>
    /// <param name="poNo">採購單號</param>
    /// <returns>採購單詳細資訊</returns>
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderByPoNoAsync(string poNo);

    /// <summary>
    /// 建立採購單
    /// </summary>
    /// <param name="request">建立採購單請求</param>
    /// <param name="userId">建立人 ID</param>
    /// <returns>建立的採購單 ID</returns>
    Task<int?> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, int userId);

    /// <summary>
    /// 核准採購單
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <param name="userId">核准人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ApprovePurchaseOrderAsync(int id, int userId);

    /// <summary>
    /// 取消採購單
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelPurchaseOrderAsync(int id, int userId);
}

/// <summary>
/// 採購驗收服務介面
/// </summary>
public interface IPurchaseReceiptService
{
    /// <summary>
    /// 取得驗收單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="purchaseOrderId">採購單 ID (可選)</param>
    /// <returns>分頁驗收單列表</returns>
    Task<PaginatedResponse<PurchaseReceiptListDto>> GetPurchaseReceiptsAsync(
        PaginationRequest request,
        int? purchaseOrderId = null);

    /// <summary>
    /// 取得驗收單詳細資訊
    /// </summary>
    /// <param name="id">驗收單 ID</param>
    /// <returns>驗收單詳細資訊</returns>
    Task<PurchaseReceiptDetailDto?> GetPurchaseReceiptByIdAsync(int id);

    /// <summary>
    /// 建立驗收單
    /// </summary>
    /// <param name="request">建立驗收單請求</param>
    /// <param name="userId">驗收人 ID</param>
    /// <returns>建立的驗收單 ID</returns>
    Task<int?> CreatePurchaseReceiptAsync(CreatePurchaseReceiptRequest request, int userId);
}

/// <summary>
/// 採購退貨服務介面
/// </summary>
public interface IPurchaseReturnService
{
    /// <summary>
    /// 取得退貨單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁退貨單列表</returns>
    Task<PaginatedResponse<PurchaseReturnListDto>> GetPurchaseReturnsAsync(
        PaginationRequest request,
        int? supplierId = null);

    /// <summary>
    /// 取得退貨單詳細資訊
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>退貨單詳細資訊</returns>
    Task<PurchaseReturnDetailDto?> GetPurchaseReturnByIdAsync(int id);

    /// <summary>
    /// 建立退貨單
    /// </summary>
    /// <param name="request">建立退貨單請求</param>
    /// <param name="userId">申請人 ID</param>
    /// <returns>建立的退貨單 ID</returns>
    Task<int?> CreatePurchaseReturnAsync(CreatePurchaseReturnRequest request, int userId);

    /// <summary>
    /// 核准退貨單
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <param name="userId">核准人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ApprovePurchaseReturnAsync(int id, int userId);

    /// <summary>
    /// 完成退貨
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CompletePurchaseReturnAsync(int id, int userId);

    /// <summary>
    /// 取消退貨單
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <param name="userId">操作人 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> CancelPurchaseReturnAsync(int id, int userId);
}
