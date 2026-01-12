using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Roles;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 角色控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// 取得角色列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁角色列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RoleListDto>>>> GetRoles(
        [FromQuery] PaginationRequest request)
    {
        var result = await _roleService.GetRolesAsync(request);
        return Ok(ApiResponse<PaginatedResponse<RoleListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得角色詳細資訊
    /// </summary>
    /// <param name="id">角色 ID</param>
    /// <returns>角色詳細資訊</returns>
    /// <response code="200">取得成功</response>
    /// <response code="404">找不到角色</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDetailDto>>> GetRole(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到角色"));
        }

        return Ok(ApiResponse<RoleDetailDto>.SuccessResponse(role));
    }

    /// <summary>
    /// 建立角色
    /// </summary>
    /// <param name="request">建立角色請求</param>
    /// <returns>建立的角色 ID</returns>
    /// <response code="201">建立成功</response>
    /// <response code="400">建立失敗</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var roleId = await _roleService.CreateRoleAsync(request);
        if (roleId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立角色失敗，名稱可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetRole),
            new { id = roleId },
            ApiResponse<int>.SuccessResponse(roleId.Value, "角色建立成功"));
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id">角色 ID</param>
    /// <param name="request">更新角色請求</param>
    /// <returns>更新結果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="404">找不到角色</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var success = await _roleService.UpdateRoleAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.FailResponse("找不到角色或無法更新系統角色"));
        }

        return Ok(ApiResponse.SuccessResponse("角色更新成功"));
    }

    /// <summary>
    /// 刪除角色
    /// </summary>
    /// <param name="id">角色 ID</param>
    /// <returns>刪除結果</returns>
    /// <response code="200">刪除成功</response>
    /// <response code="400">刪除失敗</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteRole(int id)
    {
        var success = await _roleService.DeleteRoleAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除角色失敗，角色可能不存在、是系統角色或有使用者使用此角色"));
        }

        return Ok(ApiResponse.SuccessResponse("角色刪除成功"));
    }
}
