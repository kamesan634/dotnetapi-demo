using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 採購建議控制器
/// </summary>
/// <remarks>
/// 處理採購建議查詢和轉換採購單
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Manager,Purchaser")]
[Produces("application/json")]
public class PurchaseSuggestionsController : ControllerBase
{
    private readonly IPurchaseSuggestionService _purchaseSuggestionService;
    private readonly ILogger<PurchaseSuggestionsController> _logger;

    public PurchaseSuggestionsController(
        IPurchaseSuggestionService purchaseSuggestionService,
        ILogger<PurchaseSuggestionsController> logger)
    {
        _purchaseSuggestionService = purchaseSuggestionService;
        _logger = logger;
    }

    /// <summary>
    /// 取得採購建議列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="supplierId">供應商 ID (可選)</param>
    /// <returns>分頁採購建議列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PurchaseSuggestionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseSuggestionDto>>>> GetPurchaseSuggestions(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? warehouseId = null,
        [FromQuery] int? supplierId = null)
    {
        var result = await _purchaseSuggestionService.GetPurchaseSuggestionsAsync(request, warehouseId, supplierId);
        return Ok(ApiResponse<PaginatedResponse<PurchaseSuggestionDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得採購建議摘要
    /// </summary>
    /// <returns>採購建議摘要</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseSuggestionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PurchaseSuggestionSummaryDto>>> GetSuggestionSummary()
    {
        var result = await _purchaseSuggestionService.GetSuggestionSummaryAsync();
        return Ok(ApiResponse<PurchaseSuggestionSummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// 根據建議產生採購單
    /// </summary>
    /// <param name="request">產生採購單請求</param>
    /// <returns>產生的採購單 ID 列表</returns>
    /// <response code="201">產生成功</response>
    /// <response code="400">產生失敗</response>
    [HttpPost("generate-orders")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<int>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<int>>>> GeneratePurchaseOrders(
        [FromBody] GeneratePurchaseOrderRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.FailResponse("無法取得使用者資訊"));
        }

        var orderIds = await _purchaseSuggestionService.GeneratePurchaseOrdersFromSuggestionsAsync(request, userId.Value);

        if (!orderIds.Any())
        {
            return BadRequest(ApiResponse.FailResponse("產生採購單失敗，請確認商品有對應的供應商報價"));
        }

        return Created("", ApiResponse<IEnumerable<int>>.SuccessResponse(orderIds, $"成功產生 {orderIds.Count()} 張採購單"));
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
