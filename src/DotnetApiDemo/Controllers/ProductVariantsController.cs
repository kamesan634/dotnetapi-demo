using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.ProductVariants;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductVariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(IProductVariantService variantService)
    {
        _variantService = variantService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductVariantListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductVariantListDto>>>> GetVariants(
        [FromQuery] PaginationRequest request)
    {
        var result = await _variantService.GetVariantsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ProductVariantListDto>>.SuccessResponse(result));
    }

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductVariantListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductVariantListDto>>>> GetVariantsByProduct(int productId)
    {
        var variants = await _variantService.GetVariantsByProductAsync(productId);
        return Ok(ApiResponse<IEnumerable<ProductVariantListDto>>.SuccessResponse(variants));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductVariantDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductVariantDetailDto>>> GetVariant(int id)
    {
        var variant = await _variantService.GetVariantByIdAsync(id);
        if (variant == null) return NotFound(ApiResponse.FailResponse("找不到規格"));
        return Ok(ApiResponse<ProductVariantDetailDto>.SuccessResponse(variant));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateVariant([FromBody] CreateProductVariantRequest request)
    {
        var variantId = await _variantService.CreateVariantAsync(request);
        if (variantId == null) return BadRequest(ApiResponse.FailResponse("建立規格失敗"));
        return CreatedAtAction(nameof(GetVariant), new { id = variantId }, ApiResponse<int>.SuccessResponse(variantId.Value, "規格建立成功"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateVariant(int id, [FromBody] UpdateProductVariantRequest request)
    {
        var success = await _variantService.UpdateVariantAsync(id, request);
        if (!success) return BadRequest(ApiResponse.FailResponse("更新規格失敗"));
        return Ok(ApiResponse.SuccessResponse("規格更新成功"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteVariant(int id)
    {
        var success = await _variantService.DeleteVariantAsync(id);
        if (!success) return BadRequest(ApiResponse.FailResponse("刪除規格失敗"));
        return Ok(ApiResponse.SuccessResponse("規格刪除成功"));
    }
}
