using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 採購單控制器
/// </summary>
/// <remarks>
/// 處理採購單 CRUD 與核准操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ILogger<PurchaseOrdersController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrdersController> logger)
    {
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
    }

    /// <summary>
    /// 取得採購單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁採購單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PurchaseOrderListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseOrderListDto>>>> GetPurchaseOrders(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? supplierId = null)
    {
        var result = await _purchaseOrderService.GetPurchaseOrdersAsync(request, supplierId);
        return Ok(ApiResponse<PaginatedResponse<PurchaseOrderListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得採購單詳細資訊
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <returns>採購單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到採購單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetPurchaseOrder(int id)
    {
        var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
        if (purchaseOrder == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到採購單"));
        }

        return Ok(ApiResponse<PurchaseOrderDetailDto>.SuccessResponse(purchaseOrder));
    }

    /// <summary>
    /// 根據採購單號取得採購單
    /// </summary>
    /// <param name="poNo">採購單號</param>
    /// <returns>採購單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到採購單</response>
    [HttpGet("number/{poNo}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetPurchaseOrderByNumber(string poNo)
    {
        var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByPoNoAsync(poNo);
        if (purchaseOrder == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到採購單"));
        }

        return Ok(ApiResponse<PurchaseOrderDetailDto>.SuccessResponse(purchaseOrder));
    }

    /// <summary>
    /// 建立採購單
    /// </summary>
    /// <param name="request">建立採購單請求</param>
    /// <returns>建立的採購單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var purchaseOrderId = await _purchaseOrderService.CreatePurchaseOrderAsync(request, userId);
        if (purchaseOrderId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立採購單失敗"));
        }

        return CreatedAtAction(
            nameof(GetPurchaseOrder),
            new { id = purchaseOrderId },
            ApiResponse<int>.SuccessResponse(purchaseOrderId.Value, "採購單建立成功"));
    }

    /// <summary>
    /// 核准採購單
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">核准成功</response>
    /// <response code="400">核准失敗</response>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ApprovePurchaseOrder(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _purchaseOrderService.ApprovePurchaseOrderAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("核准採購單失敗，採購單可能不存在或狀態不正確"));
        }

        return Ok(ApiResponse.SuccessResponse("採購單已核准"));
    }

    /// <summary>
    /// 取消採購單
    /// </summary>
    /// <param name="id">採購單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelPurchaseOrder(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _purchaseOrderService.CancelPurchaseOrderAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消採購單失敗，採購單可能已完成或已取消"));
        }

        return Ok(ApiResponse.SuccessResponse("採購單已取消"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
