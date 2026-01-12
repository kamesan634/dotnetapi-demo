using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Stores;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 門市控制器
/// </summary>
/// <remarks>
/// 處理門市與倉庫 CRUD 操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<StoresController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public StoresController(
        IStoreService storeService,
        IWarehouseService warehouseService,
        ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// 取得門市列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁門市列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<StoreListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StoreListDto>>>> GetStores(
        [FromQuery] PaginationRequest request)
    {
        var result = await _storeService.GetStoresAsync(request);
        return Ok(ApiResponse<PaginatedResponse<StoreListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得門市詳細資訊
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <returns>門市詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到門市</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StoreDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StoreDetailDto>>> GetStore(int id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到門市"));
        }

        return Ok(ApiResponse<StoreDetailDto>.SuccessResponse(store));
    }

    /// <summary>
    /// 建立門市
    /// </summary>
    /// <param name="request">建立門市請求</param>
    /// <returns>建立的門市 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateStore([FromBody] CreateStoreRequest request)
    {
        var storeId = await _storeService.CreateStoreAsync(request);
        if (storeId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立門市失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetStore),
            new { id = storeId },
            ApiResponse<int>.SuccessResponse(storeId.Value, "門市建立成功"));
    }

    /// <summary>
    /// 更新門市
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <param name="request">更新門市請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到門市</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateStore(int id, [FromBody] UpdateStoreRequest request)
    {
        var success = await _storeService.UpdateStoreAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到門市"));
        }

        return Ok(ApiResponse.SuccessResponse("門市更新成功"));
    }

    /// <summary>
    /// 刪除門市
    /// </summary>
    /// <param name="id">門市 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteStore(int id)
    {
        var success = await _storeService.DeleteStoreAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除門市失敗，門市可能不存在或有關聯倉庫"));
        }

        return Ok(ApiResponse.SuccessResponse("門市刪除成功"));
    }

    /// <summary>
    /// 取得倉庫列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="storeId">門市 ID (可選)</param>
    /// <returns>分頁倉庫列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<WarehouseListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<WarehouseListDto>>>> GetWarehouses(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? storeId = null)
    {
        var result = await _warehouseService.GetWarehousesAsync(request, storeId);
        return Ok(ApiResponse<PaginatedResponse<WarehouseListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得倉庫詳細資訊
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <returns>倉庫詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到倉庫</response>
    [HttpGet("warehouses/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WarehouseDetailDto>>> GetWarehouse(int id)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
        if (warehouse == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到倉庫"));
        }

        return Ok(ApiResponse<WarehouseDetailDto>.SuccessResponse(warehouse));
    }

    /// <summary>
    /// 建立倉庫
    /// </summary>
    /// <param name="request">建立倉庫請求</param>
    /// <returns>建立的倉庫 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost("warehouses")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateWarehouse([FromBody] CreateWarehouseRequest request)
    {
        var warehouseId = await _warehouseService.CreateWarehouseAsync(request);
        if (warehouseId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立倉庫失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetWarehouse),
            new { id = warehouseId },
            ApiResponse<int>.SuccessResponse(warehouseId.Value, "倉庫建立成功"));
    }

    /// <summary>
    /// 更新倉庫
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <param name="request">更新倉庫請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到倉庫</response>
    [HttpPut("warehouses/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateWarehouse(int id, [FromBody] UpdateWarehouseRequest request)
    {
        var success = await _warehouseService.UpdateWarehouseAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到倉庫"));
        }

        return Ok(ApiResponse.SuccessResponse("倉庫更新成功"));
    }

    /// <summary>
    /// 刪除倉庫
    /// </summary>
    /// <param name="id">倉庫 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("warehouses/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteWarehouse(int id)
    {
        var success = await _warehouseService.DeleteWarehouseAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除倉庫失敗，倉庫可能不存在或尚有庫存"));
        }

        return Ok(ApiResponse.SuccessResponse("倉庫刪除成功"));
    }
}
