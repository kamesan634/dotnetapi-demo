using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Points;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PointsController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointsController(IPointService pointService)
    {
        _pointService = pointService;
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PointTransactionListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PointTransactionListDto>>>> GetTransactions(
        [FromQuery] PaginationRequest request, [FromQuery] int? customerId = null)
    {
        var result = await _pointService.GetTransactionsAsync(request, customerId);
        return Ok(ApiResponse<PaginatedResponse<PointTransactionListDto>>.SuccessResponse(result));
    }

    [HttpGet("balance/{customerId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PointBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PointBalanceDto>>> GetBalance(int customerId)
    {
        var balance = await _pointService.GetBalanceAsync(customerId);
        if (balance == null) return NotFound(ApiResponse.FailResponse("找不到客戶"));
        return Ok(ApiResponse<PointBalanceDto>.SuccessResponse(balance));
    }

    [HttpPost("earn")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> EarnPoints([FromBody] EarnPointsRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _pointService.EarnPointsAsync(request, userId);
        if (!success) return BadRequest(ApiResponse.FailResponse("點數獲得失敗"));
        return Ok(ApiResponse.SuccessResponse("點數獲得成功"));
    }

    [HttpPost("redeem")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _pointService.RedeemPointsAsync(request, userId);
        if (!success) return BadRequest(ApiResponse.FailResponse("點數兌換失敗，可能點數不足"));
        return Ok(ApiResponse.SuccessResponse("點數兌換成功"));
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> AdjustPoints([FromBody] AdjustPointsRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _pointService.AdjustPointsAsync(request, userId);
        if (!success) return BadRequest(ApiResponse.FailResponse("點數調整失敗"));
        return Ok(ApiResponse.SuccessResponse("點數調整成功"));
    }

    [HttpPost("expire")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> ExpirePoints()
    {
        await _pointService.ExpirePointsAsync();
        return Ok(ApiResponse.SuccessResponse("點數過期處理完成"));
    }
}
