using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 客戶/會員控制器
/// </summary>
/// <remarks>
/// 處理客戶與會員等級 CRUD 操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerLevelService _customerLevelService;
    private readonly ILogger<CustomersController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CustomersController(
        ICustomerService customerService,
        ICustomerLevelService customerLevelService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _customerLevelService = customerLevelService;
        _logger = logger;
    }

    /// <summary>
    /// 取得客戶列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="levelId">會員等級 ID (可選)</param>
    /// <returns>分頁客戶列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CustomerListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CustomerListDto>>>> GetCustomers(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? levelId = null)
    {
        var result = await _customerService.GetCustomersAsync(request, levelId);
        return Ok(ApiResponse<PaginatedResponse<CustomerListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得客戶詳細資訊
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <returns>客戶詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到客戶</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetCustomer(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到客戶"));
        }

        return Ok(ApiResponse<CustomerDetailDto>.SuccessResponse(customer));
    }

    /// <summary>
    /// 根據會員編號取得客戶
    /// </summary>
    /// <param name="memberNo">會員編號</param>
    /// <returns>客戶詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到客戶</response>
    [HttpGet("member/{memberNo}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetCustomerByMemberNo(string memberNo)
    {
        var customer = await _customerService.GetCustomerByMemberNoAsync(memberNo);
        if (customer == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到客戶"));
        }

        return Ok(ApiResponse<CustomerDetailDto>.SuccessResponse(customer));
    }

    /// <summary>
    /// 根據電話取得客戶
    /// </summary>
    /// <param name="phone">電話</param>
    /// <returns>客戶詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到客戶</response>
    [HttpGet("phone/{phone}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetCustomerByPhone(string phone)
    {
        var customer = await _customerService.GetCustomerByPhoneAsync(phone);
        if (customer == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到客戶"));
        }

        return Ok(ApiResponse<CustomerDetailDto>.SuccessResponse(customer));
    }

    /// <summary>
    /// 建立客戶
    /// </summary>
    /// <param name="request">建立客戶請求</param>
    /// <returns>建立的客戶 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var customerId = await _customerService.CreateCustomerAsync(request);
        if (customerId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立客戶失敗"));
        }

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customerId },
            ApiResponse<int>.SuccessResponse(customerId.Value, "客戶建立成功"));
    }

    /// <summary>
    /// 更新客戶
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <param name="request">更新客戶請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到客戶</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        var success = await _customerService.UpdateCustomerAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到客戶"));
        }

        return Ok(ApiResponse.SuccessResponse("客戶更新成功"));
    }

    /// <summary>
    /// 刪除客戶
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="404">找不到客戶</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteCustomer(int id)
    {
        var success = await _customerService.DeleteCustomerAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到客戶"));
        }

        return Ok(ApiResponse.SuccessResponse("客戶刪除成功"));
    }

    /// <summary>
    /// 取得會員等級列表
    /// </summary>
    /// <returns>會員等級列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("levels")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerLevelListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerLevelListDto>>>> GetCustomerLevels()
    {
        var levels = await _customerLevelService.GetCustomerLevelsAsync();
        return Ok(ApiResponse<IEnumerable<CustomerLevelListDto>>.SuccessResponse(levels));
    }

    /// <summary>
    /// 建立會員等級
    /// </summary>
    /// <param name="request">建立會員等級請求</param>
    /// <returns>建立的等級 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost("levels")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCustomerLevel([FromBody] CreateCustomerLevelRequest request)
    {
        var levelId = await _customerLevelService.CreateCustomerLevelAsync(request);
        if (levelId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立會員等級失敗，代碼可能已存在"));
        }

        return Created("", ApiResponse<int>.SuccessResponse(levelId.Value, "會員等級建立成功"));
    }
}
