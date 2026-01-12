using DotnetApiDemo.Models.DTOs.Reports;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 報表服務介面
/// </summary>
/// <remarks>
/// 提供各類營運報表查詢功能
/// </remarks>
public interface IReportService
{
    /// <summary>
    /// 取得儀表板摘要
    /// </summary>
    /// <returns>儀表板摘要資料</returns>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();

    /// <summary>
    /// 取得銷售報表
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>銷售報表列表</returns>
    Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 取得熱銷商品
    /// </summary>
    /// <param name="limit">數量限制</param>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>熱銷商品列表</returns>
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int limit, DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 取得庫存摘要
    /// </summary>
    /// <returns>庫存報表資料</returns>
    Task<InventoryReportDto> GetInventorySummaryAsync();

    /// <summary>
    /// 取得採購摘要
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>採購報表資料</returns>
    Task<PurchaseReportDto> GetPurchaseSummaryAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 取得客戶分析摘要
    /// </summary>
    /// <returns>客戶報表資料</returns>
    Task<CustomerReportDto> GetCustomerSummaryAsync();

    /// <summary>
    /// 取得銷售趨勢
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>銷售趨勢資料</returns>
    Task<IEnumerable<SalesTrendDto>> GetSalesTrendAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 匯出銷售報表為 CSV
    /// </summary>
    Task<byte[]> ExportSalesReportToCsvAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 匯出庫存報表為 CSV
    /// </summary>
    Task<byte[]> ExportInventoryReportToCsvAsync();

    /// <summary>
    /// 匯出熱銷商品報表為 CSV
    /// </summary>
    Task<byte[]> ExportTopProductsToCsvAsync(int limit, DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 匯出客戶報表為 CSV
    /// </summary>
    Task<byte[]> ExportCustomerReportToCsvAsync();

    /// <summary>
    /// 匯出採購報表為 CSV
    /// </summary>
    Task<byte[]> ExportPurchaseReportToCsvAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 取得利潤報表
    /// </summary>
    Task<ProfitReportDto> GetProfitReportAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 取得銷售比較報表
    /// </summary>
    Task<ComparisonReportDto> GetComparisonReportAsync(DateOnly period1Start, DateOnly period1End, DateOnly period2Start, DateOnly period2End);
}

/// <summary>
/// 利潤報表 DTO
/// </summary>
public class ProfitReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderProfit { get; set; }
    public IEnumerable<CategoryProfitDto> CategoryProfits { get; set; } = Enumerable.Empty<CategoryProfitDto>();
    public IEnumerable<DailyProfitDto> DailyProfits { get; set; } = Enumerable.Empty<DailyProfitDto>();
}

public class CategoryProfitDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
}

public class DailyProfitDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
}

/// <summary>
/// 比較報表 DTO
/// </summary>
public class ComparisonReportDto
{
    public PeriodSummaryDto Period1 { get; set; } = new();
    public PeriodSummaryDto Period2 { get; set; } = new();
    public decimal SalesGrowth { get; set; }
    public decimal OrderGrowth { get; set; }
    public decimal CustomerGrowth { get; set; }
    public decimal AovGrowth { get; set; }
}

public class PeriodSummaryDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
}
