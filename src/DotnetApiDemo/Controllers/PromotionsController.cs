using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 促銷活動控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ILogger<PromotionsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PromotionsController(IPromotionService promotionService, ILogger<PromotionsController> logger)
    {
        _promotionService = promotionService;
        _logger = logger;
    }

    /// <summary>
    /// 取得促銷活動列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁促銷活動列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PromotionListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PromotionListDto>>>> GetPromotions(
        [FromQuery] PaginationRequest request)
    {
        var result = await _promotionService.GetPromotionsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<PromotionListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得促銷活動詳細資訊
    /// </summary>
    /// <param name="id">促銷活動 ID</param>
    /// <returns>促銷活動詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到促銷活動</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PromotionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromotionDetailDto>>> GetPromotion(int id)
    {
        var promotion = await _promotionService.GetPromotionByIdAsync(id);
        if (promotion == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到促銷活動"));
        }

        return Ok(ApiResponse<PromotionDetailDto>.SuccessResponse(promotion));
    }

    /// <summary>
    /// 建立促銷活動
    /// </summary>
    /// <param name="request">建立促銷活動請求</param>
    /// <returns>建立的促銷活動 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var promotionId = await _promotionService.CreatePromotionAsync(request);
        if (promotionId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立促銷活動失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetPromotion),
            new { id = promotionId },
            ApiResponse<int>.SuccessResponse(promotionId.Value, "促銷活動建立成功"));
    }

    /// <summary>
    /// 更新促銷活動
    /// </summary>
    /// <param name="id">促銷活動 ID</param>
    /// <param name="request">更新促銷活動請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到促銷活動</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
    {
        var success = await _promotionService.UpdatePromotionAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到促銷活動"));
        }

        return Ok(ApiResponse.SuccessResponse("促銷活動更新成功"));
    }

    /// <summary>
    /// 刪除促銷活動
    /// </summary>
    /// <param name="id">促銷活動 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeletePromotion(int id)
    {
        var success = await _promotionService.DeletePromotionAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除促銷活動失敗，促銷活動可能不存在或有已使用的優惠券"));
        }

        return Ok(ApiResponse.SuccessResponse("促銷活動刪除成功"));
    }
}
