using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.TaxTypes;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 稅別控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class TaxTypesController : ControllerBase
{
    private readonly ILogger<TaxTypesController> _logger;

    private static readonly List<TaxTypeDto> TaxTypes = new()
    {
        new TaxTypeDto
        {
            Id = (int)TaxType.Taxable,
            Code = nameof(TaxType.Taxable),
            Name = "應稅",
            Description = "含 5% 營業稅",
            TaxRate = 5m
        },
        new TaxTypeDto
        {
            Id = (int)TaxType.TaxFree,
            Code = nameof(TaxType.TaxFree),
            Name = "免稅",
            Description = "免徵營業稅",
            TaxRate = 0m
        },
        new TaxTypeDto
        {
            Id = (int)TaxType.ZeroRate,
            Code = nameof(TaxType.ZeroRate),
            Name = "零稅率",
            Description = "適用零稅率",
            TaxRate = 0m
        }
    };

    /// <summary>
    /// 建構函式
    /// </summary>
    public TaxTypesController(ILogger<TaxTypesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 取得稅別列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>稅別列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TaxTypeDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<PaginatedResponse<TaxTypeDto>>> GetTaxTypes(
        [FromQuery] PaginationRequest request)
    {
        var query = TaxTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search) ||
                                     t.Code.Contains(request.Search));
        }

        var totalCount = query.Count();

        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var result = new PaginatedResponse<TaxTypeDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Ok(ApiResponse<PaginatedResponse<TaxTypeDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得稅別詳細資訊
    /// </summary>
    /// <param name="id">稅別 ID</param>
    /// <returns>稅別詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到稅別</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<TaxTypeDto>> GetTaxType(int id)
    {
        var taxType = TaxTypes.FirstOrDefault(t => t.Id == id);
        if (taxType == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到稅別"));
        }

        return Ok(ApiResponse<TaxTypeDto>.SuccessResponse(taxType));
    }

    /// <summary>
    /// 建立稅別 (系統稅別為唯讀)
    /// </summary>
    /// <returns>不允許建立</returns>
    /// <response code="400">稅別為系統定義，不允許新增</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse> CreateTaxType()
    {
        return BadRequest(ApiResponse.FailResponse("稅別為系統定義，不允許新增"));
    }

    /// <summary>
    /// 更新稅別 (系統稅別為唯讀)
    /// </summary>
    /// <param name="id">稅別 ID</param>
    /// <returns>不允許更新</returns>
    /// <response code="400">稅別為系統定義，不允許修改</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse> UpdateTaxType(int id)
    {
        return BadRequest(ApiResponse.FailResponse("稅別為系統定義，不允許修改"));
    }

    /// <summary>
    /// 刪除稅別 (系統稅別為唯讀)
    /// </summary>
    /// <param name="id">稅別 ID</param>
    /// <returns>不允許刪除</returns>
    /// <response code="400">稅別為系統定義，不允許刪除</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse> DeleteTaxType(int id)
    {
        return BadRequest(ApiResponse.FailResponse("稅別為系統定義，不允許刪除"));
    }
}
