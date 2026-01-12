using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 庫存控制器
/// </summary>
/// <remarks>
/// 處理庫存查詢、調整、調撥等操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IStockTransferService _stockTransferService;
    private readonly ILogger<InventoryController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public InventoryController(
        IInventoryService inventoryService,
        IStockTransferService stockTransferService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _stockTransferService = stockTransferService;
        _logger = logger;
    }

    /// <summary>
    /// 取得庫存列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="productId">商品 ID (可選)</param>
    /// <returns>分頁庫存列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InventoryListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<InventoryListDto>>>> GetInventories(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? warehouseId = null,
        [FromQuery] int? productId = null)
    {
        var result = await _inventoryService.GetInventoriesAsync(request, warehouseId, productId);
        return Ok(ApiResponse<PaginatedResponse<InventoryListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得商品在各倉庫的庫存
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns>庫存列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryListDto>>>> GetProductInventories(int productId)
    {
        var result = await _inventoryService.GetProductInventoriesAsync(productId);
        return Ok(ApiResponse<IEnumerable<InventoryListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得倉庫所有庫存
    /// </summary>
    /// <param name="warehouseId">倉庫 ID</param>
    /// <returns>庫存列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("warehouse/{warehouseId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryListDto>>>> GetWarehouseInventories(int warehouseId)
    {
        var result = await _inventoryService.GetWarehouseInventoriesAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<InventoryListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得低庫存警示
    /// </summary>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <returns>低庫存警示列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("alerts/low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LowStockAlertDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LowStockAlertDto>>>> GetLowStockAlerts(
        [FromQuery] int? warehouseId = null)
    {
        var result = await _inventoryService.GetLowStockAlertsAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<LowStockAlertDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得補貨建議
    /// </summary>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <returns>補貨建議列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("replenishment-suggestions")]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReplenishmentSuggestionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReplenishmentSuggestionDto>>>> GetReplenishmentSuggestions(
        [FromQuery] int? warehouseId = null)
    {
        var result = await _inventoryService.GetReplenishmentSuggestionsAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<ReplenishmentSuggestionDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得庫存異動記錄
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="warehouseId">倉庫 ID (可選)</param>
    /// <param name="productId">商品 ID (可選)</param>
    /// <returns>分頁庫存異動記錄</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InventoryMovementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<InventoryMovementDto>>>> GetInventoryMovements(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? warehouseId = null,
        [FromQuery] int? productId = null)
    {
        var result = await _inventoryService.GetInventoryMovementsAsync(request, warehouseId, productId);
        return Ok(ApiResponse<PaginatedResponse<InventoryMovementDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 建立庫存調整單
    /// </summary>
    /// <param name="request">庫存調整請求</param>
    /// <returns>調整單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateStockAdjustment([FromBody] StockAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var adjustmentId = await _inventoryService.CreateStockAdjustmentAsync(request, userId);
        if (adjustmentId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立庫存調整單失敗"));
        }

        return Created("", ApiResponse<int>.SuccessResponse(adjustmentId.Value, "庫存調整單建立成功"));
    }

    /// <summary>
    /// 取得調撥單列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁調撥單列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("transfers")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<StockTransferDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StockTransferDto>>>> GetStockTransfers(
        [FromQuery] PaginationRequest request)
    {
        var result = await _stockTransferService.GetStockTransfersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<StockTransferDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得調撥單詳細資訊
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>調撥單詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到調撥單</response>
    [HttpGet("transfers/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockTransferDetailDto>>> GetStockTransfer(int id)
    {
        var transfer = await _stockTransferService.GetStockTransferByIdAsync(id);
        if (transfer == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到調撥單"));
        }

        return Ok(ApiResponse<StockTransferDetailDto>.SuccessResponse(transfer));
    }

    /// <summary>
    /// 建立調撥單
    /// </summary>
    /// <param name="request">建立調撥單請求</param>
    /// <returns>建立的調撥單 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost("transfers")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateStockTransfer([FromBody] CreateStockTransferRequest request)
    {
        var userId = GetCurrentUserId();
        var transferId = await _stockTransferService.CreateStockTransferAsync(request, userId);
        if (transferId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立調撥單失敗"));
        }

        return CreatedAtAction(
            nameof(GetStockTransfer),
            new { id = transferId },
            ApiResponse<int>.SuccessResponse(transferId.Value, "調撥單建立成功"));
    }

    /// <summary>
    /// 核准調撥單
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">核准成功</response>
    /// <response code="400">核准失敗</response>
    [HttpPost("transfers/{id:int}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ApproveStockTransfer(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockTransferService.ApproveStockTransferAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("核准調撥單失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("調撥單已核准"));
    }

    /// <summary>
    /// 出庫調撥
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">出庫成功</response>
    /// <response code="400">出庫失敗</response>
    [HttpPost("transfers/{id:int}/ship")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ShipStockTransfer(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockTransferService.ShipStockTransferAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("出庫失敗，庫存可能不足"));
        }

        return Ok(ApiResponse.SuccessResponse("已完成出庫"));
    }

    /// <summary>
    /// 入庫調撥
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">入庫成功</response>
    /// <response code="400">入庫失敗</response>
    [HttpPost("transfers/{id:int}/receive")]
    [Authorize(Roles = "Admin,Manager,Warehouse")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ReceiveStockTransfer(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockTransferService.ReceiveStockTransferAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("入庫失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("已完成入庫"));
    }

    /// <summary>
    /// 取消調撥單
    /// </summary>
    /// <param name="id">調撥單 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">取消成功</response>
    /// <response code="400">取消失敗</response>
    [HttpPost("transfers/{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelStockTransfer(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _stockTransferService.CancelStockTransferAsync(id, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("取消調撥單失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("調撥單已取消"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
