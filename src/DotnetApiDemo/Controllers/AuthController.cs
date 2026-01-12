using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 驗證控制器
/// </summary>
/// <remarks>
/// 處理使用者登入、註冊、Token 刷新等驗證相關操作
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 使用者登入
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <returns>Token 回應</returns>
    /// <response code="200">登入成功</response>
    /// <response code="401">登入失敗</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse.FailResponse("使用者名稱或密碼錯誤"));
        }

        return Ok(ApiResponse<TokenResponse>.SuccessResponse(result, "登入成功"));
    }

    /// <summary>
    /// 使用者註冊
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <returns>註冊結果</returns>
    /// <response code="200">註冊成功</response>
    /// <response code="400">註冊失敗</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
    {
        var (success, errors) = await _authService.RegisterAsync(request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("註冊失敗", errors));
        }

        return Ok(ApiResponse.SuccessResponse("註冊成功"));
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    /// <param name="request">刷新 Token 請求</param>
    /// <returns>新的 Token 回應</returns>
    /// <response code="200">刷新成功</response>
    /// <response code="401">刷新失敗</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse.FailResponse("Token 無效或已過期"));
        }

        return Ok(ApiResponse<TokenResponse>.SuccessResponse(result, "Token 刷新成功"));
    }

    /// <summary>
    /// 使用者登出
    /// </summary>
    /// <returns>登出結果</returns>
    /// <response code="200">登出成功</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userId = GetCurrentUserId();
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse.SuccessResponse("登出成功"));
    }

    /// <summary>
    /// 修改密碼
    /// </summary>
    /// <param name="request">修改密碼請求</param>
    /// <returns>修改結果</returns>
    /// <response code="200">修改成功</response>
    /// <response code="400">修改失敗</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var (success, errors) = await _authService.ChangePasswordAsync(userId, request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("修改密碼失敗", errors));
        }

        return Ok(ApiResponse.SuccessResponse("密碼修改成功"));
    }

    /// <summary>
    /// 取得目前使用者資訊
    /// </summary>
    /// <returns>使用者資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="401">未授權</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到使用者"));
        }

        return Ok(ApiResponse<UserInfo>.SuccessResponse(user));
    }

    /// <summary>
    /// 取得目前使用者 ID
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
