using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// API 速率限制服務實作（滑動窗口演算法）
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<RateLimitService> _logger;
    private const string RateLimitKeyPrefix = "ratelimit:";

    public RateLimitService(
        ICacheService cacheService,
        ILogger<RateLimitService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RateLimitResult> CheckRateLimitAsync(string identifier, string endpoint, int limit, TimeSpan window)
    {
        var key = GenerateKey(identifier, endpoint, window);
        var windowSeconds = (int)window.TotalSeconds;

        // 遞增計數器
        var currentCount = await _cacheService.IncrementAsync(key);

        // 如果是第一次請求，設定過期時間
        if (currentCount == 1)
        {
            await _cacheService.SetExpirationAsync(key, window);
        }

        var resetTime = DateTimeOffset.UtcNow.Add(window).ToUnixTimeSeconds();
        var isAllowed = currentCount <= limit;

        if (!isAllowed)
        {
            _logger.LogWarning("速率限制觸發: Identifier={Identifier}, Endpoint={Endpoint}, Count={Count}, Limit={Limit}",
                identifier, endpoint, currentCount, limit);
        }

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            CurrentCount = currentCount,
            Limit = limit,
            ResetTime = resetTime,
            RetryAfterSeconds = isAllowed ? 0 : windowSeconds
        };
    }

    /// <inheritdoc />
    public async Task<long> GetCurrentCountAsync(string identifier, string endpoint)
    {
        var key = GenerateKey(identifier, endpoint, TimeSpan.FromMinutes(1));
        var value = await _cacheService.GetStringAsync(key);
        return long.TryParse(value, out var count) ? count : 0;
    }

    private static string GenerateKey(string identifier, string endpoint, TimeSpan window)
    {
        // 使用時間窗口來生成唯一鍵
        var windowStart = GetWindowStart(window);
        var normalizedEndpoint = NormalizeEndpoint(endpoint);
        return $"{RateLimitKeyPrefix}{normalizedEndpoint}:{identifier}:{windowStart}";
    }

    private static string GetWindowStart(TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow;
        var windowSeconds = (long)window.TotalSeconds;
        var windowStart = now.ToUnixTimeSeconds() / windowSeconds * windowSeconds;
        return windowStart.ToString();
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        // 移除動態參數部分，例如 /api/v1/products/123 -> /api/v1/products/*
        return endpoint.ToLowerInvariant()
            .Replace("//", "/")
            .TrimEnd('/');
    }
}
