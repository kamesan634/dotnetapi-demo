using DotnetApiDemo.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using DotnetApiDemo.Models.Entities;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 使用者在線狀態服務實作
/// </summary>
public class UserPresenceService : IUserPresenceService
{
    private readonly ICacheService _cacheService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserPresenceService> _logger;

    private const string OnlineUsersKey = "online:users";
    private const string SessionKeyPrefix = "session:user:";
    private const int SessionTimeoutMinutes = 30;

    public UserPresenceService(
        ICacheService cacheService,
        IServiceScopeFactory scopeFactory,
        ILogger<UserPresenceService> logger)
    {
        _cacheService = cacheService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SetOnlineAsync(int userId, string? ipAddress = null, string? userAgent = null)
    {
        var sessionKey = $"{SessionKeyPrefix}{userId}";
        var now = DateTime.UtcNow;

        // 設定 Session 資訊
        await _cacheService.HashSetAsync(sessionKey, "loginTime", now.ToString("O"));
        await _cacheService.HashSetAsync(sessionKey, "lastActiveTime", now.ToString("O"));

        if (!string.IsNullOrEmpty(ipAddress))
            await _cacheService.HashSetAsync(sessionKey, "ip", ipAddress);

        if (!string.IsNullOrEmpty(userAgent))
            await _cacheService.HashSetAsync(sessionKey, "userAgent", userAgent);

        // 設定過期時間
        await _cacheService.SetExpirationAsync(sessionKey, TimeSpan.FromMinutes(SessionTimeoutMinutes));

        // 加入在線使用者集合
        await _cacheService.SetAddAsync(OnlineUsersKey, userId.ToString());

        _logger.LogInformation("使用者上線: UserId={UserId}", userId);
    }

    /// <inheritdoc />
    public async Task SetOfflineAsync(int userId)
    {
        var sessionKey = $"{SessionKeyPrefix}{userId}";

        // 移除 Session 資訊
        await _cacheService.RemoveAsync(sessionKey);

        // 從在線使用者集合移除
        await _cacheService.SetRemoveAsync(OnlineUsersKey, userId.ToString());

        _logger.LogInformation("使用者離線: UserId={UserId}", userId);
    }

    /// <inheritdoc />
    public async Task UpdateActivityAsync(int userId)
    {
        var sessionKey = $"{SessionKeyPrefix}{userId}";
        var exists = await _cacheService.ExistsAsync(sessionKey);

        if (exists)
        {
            var now = DateTime.UtcNow;
            await _cacheService.HashSetAsync(sessionKey, "lastActiveTime", now.ToString("O"));
            // 重新設定過期時間
            await _cacheService.SetExpirationAsync(sessionKey, TimeSpan.FromMinutes(SessionTimeoutMinutes));
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsOnlineAsync(int userId)
    {
        return await _cacheService.SetContainsAsync(OnlineUsersKey, userId.ToString());
    }

    /// <inheritdoc />
    public async Task<int> GetOnlineCountAsync()
    {
        var members = await _cacheService.SetMembersAsync(OnlineUsersKey);
        return members.Count();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OnlineUserInfo>> GetOnlineUsersAsync()
    {
        var members = await _cacheService.SetMembersAsync(OnlineUsersKey);
        var result = new List<OnlineUserInfo>();

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var member in members)
        {
            if (int.TryParse(member, out var userId))
            {
                var session = await GetUserSessionAsync(userId);
                if (session == null)
                {
                    // Session 已過期，從在線清單移除
                    await _cacheService.SetRemoveAsync(OnlineUsersKey, member);
                    continue;
                }

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    result.Add(new OnlineUserInfo
                    {
                        UserId = userId,
                        UserName = user.UserName ?? string.Empty,
                        FullName = user.RealName,
                        LoginTime = session.LoginTime,
                        LastActiveTime = session.LastActiveTime
                    });
                }
            }
        }

        return result.OrderByDescending(u => u.LastActiveTime);
    }

    /// <inheritdoc />
    public async Task<UserSessionInfo?> GetUserSessionAsync(int userId)
    {
        var sessionKey = $"{SessionKeyPrefix}{userId}";
        var sessionData = await _cacheService.HashGetAllAsync(sessionKey);

        if (sessionData.Count == 0)
            return null;

        var session = new UserSessionInfo { UserId = userId };

        if (sessionData.TryGetValue("loginTime", out var loginTime) && DateTime.TryParse(loginTime, out var lt))
            session.LoginTime = lt;

        if (sessionData.TryGetValue("lastActiveTime", out var lastActive) && DateTime.TryParse(lastActive, out var la))
            session.LastActiveTime = la;

        if (sessionData.TryGetValue("ip", out var ip))
            session.IpAddress = ip;

        if (sessionData.TryGetValue("userAgent", out var ua))
            session.UserAgent = ua;

        return session;
    }
}
