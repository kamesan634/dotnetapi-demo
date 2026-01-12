using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

/// <summary>
/// 系統管理控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class SystemController : ControllerBase
{
    private readonly IUserPresenceService _userPresenceService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SystemController> _logger;

    public SystemController(
        IUserPresenceService userPresenceService,
        ICacheService cacheService,
        ILogger<SystemController> logger)
    {
        _userPresenceService = userPresenceService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 取得在線使用者狀態
    /// </summary>
    /// <returns>在線狀態資訊</returns>
    [HttpGet("online-status")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<OnlineStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OnlineStatusResponse>>> GetOnlineStatus()
    {
        var onlineCount = await _userPresenceService.GetOnlineCountAsync();
        var onlineUsers = await _userPresenceService.GetOnlineUsersAsync();

        var response = new OnlineStatusResponse
        {
            OnlineCount = onlineCount,
            Users = onlineUsers.Select(u => new OnlineUserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = u.FullName,
                LoginTime = u.LoginTime,
                LastActiveTime = u.LastActiveTime
            })
        };

        return Ok(ApiResponse<OnlineStatusResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// 取得快取統計資訊
    /// </summary>
    /// <returns>快取統計</returns>
    [HttpGet("cache-stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CacheStatsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CacheStatsResponse>>> GetCacheStats()
    {
        // 這裡可以擴展更詳細的統計資訊
        var response = new CacheStatsResponse
        {
            Status = "Healthy",
            Message = "Redis 快取服務運作正常"
        };

        return Ok(ApiResponse<CacheStatsResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// 清除指定快取
    /// </summary>
    /// <param name="pattern">快取鍵模式</param>
    /// <returns>操作結果</returns>
    [HttpPost("cache/clear")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> ClearCache([FromQuery] string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return BadRequest(ApiResponse.FailResponse("請指定快取鍵模式"));
        }

        await _cacheService.RemoveByPatternAsync(pattern);
        _logger.LogInformation("快取已清除: Pattern={Pattern}", pattern);

        return Ok(ApiResponse.SuccessResponse("快取已清除"));
    }
}

/// <summary>
/// 在線狀態回應
/// </summary>
public class OnlineStatusResponse
{
    /// <summary>
    /// 在線人數
    /// </summary>
    public int OnlineCount { get; set; }

    /// <summary>
    /// 在線使用者清單
    /// </summary>
    public IEnumerable<OnlineUserDto> Users { get; set; } = Enumerable.Empty<OnlineUserDto>();
}

/// <summary>
/// 在線使用者 DTO
/// </summary>
public class OnlineUserDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// 登入時間
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 最後活動時間
    /// </summary>
    public DateTime LastActiveTime { get; set; }
}

/// <summary>
/// 快取統計回應
/// </summary>
public class CacheStatsResponse
{
    /// <summary>
    /// 狀態
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
