using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 掛單控制器
/// </summary>
/// <remarks>
/// 處理掛單的建立、恢復、取消等操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SuspendedOrdersController : ControllerBase
{
    private readonly ISuspendedOrderService _suspendedOrderService;
    private readonly ILogger<SuspendedOrdersController> _logger;

    public SuspendedOrdersController(
        ISuspendedOrderService suspendedOrderService,
        ILogger<SuspendedOrdersController> logger)
    {
        _suspendedOrderService = suspendedOrderService;
        _logger = logger;
    }

    /// <summary>
    /// 取得掛單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <param name="pendingOnly">是否只取待處理的掛單</param>
    /// <returns>分頁掛單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SuspendedOrderListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SuspendedOrderListDto>>>> GetSuspendedOrders(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? storeId = null,
        [FromQuery] bool pendingOnly = true)
    {
        var result = await _suspendedOrderService.GetSuspendedOrdersAsync(request, storeId, pendingOnly);
        return Ok(ApiResponse<PaginatedResponse<SuspendedOrderListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得掛單詳細資訊
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <returns>掛單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到掛單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SuspendedOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuspendedOrderDetailDto>>> GetSuspendedOrder(int id)
    {
        var order = await _suspendedOrderService.GetSuspendedOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到掛單"));
        }

        return Ok(ApiResponse<SuspendedOrderDetailDto>.SuccessResponse(order));
    }

    /// <summary>
    /// 根據編號取得掛單
    /// </summary>
    /// <param name="orderNo">掛單編號</param>
    /// <returns>掛單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到掛單</response>
    [HttpGet("no/{orderNo}")]
    [ProducesResponseType(typeof(ApiResponse<SuspendedOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuspendedOrderDetailDto>>> GetSuspendedOrderByNo(string orderNo)
    {
        var order = await _suspendedOrderService.GetSuspendedOrderByNoAsync(orderNo);
        if (order == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到掛單"));
        }

        return Ok(ApiResponse<SuspendedOrderDetailDto>.SuccessResponse(order));
    }

    /// <summary>
    /// 建立掛單
    /// </summary>
    /// <param name="request">建立掛單請求</param>
    /// <returns>建立的掛單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateSuspendedOrder([FromBody] CreateSuspendedOrderRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.FailResponse("無法取得使用者資訊"));
        }

        var orderId = await _suspendedOrderService.CreateSuspendedOrderAsync(request, userId.Value);
        if (orderId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立掛單失敗，請檢查商品資訊"));
        }

        return CreatedAtAction(
            nameof(GetSuspendedOrder),
            new { id = orderId },
            ApiResponse<int>.SuccessResponse(orderId.Value, "掛單建立成功"));
    }

    /// <summary>
    /// 恢復掛單
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <returns>恢復的掛單詳細資訊</returns>
    /// <response code="200">恢復成功</response>
    /// <response code="400">恢復失敗</response>
    [HttpPost("{id:int}/resume")]
    [ProducesResponseType(typeof(ApiResponse<SuspendedOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuspendedOrderDetailDto>>> ResumeSuspendedOrder(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.FailResponse("無法取得使用者資訊"));
        }

        var order = await _suspendedOrderService.ResumeSuspendedOrderAsync(id, userId.Value);
        if (order == null)
        {
            return BadRequest(ApiResponse.FailResponse("恢復掛單失敗，掛單可能不存在、已過期或狀態不正確"));
        }

        return Ok(ApiResponse<SuspendedOrderDetailDto>.SuccessResponse(order, "掛單恢復成功"));
    }

    /// <summary>
    /// 取消掛單
    /// </summary>
    /// <param name="id">掛單 ID</param>
    /// <param name="reason">取消原因 (可選)</param>
    /// <returns>取消結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelSuspendedOrder(int id, [FromQuery] string? reason = null)
    {
        var success = await _suspendedOrderService.CancelSuspendedOrderAsync(id, reason);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消掛單失敗，掛單可能不存在或狀態不正確"));
        }

        return Ok(ApiResponse.SuccessResponse("掛單取消成功"));
    }

    /// <summary>
    /// 取得門市待處理掛單數量
    /// </summary>
    /// <param name="storeId">門市 ID</param>
    /// <returns>待處理掛單數量</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("pending-count/{storeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetPendingCount(int storeId)
    {
        var count = await _suspendedOrderService.GetPendingCountAsync(storeId);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
