using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Users;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 使用者控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 取得使用者列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁使用者列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserListDto>>>> GetUsers(
        [FromQuery] PaginationRequest request)
    {
        var result = await _userService.GetUsersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<UserListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得使用者詳細資訊
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>使用者詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到使用者</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到使用者"));
        }

        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(user));
    }

    /// <summary>
    /// 建立使用者
    /// </summary>
    /// <param name="request">建立使用者請求</param>
    /// <returns>建立的使用者 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateUser([FromBody] CreateUserRequest request)
    {
        var (success, userId, errors) = await _userService.CreateUserAsync(request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("建立使用者失敗", errors));
        }

        return CreatedAtAction(
            nameof(GetUser),
            new { id = userId },
            ApiResponse<int>.SuccessResponse(userId!.Value, "使用者建立成功"));
    }

    /// <summary>
    /// 更新使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="request">更新使用者請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">更新失敗</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var (success, errors) = await _userService.UpdateUserAsync(id, request);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新使用者失敗", errors));
        }

        return Ok(ApiResponse.SuccessResponse("使用者更新成功"));
    }

    /// <summary>
    /// 刪除使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除使用者失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("使用者刪除成功"));
    }
}
