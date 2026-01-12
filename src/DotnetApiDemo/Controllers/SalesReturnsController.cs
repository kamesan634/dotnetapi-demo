using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SalesReturns;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 銷售退貨控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SalesReturnsController : ControllerBase
{
    private readonly ISalesReturnService _salesReturnService;
    private readonly ILogger<SalesReturnsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SalesReturnsController(ISalesReturnService salesReturnService, ILogger<SalesReturnsController> logger)
    {
        _salesReturnService = salesReturnService;
        _logger = logger;
    }

    /// <summary>
    /// 取得退貨單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁退貨單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SalesReturnListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SalesReturnListDto>>>> GetSalesReturns(
        [FromQuery] PaginationRequest request)
    {
        var result = await _salesReturnService.GetSalesReturnsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<SalesReturnListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得退貨單詳細資訊
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>退貨單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到退貨單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SalesReturnDetailDto>>> GetSalesReturn(int id)
    {
        var salesReturn = await _salesReturnService.GetSalesReturnByIdAsync(id);
        if (salesReturn == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到退貨單"));
        }

        return Ok(ApiResponse<SalesReturnDetailDto>.SuccessResponse(salesReturn));
    }

    /// <summary>
    /// 依退貨單號取得退貨單
    /// </summary>
    /// <param name="returnNumber">退貨單號</param>
    /// <returns>退貨單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到退貨單</response>
    [HttpGet("number/{returnNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SalesReturnDetailDto>>> GetSalesReturnByNumber(string returnNumber)
    {
        var salesReturn = await _salesReturnService.GetSalesReturnByNumberAsync(returnNumber);
        if (salesReturn == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到退貨單"));
        }

        return Ok(ApiResponse<SalesReturnDetailDto>.SuccessResponse(salesReturn));
    }

    /// <summary>
    /// 取得訂單可退貨商品
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <returns>可退貨商品列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("orders/{orderId:int}/returnable-items")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemForReturnDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemForReturnDto>>>> GetReturnableItems(int orderId)
    {
        var items = await _salesReturnService.GetReturnableItemsAsync(orderId);
        return Ok(ApiResponse<IEnumerable<OrderItemForReturnDto>>.SuccessResponse(items));
    }

    /// <summary>
    /// 建立退貨單
    /// </summary>
    /// <param name="request">建立退貨單請求</param>
    /// <returns>建立的退貨單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateSalesReturn([FromBody] CreateSalesReturnRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var salesReturnId = await _salesReturnService.CreateSalesReturnAsync(request, userId);

        if (salesReturnId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立退貨單失敗，請確認訂單狀態及退貨數量"));
        }

        return CreatedAtAction(
            nameof(GetSalesReturn),
            new { id = salesReturnId },
            ApiResponse<int>.SuccessResponse(salesReturnId.Value, "退貨單建立成功"));
    }

    /// <summary>
    /// 處理退貨單 (核准/拒絕)
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <param name="request">處理退貨請求</param>
    /// <returns>處理結果</returns>
    /// <response code="200">處理成功</response>
    /// <response code="400">處理失敗</response>
    [HttpPost("{id:int}/process")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ProcessSalesReturn(int id, [FromBody] ProcessSalesReturnRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _salesReturnService.ProcessSalesReturnAsync(id, request, userId);

        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("處理退貨單失敗，請確認退貨單狀態"));
        }

        var message = request.Approve ? "退貨單已核准" : "退貨單已拒絕";
        return Ok(ApiResponse.SuccessResponse(message));
    }

    /// <summary>
    /// 執行退款
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <param name="request">退款請求</param>
    /// <returns>退款結果</returns>
    /// <response code="200">退款成功</response>
    /// <response code="400">退款失敗</response>
    [HttpPost("{id:int}/refund")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Refund(int id, [FromBody] RefundRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _salesReturnService.RefundAsync(id, request, userId);

        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("退款失敗，請確認退貨單狀態"));
        }

        return Ok(ApiResponse.SuccessResponse("退款成功"));
    }

    /// <summary>
    /// 取消退貨單
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>取消結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelSalesReturn(int id)
    {
        var success = await _salesReturnService.CancelSalesReturnAsync(id);

        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消退貨單失敗，退貨單可能已完成或已退款"));
        }

        return Ok(ApiResponse.SuccessResponse("退貨單已取消"));
    }
}
