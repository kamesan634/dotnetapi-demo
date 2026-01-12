using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductCombosController : ControllerBase
{
    private readonly IProductComboService _comboService;
    private readonly ILogger<ProductCombosController> _logger;

    public ProductCombosController(IProductComboService comboService, ILogger<ProductCombosController> logger)
    {
        _comboService = comboService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductComboListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductComboListDto>>>> GetCombos(
        [FromQuery] PaginationRequest request,
        [FromQuery] bool? activeOnly = null)
    {
        var result = await _comboService.GetCombosAsync(request, activeOnly);
        return Ok(ApiResponse<PaginatedResponse<ProductComboListDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductComboDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductComboDetailDto>>> GetCombo(int id)
    {
        var combo = await _comboService.GetComboByIdAsync(id);
        if (combo == null)
            return NotFound(ApiResponse.FailResponse("找不到商品組合"));

        return Ok(ApiResponse<ProductComboDetailDto>.SuccessResponse(combo));
    }

    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ProductComboDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductComboDetailDto>>> GetComboByCode(string code)
    {
        var combo = await _comboService.GetComboByCodeAsync(code);
        if (combo == null)
            return NotFound(ApiResponse.FailResponse("找不到商品組合"));

        return Ok(ApiResponse<ProductComboDetailDto>.SuccessResponse(combo));
    }

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductComboListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductComboListDto>>>> GetCombosByProduct(int productId)
    {
        var combos = await _comboService.GetActiveCombosByProductAsync(productId);
        return Ok(ApiResponse<IEnumerable<ProductComboListDto>>.SuccessResponse(combos));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCombo([FromBody] CreateProductComboRequest request)
    {
        var comboId = await _comboService.CreateComboAsync(request);
        if (comboId == null)
            return BadRequest(ApiResponse.FailResponse("建立商品組合失敗，代碼可能已存在或商品不存在"));

        return CreatedAtAction(
            nameof(GetCombo),
            new { id = comboId },
            ApiResponse<int>.SuccessResponse(comboId.Value, "商品組合建立成功"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateCombo(int id, [FromBody] UpdateProductComboRequest request)
    {
        var success = await _comboService.UpdateComboAsync(id, request);
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到商品組合"));

        return Ok(ApiResponse.SuccessResponse("商品組合更新成功"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteCombo(int id)
    {
        var success = await _comboService.DeleteComboAsync(id);
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到商品組合"));

        return Ok(ApiResponse.SuccessResponse("商品組合刪除成功"));
    }

    [HttpPost("{id:int}/items")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> AddComboItem(int id, [FromBody] CreateProductComboItemRequest request)
    {
        var success = await _comboService.AddComboItemAsync(id, request);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("新增組合項目失敗"));

        return Ok(ApiResponse.SuccessResponse("組合項目新增成功"));
    }

    [HttpDelete("{id:int}/items/{productId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RemoveComboItem(int id, int productId)
    {
        var success = await _comboService.RemoveComboItemAsync(id, productId);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("移除組合項目失敗"));

        return Ok(ApiResponse.SuccessResponse("組合項目移除成功"));
    }
}
