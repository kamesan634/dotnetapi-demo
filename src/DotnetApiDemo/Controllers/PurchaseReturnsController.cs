using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 採購退貨單控制器
/// </summary>
/// <remarks>
/// 處理採購退貨單的建立、核准、完成與取消操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IPurchaseReturnService _purchaseReturnService;
    private readonly ILogger<PurchaseReturnsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="purchaseReturnService">採購退貨服務</param>
    /// <param name="logger">日誌服務</param>
    public PurchaseReturnsController(
        IPurchaseReturnService purchaseReturnService,
        ILogger<PurchaseReturnsController> logger)
    {
        _purchaseReturnService = purchaseReturnService;
        _logger = logger;
    }

    /// <summary>
    /// 取得退貨單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁退貨單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PurchaseReturnListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseReturnListDto>>>> GetPurchaseReturns(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? supplierId = null)
    {
        var result = await _purchaseReturnService.GetPurchaseReturnsAsync(request, supplierId);
        return Ok(ApiResponse<PaginatedResponse<PurchaseReturnListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得退貨單詳細資訊
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>退貨單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到退貨單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PurchaseReturnDetailDto>>> GetPurchaseReturn(int id)
    {
        var purchaseReturn = await _purchaseReturnService.GetPurchaseReturnByIdAsync(id);
        if (purchaseReturn == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到退貨單"));
        }

        return Ok(ApiResponse<PurchaseReturnDetailDto>.SuccessResponse(purchaseReturn));
    }

    /// <summary>
    /// 建立退貨單
    /// </summary>
    /// <param name="request">建立退貨單請求</param>
    /// <returns>建立的退貨單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Purchaser,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePurchaseReturn([FromBody] CreatePurchaseReturnRequest request)
    {
        var userId = GetCurrentUserId();
        var returnId = await _purchaseReturnService.CreatePurchaseReturnAsync(request, userId);
        if (returnId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立退貨單失敗"));
        }

        return CreatedAtAction(
            nameof(GetPurchaseReturn),
            new { id = returnId },
            ApiResponse<int>.SuccessResponse(returnId.Value, "退貨單建立成功"));
    }

    /// <summary>
    /// 核准退貨單
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">核准成功</response>
    /// <response code="400">核准失敗</response>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ApprovePurchaseReturn(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _purchaseReturnService.ApprovePurchaseReturnAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("核准退貨單失敗，退貨單可能不存在或狀態不正確"));
        }

        return Ok(ApiResponse.SuccessResponse("退貨單已核准"));
    }

    /// <summary>
    /// 完成退貨
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">完成成功</response>
    /// <response code="400">完成失敗</response>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CompletePurchaseReturn(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _purchaseReturnService.CompletePurchaseReturnAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("完成退貨失敗，退貨單可能不存在或狀態不正確"));
        }

        return Ok(ApiResponse.SuccessResponse("退貨已完成"));
    }

    /// <summary>
    /// 取消退貨單
    /// </summary>
    /// <param name="id">退貨單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelPurchaseReturn(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _purchaseReturnService.CancelPurchaseReturnAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消退貨單失敗，退貨單可能已完成或不存在"));
        }

        return Ok(ApiResponse.SuccessResponse("退貨單已取消"));
    }

    /// <summary>
    /// 取得當前使用者 ID
    /// </summary>
    /// <returns>使用者 ID</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
