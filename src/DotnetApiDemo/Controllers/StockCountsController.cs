using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 庫存盤點控制器
/// </summary>
/// <remarks>
/// 處理庫存盤點單的建立、查詢、執行、完成等操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class StockCountsController : ControllerBase
{
    private readonly IStockCountService _stockCountService;
    private readonly ILogger<StockCountsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="stockCountService">盤點服務</param>
    /// <param name="logger">日誌服務</param>
    public StockCountsController(
        IStockCountService stockCountService,
        ILogger<StockCountsController> logger)
    {
        _stockCountService = stockCountService;
        _logger = logger;
    }

    /// <summary>
    /// 取得盤點單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="status">狀態篩選 (可選)</param>
    /// <returns>分頁盤點單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<StockCountListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StockCountListDto>>>> GetStockCounts(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? warehouseId = null,
        [FromQuery] StockCountStatus? status = null)
    {
        var result = await _stockCountService.GetStockCountsAsync(request, warehouseId, status);
        return Ok(ApiResponse<PaginatedResponse<StockCountListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得盤點單詳細資訊
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <returns>盤點單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到盤點單</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockCountDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockCountDetailDto>>> GetStockCount(int id)
    {
        var stockCount = await _stockCountService.GetStockCountByIdAsync(id);
        if (stockCount == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到盤點單"));
        }

        return Ok(ApiResponse<StockCountDetailDto>.SuccessResponse(stockCount));
    }

    /// <summary>
    /// 建立盤點單
    /// </summary>
    /// <param name="request">建立盤點單請求</param>
    /// <returns>建立的盤點單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateStockCount([FromBody] CreateStockCountRequest request)
    {
        var userId = GetCurrentUserId();
        var stockCountId = await _stockCountService.CreateStockCountAsync(request, userId);
        if (stockCountId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立盤點單失敗"));
        }

        return CreatedAtAction(
            nameof(GetStockCount),
            new { id = stockCountId },
            ApiResponse<int>.SuccessResponse(stockCountId.Value, "盤點單建立成功"));
    }

    /// <summary>
    /// 更新盤點明細
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <param name="itemId">明細 ID</param>
    /// <param name="request">更新請求</param>
    /// <returns>操作結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("{id:int}/items/{itemId:int}")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateStockCountItem(
        int id,
        int itemId,
        [FromBody] UpdateStockCountItemRequest request)
    {
        var userId = GetCurrentUserId();
        var success = await _stockCountService.UpdateStockCountItemAsync(id, itemId, request, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新盤點明細失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("盤點明細更新成功"));
    }

    /// <summary>
    /// 開始盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">開始成功</response>
    /// <response code="400">開始失敗</response>
    [HttpPost("{id:int}/start")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> StartStockCount(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockCountService.StartStockCountAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("開始盤點失敗，請確認盤點單狀態及明細"));
        }

        return Ok(ApiResponse.SuccessResponse("盤點已開始"));
    }

    /// <summary>
    /// 完成盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">完成成功</response>
    /// <response code="400">完成失敗</response>
    /// <remarks>
    /// 完成盤點時會根據差異自動產生庫存調整
    /// </remarks>
    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CompleteStockCount(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockCountService.CompleteStockCountAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("完成盤點失敗，請確認所有項目皆已盤點"));
        }

        return Ok(ApiResponse.SuccessResponse("盤點已完成，庫存調整已產生"));
    }

    /// <summary>
    /// 取消盤點
    /// </summary>
    /// <param name="id">盤點單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelStockCount(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockCountService.CancelStockCountAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消盤點失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("盤點已取消"));
    }

    /// <summary>
    /// 取得目前使用者 ID
    /// </summary>
    /// <returns>使用者 ID</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
