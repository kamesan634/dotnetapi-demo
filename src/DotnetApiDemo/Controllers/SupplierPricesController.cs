using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 供應商報價控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SupplierPricesController : ControllerBase
{
    private readonly ISupplierPriceService _supplierPriceService;
    private readonly ILogger<SupplierPricesController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SupplierPricesController(ISupplierPriceService supplierPriceService, ILogger<SupplierPricesController> logger)
    {
        _supplierPriceService = supplierPriceService;
        _logger = logger;
    }

    /// <summary>
    /// 取得供應商報價列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="supplierId">供應商 ID (可選篩選)</param>
    /// <param name="productId">商品 ID (可選篩選)</param>
    /// <returns>分頁供應商報價列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SupplierPriceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SupplierPriceListDto>>>> GetSupplierPrices(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? supplierId = null,
        [FromQuery] int? productId = null)
    {
        var result = await _supplierPriceService.GetSupplierPricesAsync(request, supplierId, productId);
        return Ok(ApiResponse<PaginatedResponse<SupplierPriceListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得供應商報價詳細資訊
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <returns>供應商報價詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到報價</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPriceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierPriceDetailDto>>> GetSupplierPrice(int id)
    {
        var supplierPrice = await _supplierPriceService.GetSupplierPriceByIdAsync(id);
        if (supplierPrice == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到供應商報價"));
        }

        return Ok(ApiResponse<SupplierPriceDetailDto>.SuccessResponse(supplierPrice));
    }

    /// <summary>
    /// 依商品查詢供應商報價
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns>供應商報價列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierPriceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SupplierPriceListDto>>>> GetPricesByProduct(int productId)
    {
        var result = await _supplierPriceService.GetPricesByProductAsync(productId);
        return Ok(ApiResponse<IEnumerable<SupplierPriceListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 依供應商查詢商品報價
    /// </summary>
    /// <param name="supplierId">供應商 ID</param>
    /// <returns>供應商報價列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("supplier/{supplierId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierPriceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SupplierPriceListDto>>>> GetPricesBySupplier(int supplierId)
    {
        var result = await _supplierPriceService.GetPricesBySupplierAsync(supplierId);
        return Ok(ApiResponse<IEnumerable<SupplierPriceListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 建立供應商報價
    /// </summary>
    /// <param name="request">建立供應商報價請求</param>
    /// <returns>建立的報價 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateSupplierPrice([FromBody] CreateSupplierPriceRequest request)
    {
        var priceId = await _supplierPriceService.CreateSupplierPriceAsync(request);
        if (priceId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立供應商報價失敗，供應商或商品可能不存在，或相同生效日期的報價已存在"));
        }

        return CreatedAtAction(
            nameof(GetSupplierPrice),
            new { id = priceId },
            ApiResponse<int>.SuccessResponse(priceId.Value, "供應商報價建立成功"));
    }

    /// <summary>
    /// 更新供應商報價
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <param name="request">更新供應商報價請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到報價</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Purchaser")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateSupplierPrice(int id, [FromBody] UpdateSupplierPriceRequest request)
    {
        var success = await _supplierPriceService.UpdateSupplierPriceAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到供應商報價"));
        }

        return Ok(ApiResponse.SuccessResponse("供應商報價更新成功"));
    }

    /// <summary>
    /// 刪除供應商報價
    /// </summary>
    /// <param name="id">報價 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="404">找不到報價</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteSupplierPrice(int id)
    {
        var success = await _supplierPriceService.DeleteSupplierPriceAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到供應商報價"));
        }

        return Ok(ApiResponse.SuccessResponse("供應商報價刪除成功"));
    }
}
