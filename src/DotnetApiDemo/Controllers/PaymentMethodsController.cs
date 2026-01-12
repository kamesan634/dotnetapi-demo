using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.PaymentMethods;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 付款方式控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ILogger<PaymentMethodsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PaymentMethodsController(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodsController> logger)
    {
        _paymentMethodService = paymentMethodService;
        _logger = logger;
    }

    /// <summary>
    /// 取得付款方式列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁付款方式列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PaymentMethodListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PaymentMethodListDto>>>> GetPaymentMethods(
        [FromQuery] PaginationRequest request)
    {
        var result = await _paymentMethodService.GetPaymentMethodsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<PaymentMethodListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得所有啟用的付款方式
    /// </summary>
    /// <returns>啟用的付款方式列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentMethodListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodListDto>>>> GetActivePaymentMethods()
    {
        var result = await _paymentMethodService.GetActivePaymentMethodsAsync();
        return Ok(ApiResponse<IEnumerable<PaymentMethodListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得付款方式詳細資訊
    /// </summary>
    /// <param name="id">付款方式 ID</param>
    /// <returns>付款方式詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到付款方式</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDetailDto>>> GetPaymentMethod(int id)
    {
        var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id);
        if (paymentMethod == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到付款方式"));
        }

        return Ok(ApiResponse<PaymentMethodDetailDto>.SuccessResponse(paymentMethod));
    }

    /// <summary>
    /// 依代碼取得付款方式
    /// </summary>
    /// <param name="code">付款方式代碼</param>
    /// <returns>付款方式詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到付款方式</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentMethodDetailDto>>> GetPaymentMethodByCode(string code)
    {
        var paymentMethod = await _paymentMethodService.GetPaymentMethodByCodeAsync(code);
        if (paymentMethod == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到付款方式"));
        }

        return Ok(ApiResponse<PaymentMethodDetailDto>.SuccessResponse(paymentMethod));
    }

    /// <summary>
    /// 建立付款方式
    /// </summary>
    /// <param name="request">建立付款方式請求</param>
    /// <returns>建立的付款方式 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        var paymentMethodId = await _paymentMethodService.CreatePaymentMethodAsync(request);
        if (paymentMethodId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立付款方式失敗，代碼可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetPaymentMethod),
            new { id = paymentMethodId },
            ApiResponse<int>.SuccessResponse(paymentMethodId.Value, "付款方式建立成功"));
    }

    /// <summary>
    /// 更新付款方式
    /// </summary>
    /// <param name="id">付款方式 ID</param>
    /// <param name="request">更新付款方式請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodRequest request)
    {
        var success = await _paymentMethodService.UpdatePaymentMethodAsync(id, request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新付款方式失敗，付款方式可能不存在"));
        }

        return Ok(ApiResponse.SuccessResponse("付款方式更新成功"));
    }

    /// <summary>
    /// 刪除付款方式
    /// </summary>
    /// <param name="id">付款方式 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeletePaymentMethod(int id)
    {
        var success = await _paymentMethodService.DeletePaymentMethodAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除付款方式失敗，付款方式可能不存在或有關聯的付款記錄"));
        }

        return Ok(ApiResponse.SuccessResponse("付款方式刪除成功"));
    }
}
