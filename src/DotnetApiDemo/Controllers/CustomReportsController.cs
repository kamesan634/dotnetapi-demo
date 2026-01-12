using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/reports/custom")]
[Authorize]
[Produces("application/json")]
public class CustomReportsController : ControllerBase
{
    private readonly ICustomReportService _reportService;
    private readonly ILogger<CustomReportsController> _logger;

    public CustomReportsController(ICustomReportService reportService, ILogger<CustomReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    #region Custom Reports

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CustomReportListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CustomReportListDto>>>> GetCustomReports(
        [FromQuery] PaginationRequest request,
        [FromQuery] bool includePublic = true)
    {
        var result = await _reportService.GetCustomReportsAsync(request, GetCurrentUserId(), includePublic);
        return Ok(ApiResponse<PaginatedResponse<CustomReportListDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomReportDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomReportDetailDto>>> GetCustomReport(int id)
    {
        var report = await _reportService.GetCustomReportByIdAsync(id);
        if (report == null)
            return NotFound(ApiResponse.FailResponse("找不到自訂報表"));

        return Ok(ApiResponse<CustomReportDetailDto>.SuccessResponse(report));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCustomReport([FromBody] CreateCustomReportRequest request)
    {
        var reportId = await _reportService.CreateCustomReportAsync(request, GetCurrentUserId());
        if (reportId == null)
            return BadRequest(ApiResponse.FailResponse("建立自訂報表失敗，代碼可能已存在"));

        return CreatedAtAction(
            nameof(GetCustomReport),
            new { id = reportId },
            ApiResponse<int>.SuccessResponse(reportId.Value, "自訂報表建立成功"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateCustomReport(int id, [FromBody] UpdateCustomReportRequest request)
    {
        var success = await _reportService.UpdateCustomReportAsync(id, request, GetCurrentUserId());
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到自訂報表"));

        return Ok(ApiResponse.SuccessResponse("自訂報表更新成功"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteCustomReport(int id)
    {
        var success = await _reportService.DeleteCustomReportAsync(id, GetCurrentUserId());
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到自訂報表"));

        return Ok(ApiResponse.SuccessResponse("自訂報表刪除成功"));
    }

    [HttpPost("{id:int}/run")]
    [ProducesResponseType(typeof(ApiResponse<CustomReportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomReportResultDto>>> RunCustomReport(
        int id,
        [FromBody] RunCustomReportRequest? request = null)
    {
        var result = await _reportService.RunCustomReportAsync(id, request);
        if (result == null)
            return NotFound(ApiResponse.FailResponse("找不到自訂報表"));

        return Ok(ApiResponse<CustomReportResultDto>.SuccessResponse(result));
    }

    [HttpGet("{id:int}/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportCustomReport(int id, [FromQuery] string format = "csv")
    {
        var data = await _reportService.ExportCustomReportAsync(id, format, null);
        if (data == null)
            return NotFound(ApiResponse.FailResponse("找不到自訂報表"));

        var contentType = format.ToLower() == "csv" ? "text/csv; charset=utf-8" : "application/json";
        var fileName = $"report_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        return File(data, contentType, fileName);
    }

    #endregion

    #region Scheduled Reports

    [HttpGet("scheduled")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ScheduledReportListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ScheduledReportListDto>>>> GetScheduledReports(
        [FromQuery] PaginationRequest request)
    {
        var result = await _reportService.GetScheduledReportsAsync(request, GetCurrentUserId());
        return Ok(ApiResponse<PaginatedResponse<ScheduledReportListDto>>.SuccessResponse(result));
    }

    [HttpGet("scheduled/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ScheduledReportDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScheduledReportDetailDto>>> GetScheduledReport(int id)
    {
        var report = await _reportService.GetScheduledReportByIdAsync(id);
        if (report == null)
            return NotFound(ApiResponse.FailResponse("找不到排程報表"));

        return Ok(ApiResponse<ScheduledReportDetailDto>.SuccessResponse(report));
    }

    [HttpPost("scheduled")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateScheduledReport([FromBody] CreateScheduledReportRequest request)
    {
        var reportId = await _reportService.CreateScheduledReportAsync(request, GetCurrentUserId());
        if (reportId == null)
            return BadRequest(ApiResponse.FailResponse("建立排程報表失敗"));

        return CreatedAtAction(
            nameof(GetScheduledReport),
            new { id = reportId },
            ApiResponse<int>.SuccessResponse(reportId.Value, "排程報表建立成功"));
    }

    [HttpPut("scheduled/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateScheduledReport(int id, [FromBody] UpdateScheduledReportRequest request)
    {
        var success = await _reportService.UpdateScheduledReportAsync(id, request, GetCurrentUserId());
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到排程報表"));

        return Ok(ApiResponse.SuccessResponse("排程報表更新成功"));
    }

    [HttpDelete("scheduled/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteScheduledReport(int id)
    {
        var success = await _reportService.DeleteScheduledReportAsync(id, GetCurrentUserId());
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到排程報表"));

        return Ok(ApiResponse.SuccessResponse("排程報表刪除成功"));
    }

    [HttpPost("scheduled/{id:int}/run")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RunScheduledReportNow(int id)
    {
        var success = await _reportService.RunScheduledReportNowAsync(id);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("執行排程報表失敗"));

        return Ok(ApiResponse.SuccessResponse("排程報表已執行"));
    }

    [HttpGet("scheduled/{id:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduledReportHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ScheduledReportHistoryDto>>>> GetScheduledReportHistory(
        int id,
        [FromQuery] int limit = 10)
    {
        var history = await _reportService.GetScheduledReportHistoryAsync(id, limit);
        return Ok(ApiResponse<IEnumerable<ScheduledReportHistoryDto>>.SuccessResponse(history));
    }

    #endregion
}
