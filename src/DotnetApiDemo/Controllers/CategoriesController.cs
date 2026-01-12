using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 商品分類控制器
/// </summary>
/// <remarks>
/// 處理商品分類 CRUD 操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// 取得分類列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁分類列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CategoryListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CategoryListDto>>>> GetCategories(
        [FromQuery] PaginationRequest request)
    {
        var result = await _categoryService.GetCategoriesAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CategoryListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得分類樹狀結構
    /// </summary>
    /// <returns>分類樹</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryTreeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryTreeDto>>>> GetCategoryTree()
    {
        var result = await _categoryService.GetCategoryTreeAsync();
        return Ok(ApiResponse<IEnumerable<CategoryTreeDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得分類詳細資訊
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <returns>分類詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到分類</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryListDto>>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到分類"));
        }

        return Ok(ApiResponse<CategoryListDto>.SuccessResponse(category));
    }

    /// <summary>
    /// 建立分類
    /// </summary>
    /// <param name="request">建立分類請求</param>
    /// <returns>建立的分類 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var categoryId = await _categoryService.CreateCategoryAsync(request);
        if (categoryId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立分類失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = categoryId },
            ApiResponse<int>.SuccessResponse(categoryId.Value, "分類建立成功"));
    }

    /// <summary>
    /// 更新分類
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <param name="request">更新分類請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到分類</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var success = await _categoryService.UpdateCategoryAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到分類"));
        }

        return Ok(ApiResponse.SuccessResponse("分類更新成功"));
    }

    /// <summary>
    /// 刪除分類
    /// </summary>
    /// <param name="id">分類 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteCategory(int id)
    {
        var success = await _categoryService.DeleteCategoryAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除分類失敗，分類可能不存在或包含子分類/商品"));
        }

        return Ok(ApiResponse.SuccessResponse("分類刪除成功"));
    }
}
