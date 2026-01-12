using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 訂單控制器
/// </summary>
/// <remarks>
/// 處理訂單 CRUD 與付款操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// 取得訂單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <param name="customerId">客戶 ID (可選)</param>
    /// <returns>分頁訂單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<OrderListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<OrderListDto>>>> GetOrders(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? storeId = null,
        [FromQuery] int? customerId = null)
    {
        var result = await _orderService.GetOrdersAsync(request, storeId, customerId);
        return Ok(ApiResponse<PaginatedResponse<OrderListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得訂單詳細資訊
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>訂單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到訂單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到訂單"));
        }

        return Ok(ApiResponse<OrderDetailDto>.SuccessResponse(order));
    }

    /// <summary>
    /// 根據訂單編號取得訂單
    /// </summary>
    /// <param name="orderNo">訂單編號</param>
    /// <returns>訂單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到訂單</response>
    [HttpGet("number/{orderNo}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderByNumber(string orderNo)
    {
        var order = await _orderService.GetOrderByOrderNoAsync(orderNo);
        if (order == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到訂單"));
        }

        return Ok(ApiResponse<OrderDetailDto>.SuccessResponse(order));
    }

    /// <summary>
    /// 建立訂單
    /// </summary>
    /// <param name="request">建立訂單請求</param>
    /// <returns>建立的訂單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var orderId = await _orderService.CreateOrderAsync(request, userId);
        if (orderId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立訂單失敗"));
        }

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = orderId },
            ApiResponse<int>.SuccessResponse(orderId.Value, "訂單建立成功"));
    }

    /// <summary>
    /// 新增付款
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <param name="request">付款請求</param>
    /// <returns>付款 ID</returns>
    /// <response code="200">付款成功</response>
    /// <response code="400">付款失敗</response>
    [HttpPost("{id:int}/payments")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> AddPayment(int id, [FromBody] AddPaymentRequest request)
    {
        var userId = GetCurrentUserId();
        var paymentId = await _orderService.AddPaymentAsync(id, request, userId);
        if (paymentId == null)
        {
            return BadRequest(ApiResponse.FailResponse("付款失敗，訂單可能不存在或已取消"));
        }

        return Ok(ApiResponse<int>.SuccessResponse(paymentId.Value, "付款成功"));
    }

    /// <summary>
    /// 完成訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">完成成功</response>
    /// <response code="400">完成失敗</response>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CompleteOrder(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _orderService.CompleteOrderAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("完成訂單失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("訂單已完成"));
    }

    /// <summary>
    /// 取消訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelOrder(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _orderService.CancelOrderAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消訂單失敗，訂單可能已完成"));
        }

        return Ok(ApiResponse.SuccessResponse("訂單已取消"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }

    /// <summary>
    /// 取得待處理訂單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <returns>待處理訂單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PendingOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PendingOrderDto>>>> GetPendingOrders(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? storeId = null)
    {
        var result = await _orderService.GetPendingOrdersAsync(request, storeId);
        return Ok(ApiResponse<PaginatedResponse<PendingOrderDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得待處理訂單統計
    /// </summary>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <returns>待處理訂單統計</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("pending/summary")]
    [ProducesResponseType(typeof(ApiResponse<PendingOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PendingOrderSummaryDto>>> GetPendingOrderSummary(
        [FromQuery] int? storeId = null)
    {
        var result = await _orderService.GetPendingOrderSummaryAsync(storeId);
        return Ok(ApiResponse<PendingOrderSummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 開始處理訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">處理成功</response>
    /// <response code="400">處理失敗</response>
    [HttpPost("{id:int}/start-processing")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> StartProcessingOrder(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _orderService.StartProcessingOrderAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("開始處理訂單失敗，訂單可能不存在或已在處理中"));
        }

        return Ok(ApiResponse.SuccessResponse("已開始處理訂單"));
    }

    /// <summary>
    /// 完成訂單處理
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <param name="request">處理請求 (可選)</param>
    /// <returns>操作結果</returns>
    /// <response code="200">完成成功</response>
    /// <response code="400">完成失敗</response>
    [HttpPost("{id:int}/finish-processing")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> FinishProcessingOrder(int id, [FromBody] ProcessOrderRequest? request = null)
    {
        var userId = GetCurrentUserId();
        var success = await _orderService.FinishProcessingOrderAsync(id, userId, request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("完成訂單處理失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("訂單處理已完成"));
    }

    /// <summary>
    /// 更新訂單優先級
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <param name="request">優先級請求</param>
    /// <returns>操作結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("{id:int}/priority")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateOrderPriority(int id, [FromBody] UpdateOrderPriorityRequest request)
    {
        var userId = GetCurrentUserId();
        var success = await _orderService.UpdateOrderPriorityAsync(id, request.Priority, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新訂單優先級失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("訂單優先級已更新"));
    }
}
