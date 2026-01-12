using System.Text.Json;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 審計日誌佇列服務實作
/// </summary>
public class AuditQueueService : IAuditQueueService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuditQueueService> _logger;
    private const string QueueKey = "audit:log:queue";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditQueueService(
        ICacheService cacheService,
        ILogger<AuditQueueService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(AuditLogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await _cacheService.ListLeftPushAsync(QueueKey, json);
        _logger.LogDebug("審計日誌已加入佇列: {Id}, Action={Action}", entry.Id, entry.Action);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogEntry>> DequeueAsync(int count = 100)
    {
        var entries = new List<AuditLogEntry>();

        for (int i = 0; i < count; i++)
        {
            var json = await _cacheService.ListRightPopAsync(QueueKey);
            if (string.IsNullOrEmpty(json))
                break;

            try
            {
                var entry = JsonSerializer.Deserialize<AuditLogEntry>(json, JsonOptions);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "反序列化審計日誌失敗: {Json}", json);
            }
        }

        return entries;
    }

    /// <inheritdoc />
    public async Task<long> GetQueueLengthAsync()
    {
        return await _cacheService.ListLengthAsync(QueueKey);
    }
}
