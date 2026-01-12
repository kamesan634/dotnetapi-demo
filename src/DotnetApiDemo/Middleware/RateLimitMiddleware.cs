using System.Security.Claims;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Middleware;

/// <summary>
/// API 速率限制中介軟體
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        ILogger<RateLimitMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _options = configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? new RateLimitOptions();
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        // 跳過不需要限制的路徑
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (ShouldSkip(path))
        {
            await _next(context);
            return;
        }

        // 取得識別碼（優先使用 UserId，否則使用 IP）
        var identifier = GetIdentifier(context);
        var endpoint = $"{context.Request.Method}:{path}";

        // 根據端點類型決定限制
        var (limit, window) = GetLimitForEndpoint(path);

        var result = await rateLimitService.CheckRateLimitAsync(identifier, endpoint, limit, window);

        // 設定回應標頭
        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = result.ResetTime.ToString();

        if (!result.IsAllowed)
        {
            context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "請求過於頻繁，請稍後再試",
                retryAfter = result.RetryAfterSeconds
            });
            return;
        }

        await _next(context);
    }

    private string GetIdentifier(HttpContext context)
    {
        // 優先使用已認證的使用者 ID
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // 否則使用 IP 地址
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 檢查 X-Forwarded-For 標頭（反向代理情況）
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',').First().Trim();
        }

        return $"ip:{ip}";
    }

    private (int limit, TimeSpan window) GetLimitForEndpoint(string path)
    {
        // 登入端點：較嚴格的限制
        if (path.Contains("/auth/login"))
        {
            return (_options.LoginLimit, TimeSpan.FromMinutes(_options.LoginWindowMinutes));
        }

        // 匯出端點：限制較少但時間窗口較長
        if (path.Contains("/export"))
        {
            return (_options.ExportLimit, TimeSpan.FromHours(_options.ExportWindowHours));
        }

        // 報表端點：中等限制
        if (path.Contains("/reports"))
        {
            return (_options.ReportLimit, TimeSpan.FromMinutes(_options.ReportWindowMinutes));
        }

        // 一般 API：預設限制
        return (_options.DefaultLimit, TimeSpan.FromMinutes(_options.DefaultWindowMinutes));
    }

    private bool ShouldSkip(string path)
    {
        // 跳過健康檢查和 Swagger
        return path.StartsWith("/health") ||
               path.StartsWith("/swagger") ||
               path.StartsWith("/favicon");
    }
}

/// <summary>
/// 速率限制配置選項
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// 預設限制次數（每分鐘）
    /// </summary>
    public int DefaultLimit { get; set; } = 100;

    /// <summary>
    /// 預設時間窗口（分鐘）
    /// </summary>
    public int DefaultWindowMinutes { get; set; } = 1;

    /// <summary>
    /// 登入限制次數
    /// </summary>
    public int LoginLimit { get; set; } = 5;

    /// <summary>
    /// 登入時間窗口（分鐘）
    /// </summary>
    public int LoginWindowMinutes { get; set; } = 1;

    /// <summary>
    /// 匯出限制次數
    /// </summary>
    public int ExportLimit { get; set; } = 10;

    /// <summary>
    /// 匯出時間窗口（小時）
    /// </summary>
    public int ExportWindowHours { get; set; } = 1;

    /// <summary>
    /// 報表限制次數
    /// </summary>
    public int ReportLimit { get; set; } = 30;

    /// <summary>
    /// 報表時間窗口（分鐘）
    /// </summary>
    public int ReportWindowMinutes { get; set; } = 1;
}

/// <summary>
/// 速率限制中介軟體擴充方法
/// </summary>
public static class RateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitMiddleware>();
    }
}
