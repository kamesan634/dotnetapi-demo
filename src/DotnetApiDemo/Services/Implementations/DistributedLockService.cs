using DotnetApiDemo.Services.Interfaces;
using StackExchange.Redis;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 分散式鎖服務實作（使用 Redis）
/// </summary>
public class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLockService> _logger;
    private const string LockKeyPrefix = "lock:";

    public DistributedLockService(
        IConnectionMultiplexer redis,
        ILogger<DistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDistributedLock?> AcquireAsync(string resource, TimeSpan expiry)
    {
        return await AcquireAsync(resource, expiry, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    /// <inheritdoc />
    public async Task<IDistributedLock?> AcquireAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry)
    {
        var lockKey = $"{LockKeyPrefix}{resource}";
        var lockValue = Guid.NewGuid().ToString();
        var database = _redis.GetDatabase();
        var startTime = DateTime.UtcNow;

        do
        {
            // 嘗試獲取鎖
            var acquired = await database.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("獲取分散式鎖成功: {Resource}", resource);
                return new RedisDistributedLock(database, lockKey, lockValue, _logger);
            }

            if (wait == TimeSpan.Zero)
            {
                _logger.LogDebug("獲取分散式鎖失敗（無等待）: {Resource}", resource);
                return null;
            }

            // 等待重試
            await Task.Delay(retry);

        } while (DateTime.UtcNow - startTime < wait);

        _logger.LogWarning("獲取分散式鎖超時: {Resource}", resource);
        return null;
    }
}

/// <summary>
/// Redis 分散式鎖實作
/// </summary>
internal class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockKey;
    private readonly string _lockValue;
    private readonly ILogger _logger;
    private bool _isReleased;

    public string Resource => _lockKey;
    public bool IsAcquired => !_isReleased;

    public RedisDistributedLock(IDatabase database, string lockKey, string lockValue, ILogger logger)
    {
        _database = database;
        _lockKey = lockKey;
        _lockValue = lockValue;
        _logger = logger;
        _isReleased = false;
    }

    public async Task ReleaseAsync()
    {
        if (_isReleased) return;

        // 使用 Lua 腳本確保只有鎖的擁有者才能釋放
        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });

        _isReleased = true;
        _logger.LogDebug("釋放分散式鎖: {Resource}", _lockKey);
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }
}
