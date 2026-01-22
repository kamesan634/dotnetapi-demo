using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;
using StackExchange.Redis;

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
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SystemController> _logger;

    public SystemController(
        IUserPresenceService userPresenceService,
        ICacheService cacheService,
        IConnectionMultiplexer redis,
        ILogger<SystemController> logger)
    {
        _userPresenceService = userPresenceService;
        _cacheService = cacheService;
        _redis = redis;
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
        try
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            // 取得 Redis 伺服器資訊
            var info = await server.InfoAsync();

            // 解析統計資訊
            long totalKeys = 0;
            long usedMemoryBytes = 0;
            long connectedClients = 0;
            long totalCommandsProcessed = 0;
            string redisVersion = "";
            long uptimeInSeconds = 0;
            double hitRate = 0;

            foreach (var section in info)
            {
                foreach (var pair in section)
                {
                    switch (pair.Key.ToLower())
                    {
                        case "redis_version":
                            redisVersion = pair.Value;
                            break;
                        case "uptime_in_seconds":
                            long.TryParse(pair.Value, out uptimeInSeconds);
                            break;
                        case "connected_clients":
                            long.TryParse(pair.Value, out connectedClients);
                            break;
                        case "used_memory":
                            long.TryParse(pair.Value, out usedMemoryBytes);
                            break;
                        case "total_commands_processed":
                            long.TryParse(pair.Value, out totalCommandsProcessed);
                            break;
                    }
                }
            }

            // 取得 keyspace 資訊
            var keyspaceInfo = info.FirstOrDefault(s => s.Any(p => p.Key.StartsWith("db")));
            if (keyspaceInfo != null)
            {
                foreach (var db in keyspaceInfo)
                {
                    // 格式: keys=123,expires=45,avg_ttl=67890
                    var parts = db.Value.Split(',');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("keys="))
                        {
                            long.TryParse(part.Substring(5), out var keys);
                            totalKeys += keys;
                        }
                    }
                }
            }

            // 計算命中率
            var statsSection = info.FirstOrDefault(s => s.Any(p => p.Key == "keyspace_hits"));
            if (statsSection != null)
            {
                long.TryParse(statsSection.FirstOrDefault(p => p.Key == "keyspace_hits").Value, out var hits);
                long.TryParse(statsSection.FirstOrDefault(p => p.Key == "keyspace_misses").Value, out var misses);
                if (hits + misses > 0)
                {
                    hitRate = Math.Round((double)hits / (hits + misses) * 100, 2);
                }
            }

            var response = new CacheStatsResponse
            {
                Status = "Healthy",
                Message = "Redis 快取服務運作正常",
                RedisVersion = redisVersion,
                UptimeInSeconds = uptimeInSeconds,
                ConnectedClients = connectedClients,
                TotalKeys = totalKeys,
                UsedMemoryBytes = usedMemoryBytes,
                UsedMemoryHuman = FormatBytes(usedMemoryBytes),
                TotalCommandsProcessed = totalCommandsProcessed,
                HitRate = hitRate
            };

            return Ok(ApiResponse<CacheStatsResponse>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 Redis 統計資訊失敗");
            return Ok(ApiResponse<CacheStatsResponse>.SuccessResponse(new CacheStatsResponse
            {
                Status = "Error",
                Message = $"無法取得 Redis 統計資訊: {ex.Message}"
            }));
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:0.##} {suffixes[suffixIndex]}";
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

    /// <summary>
    /// Redis 版本
    /// </summary>
    public string RedisVersion { get; set; } = string.Empty;

    /// <summary>
    /// 運行時間（秒）
    /// </summary>
    public long UptimeInSeconds { get; set; }

    /// <summary>
    /// 連線客戶端數量
    /// </summary>
    public long ConnectedClients { get; set; }

    /// <summary>
    /// 總快取鍵數量
    /// </summary>
    public long TotalKeys { get; set; }

    /// <summary>
    /// 使用記憶體（Bytes）
    /// </summary>
    public long UsedMemoryBytes { get; set; }

    /// <summary>
    /// 使用記憶體（人類可讀格式）
    /// </summary>
    public string UsedMemoryHuman { get; set; } = string.Empty;

    /// <summary>
    /// 已處理命令總數
    /// </summary>
    public long TotalCommandsProcessed { get; set; }

    /// <summary>
    /// 命中率 (%)
    /// </summary>
    public double HitRate { get; set; }
}
