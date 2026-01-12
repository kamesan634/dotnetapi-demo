using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Coupons;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 優惠券控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;
    private readonly ILogger<CouponsController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CouponsController(ICouponService couponService, ILogger<CouponsController> logger)
    {
        _couponService = couponService;
        _logger = logger;
    }

    /// <summary>
    /// 取得優惠券列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁優惠券列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CouponListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CouponListDto>>>> GetCoupons(
        [FromQuery] PaginationRequest request)
    {
        var result = await _couponService.GetCouponsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CouponListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得優惠券詳細資訊
    /// </summary>
    /// <param name="id">優惠券 ID</param>
    /// <returns>優惠券詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到優惠券</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CouponDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CouponDetailDto>>> GetCoupon(int id)
    {
        var coupon = await _couponService.GetCouponByIdAsync(id);
        if (coupon == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到優惠券"));
        }

        return Ok(ApiResponse<CouponDetailDto>.SuccessResponse(coupon));
    }

    /// <summary>
    /// 依優惠券代碼取得優惠券資訊
    /// </summary>
    /// <param name="code">優惠券代碼</param>
    /// <returns>優惠券詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到優惠券</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<CouponDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CouponDetailDto>>> GetCouponByCode(string code)
    {
        var coupon = await _couponService.GetCouponByCodeAsync(code);
        if (coupon == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到優惠券"));
        }

        return Ok(ApiResponse<CouponDetailDto>.SuccessResponse(coupon));
    }

    /// <summary>
    /// 建立優惠券
    /// </summary>
    /// <param name="request">建立優惠券請求</param>
    /// <returns>建立的優惠券 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var couponId = await _couponService.CreateCouponAsync(request);
        if (couponId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立優惠券失敗，代碼可能已存在或促銷活動/客戶不存在"));
        }

        return CreatedAtAction(
            nameof(GetCoupon),
            new { id = couponId },
            ApiResponse<int>.SuccessResponse(couponId.Value, "優惠券建立成功"));
    }

    /// <summary>
    /// 更新優惠券
    /// </summary>
    /// <param name="id">優惠券 ID</param>
    /// <param name="request">更新優惠券請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateCoupon(int id, [FromBody] UpdateCouponRequest request)
    {
        var success = await _couponService.UpdateCouponAsync(id, request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新優惠券失敗，優惠券可能不存在、已使用或客戶不存在"));
        }

        return Ok(ApiResponse.SuccessResponse("優惠券更新成功"));
    }

    /// <summary>
    /// 刪除優惠券
    /// </summary>
    /// <param name="id">優惠券 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteCoupon(int id)
    {
        var success = await _couponService.DeleteCouponAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除優惠券失敗，優惠券可能不存在或已使用"));
        }

        return Ok(ApiResponse.SuccessResponse("優惠券刪除成功"));
    }
}
