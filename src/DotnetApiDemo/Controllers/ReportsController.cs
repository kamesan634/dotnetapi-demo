using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 報表控制器
/// </summary>
/// <remarks>
/// 處理各類營運報表查詢
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// 取得儀表板摘要
    /// </summary>
    /// <returns>儀表板摘要資料</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得銷售報表
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>銷售報表列表</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalesReportDto>>>> GetSalesReport(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        // 預設為最近 30 天
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetSalesReportAsync(start, end);
        return Ok(ApiResponse<IEnumerable<SalesReportDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得熱銷商品
    /// </summary>
    /// <param name="limit">數量限制 (預設: 10)</param>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>熱銷商品列表</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopProductDto>>>> GetTopProducts(
        [FromQuery] int limit = 10,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(ApiResponse.FailResponse("數量限制需介於 1 到 100 之間"));
        }

        // 預設為最近 30 天
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetTopProductsAsync(limit, start, end);
        return Ok(ApiResponse<IEnumerable<TopProductDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得庫存摘要
    /// </summary>
    /// <returns>庫存報表資料</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("inventory")]
    [ProducesResponseType(typeof(ApiResponse<InventoryReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryReportDto>>> GetInventorySummary()
    {
        var result = await _reportService.GetInventorySummaryAsync();
        return Ok(ApiResponse<InventoryReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得採購摘要
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>採購報表資料</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("purchasing")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PurchaseReportDto>>> GetPurchasingSummary(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        // 預設為最近 30 天
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetPurchaseSummaryAsync(start, end);
        return Ok(ApiResponse<PurchaseReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得客戶分析
    /// </summary>
    /// <returns>客戶報表資料</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(ApiResponse<CustomerReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerReportDto>>> GetCustomerSummary()
    {
        var result = await _reportService.GetCustomerSummaryAsync();
        return Ok(ApiResponse<CustomerReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得銷售趨勢
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>銷售趨勢資料</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("sales/trend")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesTrendDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalesTrendDto>>>> GetSalesTrend(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        // 預設為最近 30 天
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetSalesTrendAsync(start, end);
        return Ok(ApiResponse<IEnumerable<SalesTrendDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 匯出銷售報表 (CSV)
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("sales/export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportSalesReport(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var csvBytes = await _reportService.ExportSalesReportToCsvAsync(start, end);
        var fileName = $"銷售報表_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 匯出庫存報表 (CSV)
    /// </summary>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    [HttpGet("inventory/export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportInventoryReport()
    {
        var csvBytes = await _reportService.ExportInventoryReportToCsvAsync();
        var fileName = $"庫存報表_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 匯出熱銷商品報表 (CSV)
    /// </summary>
    /// <param name="limit">數量限制 (預設: 10)</param>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("top-products/export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportTopProducts(
        [FromQuery] int limit = 10,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(ApiResponse.FailResponse("數量限制需介於 1 到 100 之間"));
        }

        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var csvBytes = await _reportService.ExportTopProductsToCsvAsync(limit, start, end);
        var fileName = $"熱銷商品報表_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 匯出客戶分析報表 (CSV)
    /// </summary>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    [HttpGet("customers/export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCustomerReport()
    {
        var csvBytes = await _reportService.ExportCustomerReportToCsvAsync();
        var fileName = $"客戶分析報表_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 匯出採購報表 (CSV)
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("purchasing/export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPurchasingReport(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var csvBytes = await _reportService.ExportPurchaseReportToCsvAsync(start, end);
        var fileName = $"採購報表_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 取得利潤報表
    /// </summary>
    /// <param name="startDate">開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="endDate">結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>利潤報表資料</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("profit")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ProfitReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProfitReportDto>>> GetProfitReport(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetProfitReportAsync(start, end);
        return Ok(ApiResponse<ProfitReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得銷售比較報表
    /// </summary>
    /// <param name="period1Start">期間一開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="period1End">期間一結束日期 (格式: yyyy-MM-dd)</param>
    /// <param name="period2Start">期間二開始日期 (格式: yyyy-MM-dd)</param>
    /// <param name="period2End">期間二結束日期 (格式: yyyy-MM-dd)</param>
    /// <returns>比較報表資料</returns>
    /// <response code="200">取得成功</response>
    /// <response code="400">參數錯誤</response>
    [HttpGet("comparison")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ComparisonReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ComparisonReportDto>>> GetComparisonReport(
        [FromQuery] DateOnly period1Start,
        [FromQuery] DateOnly period1End,
        [FromQuery] DateOnly period2Start,
        [FromQuery] DateOnly period2End)
    {
        if (period1Start > period1End || period2Start > period2End)
        {
            return BadRequest(ApiResponse.FailResponse("開始日期不可大於結束日期"));
        }

        var result = await _reportService.GetComparisonReportAsync(period1Start, period1End, period2Start, period2End);
        return Ok(ApiResponse<ComparisonReportDto>.SuccessResponse(result));
    }
}
