using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 商品控制器
/// </summary>
/// <remarks>
/// 處理商品 CRUD 操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// 取得商品列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="categoryId">分類 ID (可選)</param>
    /// <returns>分頁商品列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductListDto>>>> GetProducts(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? categoryId = null)
    {
        var result = await _productService.GetProductsAsync(request, categoryId);
        return Ok(ApiResponse<PaginatedResponse<ProductListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得商品詳細資訊
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>商品詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到商品</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到商品"));
        }

        return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(product));
    }

    /// <summary>
    /// 根據 SKU 取得商品
    /// </summary>
    /// <param name="sku">商品編號</param>
    /// <returns>商品詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到商品</response>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProductBySku(string sku)
    {
        var product = await _productService.GetProductBySkuAsync(sku);
        if (product == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到商品"));
        }

        return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(product));
    }

    /// <summary>
    /// 根據條碼取得商品
    /// </summary>
    /// <param name="barcode">條碼</param>
    /// <returns>商品詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到商品</response>
    [HttpGet("barcode/{barcode}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProductByBarcode(string barcode)
    {
        var product = await _productService.GetProductByBarcodeAsync(barcode);
        if (product == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到商品"));
        }

        return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(product));
    }

    /// <summary>
    /// 建立商品
    /// </summary>
    /// <param name="request">建立商品請求</param>
    /// <returns>建立的商品 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var productId = await _productService.CreateProductAsync(request);
        if (productId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立商品失敗，SKU 可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = productId },
            ApiResponse<int>.SuccessResponse(productId.Value, "商品建立成功"));
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <param name="request">更新商品請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到商品</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var success = await _productService.UpdateProductAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到商品"));
        }

        return Ok(ApiResponse.SuccessResponse("商品更新成功"));
    }

    /// <summary>
    /// 刪除商品
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="404">找不到商品</response>
    /// <response code="400">刪除失敗 (商品有庫存)</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteProduct(int id)
    {
        var success = await _productService.DeleteProductAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除商品失敗，商品可能不存在或尚有庫存"));
        }

        return Ok(ApiResponse.SuccessResponse("商品刪除成功"));
    }

    /// <summary>
    /// 取得所有計量單位
    /// </summary>
    /// <returns>計量單位列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("units")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UnitDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetUnits()
    {
        var units = await _productService.GetUnitsAsync();
        return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResponse(units));
    }

    /// <summary>
    /// 建立計量單位
    /// </summary>
    /// <param name="request">建立計量單位請求</param>
    /// <returns>建立的單位 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost("units")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateUnit([FromBody] CreateUnitRequest request)
    {
        var unitId = await _productService.CreateUnitAsync(request);
        if (unitId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立計量單位失敗，代碼可能已存在"));
        }

        return Created("", ApiResponse<int>.SuccessResponse(unitId.Value, "計量單位建立成功"));
    }

    /// <summary>
    /// 匯入商品
    /// </summary>
    /// <param name="request">匯入請求</param>
    /// <returns>匯入結果</returns>
    /// <response code="200">匯入完成</response>
    [HttpPost("import")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ProductImportResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductImportResultDto>>> ImportProducts([FromBody] ProductImportRequest request)
    {
        var result = await _productService.ImportProductsAsync(request);
        return Ok(ApiResponse<ProductImportResultDto>.SuccessResponse(result, "商品匯入完成"));
    }

    /// <summary>
    /// 匯出商品為 CSV
    /// </summary>
    /// <param name="categoryId">分類 ID (可選)</param>
    /// <returns>CSV 檔案</returns>
    /// <response code="200">匯出成功</response>
    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProducts([FromQuery] int? categoryId = null)
    {
        var csvBytes = await _productService.ExportProductsToCsvAsync(categoryId);
        var fileName = $"商品列表_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 取得匯入範本 (CSV)
    /// </summary>
    /// <returns>CSV 範本檔案</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("import/template")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportTemplate()
    {
        var csvBytes = await _productService.GetImportTemplateAsync();
        return File(csvBytes, "text/csv; charset=utf-8", "商品匯入範本.csv");
    }

    /// <summary>
    /// 從 Excel 檔案匯入商品
    /// </summary>
    /// <param name="file">Excel 檔案 (.xlsx)</param>
    /// <param name="updateExisting">是否更新現有商品</param>
    /// <returns>匯入結果</returns>
    /// <response code="200">匯入完成</response>
    /// <response code="400">檔案格式錯誤</response>
    [HttpPost("import/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ProductImportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductImportResultDto>>> ImportProductsFromExcel(
        IFormFile file,
        [FromQuery] bool updateExisting = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse.FailResponse("請上傳檔案"));
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(ApiResponse.FailResponse("只支援 Excel 檔案格式 (.xlsx, .xls)"));
        }

        using var stream = file.OpenReadStream();
        var result = await _productService.ImportProductsFromExcelAsync(stream, updateExisting);
        return Ok(ApiResponse<ProductImportResultDto>.SuccessResponse(result, "商品匯入完成"));
    }

    /// <summary>
    /// 匯出商品為 Excel
    /// </summary>
    /// <param name="categoryId">分類 ID (可選)</param>
    /// <returns>Excel 檔案</returns>
    /// <response code="200">匯出成功</response>
    [HttpGet("export/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProductsToExcel([FromQuery] int? categoryId = null)
    {
        var excelBytes = await _productService.ExportProductsToExcelAsync(categoryId);
        var fileName = $"商品列表_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// 取得 Excel 匯入範本
    /// </summary>
    /// <returns>Excel 範本檔案</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("import/template/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportTemplateExcel()
    {
        var excelBytes = await _productService.GetImportTemplateExcelAsync();
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "商品匯入範本.xlsx");
    }
}
