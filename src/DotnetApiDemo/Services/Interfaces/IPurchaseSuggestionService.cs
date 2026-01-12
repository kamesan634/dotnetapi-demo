using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Models.DTOs.Purchasing;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 採購建議服務介面
/// </summary>
public interface IPurchaseSuggestionService
{
    /// <summary>
    /// 取得採購建議
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁採購建議列表</returns>
    Task<PaginatedResponse<PurchaseSuggestionDto>> GetPurchaseSuggestionsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? supplierId = null);

    /// <summary>
    /// 根據建議產生採購單
    /// </summary>
    /// <param name="request">產生採購單請求</param>
    /// <param name="userId">操作使用者 ID</param>
    /// <returns>產生的採購單 ID 列表</returns>
    Task<IEnumerable<int>> GeneratePurchaseOrdersFromSuggestionsAsync(
        GeneratePurchaseOrderRequest request,
        int userId);

    /// <summary>
    /// 取得採購建議摘要
    /// </summary>
    /// <returns>採購建議摘要</returns>
    Task<PurchaseSuggestionSummaryDto> GetSuggestionSummaryAsync();
}

/// <summary>
/// 採購建議 DTO
/// </summary>
public class PurchaseSuggestionDto
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫 ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 目前庫存
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// 安全庫存
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 缺口數量
    /// </summary>
    public int ShortageQuantity { get; set; }

    /// <summary>
    /// 建議採購數量
    /// </summary>
    public int SuggestedQuantity { get; set; }

    /// <summary>
    /// 優先供應商 ID
    /// </summary>
    public int? PreferredSupplierId { get; set; }

    /// <summary>
    /// 優先供應商名稱
    /// </summary>
    public string? PreferredSupplierName { get; set; }

    /// <summary>
    /// 參考單價
    /// </summary>
    public decimal? ReferencePrice { get; set; }

    /// <summary>
    /// 預估金額
    /// </summary>
    public decimal? EstimatedAmount { get; set; }

    /// <summary>
    /// 最後採購日期
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }

    /// <summary>
    /// 緊急程度 (Critical/Warning/Normal)
    /// </summary>
    public string UrgencyLevel { get; set; } = "Normal";
}

/// <summary>
/// 產生採購單請求 DTO
/// </summary>
public class GeneratePurchaseOrderRequest
{
    /// <summary>
    /// 商品 ID 列表
    /// </summary>
    public IEnumerable<int> ProductIds { get; set; } = Enumerable.Empty<int>();

    /// <summary>
    /// 倉庫 ID (可選，不指定則依商品的低庫存倉庫)
    /// </summary>
    public int? WarehouseId { get; set; }

    /// <summary>
    /// 是否依供應商分組
    /// </summary>
    public bool GroupBySupplier { get; set; } = true;

    /// <summary>
    /// 期望到貨日
    /// </summary>
    public DateOnly? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 採購建議摘要 DTO
/// </summary>
public class PurchaseSuggestionSummaryDto
{
    /// <summary>
    /// 需採購商品數量
    /// </summary>
    public int TotalProductCount { get; set; }

    /// <summary>
    /// 緊急商品數量
    /// </summary>
    public int CriticalCount { get; set; }

    /// <summary>
    /// 警示商品數量
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// 一般商品數量
    /// </summary>
    public int NormalCount { get; set; }

    /// <summary>
    /// 預估總金額
    /// </summary>
    public decimal EstimatedTotalAmount { get; set; }

    /// <summary>
    /// 涉及供應商數量
    /// </summary>
    public int SupplierCount { get; set; }
}
