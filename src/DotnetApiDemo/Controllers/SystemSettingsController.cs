using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SystemSettings;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 系統設定控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(ISystemSettingService systemSettingService, ILogger<SystemSettingsController> logger)
    {
        _systemSettingService = systemSettingService;
        _logger = logger;
    }

    /// <summary>
    /// 取得系統設定列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SystemSettingListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SystemSettingListDto>>>> GetSettings(
        [FromQuery] PaginationRequest request)
    {
        var result = await _systemSettingService.GetSettingsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<SystemSettingListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// 取得所有設定分類
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetCategories()
    {
        var categories = await _systemSettingService.GetCategoriesAsync();
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(categories));
    }

    /// <summary>
    /// 依分類取得設定
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemSettingListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SystemSettingListDto>>>> GetSettingsByCategory(string category)
    {
        var settings = await _systemSettingService.GetSettingsByCategoryAsync(category);
        return Ok(ApiResponse<IEnumerable<SystemSettingListDto>>.SuccessResponse(settings));
    }

    /// <summary>
    /// 取得設定詳細資訊
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SystemSettingDetailDto>>> GetSetting(int id)
    {
        var setting = await _systemSettingService.GetSettingByIdAsync(id);
        if (setting == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到設定"));
        }

        return Ok(ApiResponse<SystemSettingDetailDto>.SuccessResponse(setting));
    }

    /// <summary>
    /// 依鍵值取得設定
    /// </summary>
    [HttpGet("{category}/{key}")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SystemSettingDetailDto>>> GetSettingByKey(string category, string key)
    {
        var setting = await _systemSettingService.GetSettingByKeyAsync(category, key);
        if (setting == null)
        {
            return NotFound(ApiResponse.FailResponse("找不到設定"));
        }

        return Ok(ApiResponse<SystemSettingDetailDto>.SuccessResponse(setting));
    }

    /// <summary>
    /// 建立系統設定
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateSetting([FromBody] CreateSystemSettingRequest request)
    {
        var settingId = await _systemSettingService.CreateSettingAsync(request);
        if (settingId == null)
        {
            return BadRequest(ApiResponse.FailResponse("建立設定失敗，設定可能已存在"));
        }

        return CreatedAtAction(
            nameof(GetSetting),
            new { id = settingId },
            ApiResponse<int>.SuccessResponse(settingId.Value, "設定建立成功"));
    }

    /// <summary>
    /// 更新系統設定
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateSetting(int id, [FromBody] UpdateSystemSettingRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _systemSettingService.UpdateSettingAsync(id, request, userId);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("更新設定失敗"));
        }

        return Ok(ApiResponse.SuccessResponse("設定更新成功"));
    }

    /// <summary>
    /// 刪除系統設定
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteSetting(int id)
    {
        var success = await _systemSettingService.DeleteSettingAsync(id);
        if (!success)
        {
            return BadRequest(ApiResponse.FailResponse("刪除設定失敗，設定可能不存在或為系統設定"));
        }

        return Ok(ApiResponse.SuccessResponse("設定刪除成功"));
    }
}
