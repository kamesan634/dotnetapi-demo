using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SalesReturns;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 銷售退貨服務介面
/// </summary>
public interface ISalesReturnService
{
    /// <summary>
    /// 取得退貨單列表
    /// </summary>
    Task<PaginatedResponse<SalesReturnListDto>> GetSalesReturnsAsync(PaginationRequest request);

    /// <summary>
    /// 取得退貨單詳細資訊
    /// </summary>
    Task<SalesReturnDetailDto?> GetSalesReturnByIdAsync(int id);

    /// <summary>
    /// 依退貨單號取得退貨單
    /// </summary>
    Task<SalesReturnDetailDto?> GetSalesReturnByNumberAsync(string returnNumber);

    /// <summary>
    /// 建立退貨單
    /// </summary>
    Task<int?> CreateSalesReturnAsync(CreateSalesReturnRequest request, int userId);

    /// <summary>
    /// 處理退貨單 (核准/拒絕)
    /// </summary>
    Task<bool> ProcessSalesReturnAsync(int id, ProcessSalesReturnRequest request, int userId);

    /// <summary>
    /// 執行退款
    /// </summary>
    Task<bool> RefundAsync(int id, RefundRequest request, int userId);

    /// <summary>
    /// 取消退貨單
    /// </summary>
    Task<bool> CancelSalesReturnAsync(int id);

    /// <summary>
    /// 取得訂單可退貨商品
    /// </summary>
    Task<IEnumerable<OrderItemForReturnDto>> GetReturnableItemsAsync(int orderId);
}

/// <summary>
/// 訂單可退貨商品 DTO
/// </summary>
public class OrderItemForReturnDto
{
    /// <summary>
    /// 訂單明細 ID
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品 SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 購買數量
    /// </summary>
    public int PurchasedQuantity { get; set; }

    /// <summary>
    /// 已退貨數量
    /// </summary>
    public int ReturnedQuantity { get; set; }

    /// <summary>
    /// 可退貨數量
    /// </summary>
    public int ReturnableQuantity { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    public decimal UnitPrice { get; set; }
}
