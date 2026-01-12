using System.Text.Json;
using DotnetApiDemo.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// Redis 快取服務實作
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string _instanceName;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
        _instanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "DotnetApiDemo:";
    }

    private string GetFullKey(string key) => $"{_instanceName}{key}";

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從快取取得資料失敗: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            var data = JsonSerializer.Serialize(value, JsonOptions);
            await _distributedCache.SetStringAsync(key, data, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定快取資料失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除快取資料失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查快取是否存在失敗: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var fullPattern = GetFullKey(pattern);
            var endpoints = _redis.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: fullPattern).ToArray();

                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                    _logger.LogInformation("依模式移除快取: {Pattern}, 移除 {Count} 個鍵", pattern, keys.Length);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "依模式移除快取失敗: {Pattern}", pattern);
        }
    }

    /// <inheritdoc />
    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.StringSetAsync(fullKey, value, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定字串快取失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.StringGetAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得字串快取失敗: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task HashSetAsync(string key, string field, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.HashSetAsync(fullKey, field, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定 Hash 欄位失敗: {Key}.{Field}", key, field);
        }
    }

    /// <inheritdoc />
    public async Task<string?> HashGetAsync(string key, string field)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.HashGetAsync(fullKey, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 Hash 欄位失敗: {Key}.{Field}", key, field);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var entries = await _database.HashGetAllAsync(fullKey);
            return entries.ToDictionary(
                e => e.Name.ToString(),
                e => e.Value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 Hash 所有欄位失敗: {Key}", key);
            return new Dictionary<string, string>();
        }
    }

    /// <inheritdoc />
    public async Task HashDeleteAsync(string key, string field)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.HashDeleteAsync(fullKey, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除 Hash 欄位失敗: {Key}.{Field}", key, field);
        }
    }

    /// <inheritdoc />
    public async Task SetExpirationAsync(string key, TimeSpan expiration)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.KeyExpireAsync(fullKey, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定過期時間失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.StringIncrementAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "遞增計數器失敗: {Key}", key);
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task ListLeftPushAsync(string key, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.ListLeftPushAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送至 List 失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<string?> ListRightPopAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.ListRightPopAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從 List 取出失敗: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<long> ListLengthAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.ListLengthAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 List 長度失敗: {Key}", key);
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task SetAddAsync(string key, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.SetAddAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增至 Set 失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task SetRemoveAsync(string key, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.SetRemoveAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從 Set 移除失敗: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> SetMembersAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var members = await _database.SetMembersAsync(fullKey);
            return members.Select(m => m.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 Set 成員失敗: {Key}", key);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetContainsAsync(string key, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.SetContainsAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查 Set 是否包含成員失敗: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(string channel, string message)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發布訊息失敗: {Channel}", channel);
        }
    }
}
