using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 計量單位控制器
/// </summary>
/// <remarks>
/// 處理計量單位 CRUD 操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    /// <summary>
    /// 取得單位列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="activeOnly">是否只取啟用的單位</param>
    /// <returns>分頁單位列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<UnitListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UnitListDto>>>> GetUnits(
        [FromQuery] PaginationRequest request,
        [FromQuery] bool? activeOnly = null)
    {
        var result = await _unitService.GetUnitsAsync(request, activeOnly);
        return Ok(ApiResponse<PaginatedResponse<UnitListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得所有啟用的單位（不分頁）
    /// </summary>
    /// <returns>單位列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UnitDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetActiveUnits()
    {
        var result = await _unitService.GetActiveUnitsAsync();
        return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得單位詳細資訊
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <returns>單位詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到單位</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UnitDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UnitDetailDto>>> GetUnit(int id)
    {
        var unit = await _unitService.GetUnitByIdAsync(id);
        if (unit == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到單位"));
        }

        return Ok(ApiResponse<UnitDetailDto>.SuccessResponse(unit));
    }

    /// <summary>
    /// 根據代碼取得單位
    /// </summary>
    /// <param name="code">單位代碼</param>
    /// <returns>單位資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到單位</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UnitDto>>> GetUnitByCode(string code)
    {
        var unit = await _unitService.GetUnitByCodeAsync(code);
        if (unit == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到單位"));
        }

        return Ok(ApiResponse<UnitDto>.SuccessResponse(unit));
    }

    /// <summary>
    /// 建立單位
    /// </summary>
    /// <param name="request">建立單位請求</param>
    /// <returns>建立的單位 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateUnit([FromBody] CreateUnitRequest request)
    {
        var unitId = await _unitService.CreateUnitAsync(request);
        if (unitId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立單位失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetUnit),
            new { id = unitId },
            ApiResponse<int>.SuccessResponse(unitId.Value, "單位建立成功"));
    }

    /// <summary>
    /// 更新單位
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <param name="request">更新單位請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到單位</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUnit(int id, [FromBody] UpdateUnitRequest request)
    {
        var success = await _unitService.UpdateUnitAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到單位"));
        }

        return Ok(ApiResponse.SuccessResponse("單位更新成功"));
    }

    /// <summary>
    /// 刪除單位
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteUnit(int id)
    {
        var success = await _unitService.DeleteUnitAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除單位失敗，單位可能不存在、為系統單位或有商品使用此單位"));
        }

        return Ok(ApiResponse.SuccessResponse("單位刪除成功"));
    }
}
