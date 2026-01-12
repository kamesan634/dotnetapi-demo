using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 供應商控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// 取得供應商列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁供應商列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SupplierListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SupplierListDto>>>> GetSuppliers(
        [FromQuery] PaginationRequest request)
    {
        var result = await _supplierService.GetSuppliersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<SupplierListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得供應商詳細資訊
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <returns>供應商詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到供應商</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> GetSupplier(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到供應商"));
        }

        return Ok(ApiResponse<SupplierDetailDto>.SuccessResponse(supplier));
    }

    /// <summary>
    /// 建立供應商
    /// </summary>
    /// <param name="request">建立供應商請求</param>
    /// <returns>建立的供應商 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplierId = await _supplierService.CreateSupplierAsync(request);
        if (supplierId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立供應商失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplierId },
            ApiResponse<int>.SuccessResponse(supplierId.Value, "供應商建立成功"));
    }

    /// <summary>
    /// 更新供應商
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <param name="request">更新供應商請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到供應商</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        var success = await _supplierService.UpdateSupplierAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到供應商"));
        }

        return Ok(ApiResponse.SuccessResponse("供應商更新成功"));
    }

    /// <summary>
    /// 刪除供應商
    /// </summary>
    /// <param name="id">供應商 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteSupplier(int id)
    {
        var success = await _supplierService.DeleteSupplierAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除供應商失敗，供應商可能不存在或有關聯採購單"));
        }

        return Ok(ApiResponse.SuccessResponse("供應商刪除成功"));
    }
}
