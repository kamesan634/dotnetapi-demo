using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.CashierShifts;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class CashierShiftsController : ControllerBase
{
    private readonly ICashierShiftService _shiftService;

    public CashierShiftsController(ICashierShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CashierShiftListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CashierShiftListDto>>>> GetShifts(
        [FromQuery] PaginationRequest request)
    {
        var result = await _shiftService.GetShiftsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CashierShiftListDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CashierShiftDetailDto>>> GetShift(int id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift == null) return NotFound(ApiResponse.FailResponse("找不到班別"));
        return Ok(ApiResponse<CashierShiftDetailDto>.SuccessResponse(shift));
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CashierShiftDetailDto?>>> GetCurrentShift()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var shift = await _shiftService.GetCurrentShiftAsync(userId);
        return Ok(ApiResponse<CashierShiftDetailDto?>.SuccessResponse(shift));
    }

    [HttpPost("open")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> OpenShift([FromBody] OpenShiftRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var shiftId = await _shiftService.OpenShiftAsync(request, userId);
        if (shiftId == null) return BadRequest(ApiResponse.FailResponse("開班失敗"));
        return CreatedAtAction(nameof(GetShift), new { id = shiftId }, ApiResponse<int>.SuccessResponse(shiftId.Value, "開班成功"));
    }

    [HttpPost("{id:int}/close")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CloseShift(int id, [FromBody] CloseShiftRequest request)
    {
        var success = await _shiftService.CloseShiftAsync(id, request);
        if (!success) return BadRequest(ApiResponse.FailResponse("結班失敗"));
        return Ok(ApiResponse.SuccessResponse("結班成功"));
    }
}
