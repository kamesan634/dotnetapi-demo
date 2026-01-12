using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// Token 黑名單服務實作
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TokenBlacklistService> _logger;
    private const string BlacklistKeyPrefix = "blacklist:token:";
    private const string UserTokensKeyPrefix = "user:tokens:";

    public TokenBlacklistService(
        ICacheService cacheService,
        ILogger<TokenBlacklistService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task BlacklistTokenAsync(string jti, DateTime expiration)
    {
        var ttl = expiration - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            _logger.LogDebug("Token 已過期，無需加入黑名單: {Jti}", jti);
            return;
        }

        var key = $"{BlacklistKeyPrefix}{jti}";
        await _cacheService.SetStringAsync(key, "1", ttl);
        _logger.LogInformation("Token 已加入黑名單: {Jti}, TTL: {Ttl}", jti, ttl);
    }

    /// <inheritdoc />
    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var key = $"{BlacklistKeyPrefix}{jti}";
        var exists = await _cacheService.ExistsAsync(key);
        return exists;
    }

    /// <inheritdoc />
    public async Task BlacklistUserTokensAsync(int userId)
    {
        var userTokensKey = $"{UserTokensKeyPrefix}{userId}";
        var tokens = await _cacheService.HashGetAllAsync(userTokensKey);

        foreach (var token in tokens)
        {
            var jti = token.Key;
            if (DateTime.TryParse(token.Value, out var expiration))
            {
                await BlacklistTokenAsync(jti, expiration);
            }
        }

        // 清除使用者的 Token 追蹤記錄
        await _cacheService.RemoveAsync(userTokensKey);
        _logger.LogInformation("使用者所有 Token 已加入黑名單: UserId={UserId}, Count={Count}", userId, tokens.Count);
    }

    /// <inheritdoc />
    public async Task TrackUserTokenAsync(int userId, string jti, DateTime expiration)
    {
        var userTokensKey = $"{UserTokensKeyPrefix}{userId}";
        await _cacheService.HashSetAsync(userTokensKey, jti, expiration.ToString("O"));

        // 設定過期時間為最長 Token 有效期（7天）
        await _cacheService.SetExpirationAsync(userTokensKey, TimeSpan.FromDays(7));
    }
}
