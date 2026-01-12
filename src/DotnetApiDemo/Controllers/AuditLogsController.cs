using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.AuditLogs;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 稽核日誌控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// 取得稽核日誌列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AuditLogListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AuditLogListDto>>>> GetLogs(
        [FromQuery] AuditLogQueryRequest request)
    {
        var result = await _auditLogService.GetLogsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<AuditLogListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得稽核日誌詳細資訊
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetLog(int id)
    {
        var log = await _auditLogService.GetLogByIdAsync(id);
        if (log == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到日誌"));
        }
        return Ok(ApiResponse<AuditLogDetailDto>.SuccessResponse(log));
    }

    /// <summary>
    /// 取得所有操作類型
    /// </summary>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetActions()
    {
        var actions = await _auditLogService.GetActionsAsync();
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(actions));
    }

    /// <summary>
    /// 取得所有實體類型
    /// </summary>
    [HttpGet("entity-types")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetEntityTypes()
    {
        var entityTypes = await _auditLogService.GetEntityTypesAsync();
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(entityTypes));
    }
}
