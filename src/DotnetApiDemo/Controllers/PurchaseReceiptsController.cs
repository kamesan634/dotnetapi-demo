using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 採購驗收單控制器
/// </summary>
/// <remarks>
/// 處理採購驗收單的查詢與建立操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PurchaseReceiptsController : ControllerBase
{
    private readonly IPurchaseReceiptService _purchaseReceiptService;
    private readonly ILogger<PurchaseReceiptsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PurchaseReceiptsController(
        IPurchaseReceiptService purchaseReceiptService,
        ILogger<PurchaseReceiptsController> logger)
    {
        _purchaseReceiptService = purchaseReceiptService;
        _logger = logger;
    }

    /// <summary>
    /// 取得驗收單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="purchaseOrderId">採購單 ID (可選)</param>
    /// <returns>分頁驗收單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PurchaseReceiptListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseReceiptListDto>>>> GetPurchaseReceipts(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? purchaseOrderId = null)
    {
        var result = await _purchaseReceiptService.GetPurchaseReceiptsAsync(request, purchaseOrderId);
        return Ok(ApiResponse<PaginatedResponse<PurchaseReceiptListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得驗收單詳細資訊
    /// </summary>
    /// <param name="id">驗收單 ID</param>
    /// <returns>驗收單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到驗收單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReceiptDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PurchaseReceiptDetailDto>>> GetPurchaseReceipt(int id)
    {
        var receipt = await _purchaseReceiptService.GetPurchaseReceiptByIdAsync(id);
        if (receipt == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到驗收單"));
        }

        return Ok(ApiResponse<PurchaseReceiptDetailDto>.SuccessResponse(receipt));
    }

    /// <summary>
    /// 建立驗收單
    /// </summary>
    /// <param name="request">建立驗收單請求</param>
    /// <returns>建立的驗收單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Purchaser,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePurchaseReceipt([FromBody] CreatePurchaseReceiptRequest request)
    {
        var userId = GetCurrentUserId();
        var receiptId = await _purchaseReceiptService.CreatePurchaseReceiptAsync(request, userId);
        if (receiptId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立驗收單失敗，請確認採購單存在且已核准"));
        }

        return CreatedAtAction(
            nameof(GetPurchaseReceipt),
            new { id = receiptId },
            ApiResponse<int>.SuccessResponse(receiptId.Value, "驗收單建立成功"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
