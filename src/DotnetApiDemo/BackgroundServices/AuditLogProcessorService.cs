using DotnetApiDemo.Data;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.BackgroundServices;

/// <summary>
/// 審計日誌處理背景服務
/// </summary>
public class AuditLogProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogProcessorService> _logger;
    private readonly TimeSpan _processInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 100;

    public AuditLogProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("審計日誌處理服務啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理審計日誌佇列時發生錯誤");
            }

            await Task.Delay(_processInterval, stoppingToken);
        }

        _logger.LogInformation("審計日誌處理服務停止");
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var auditQueueService = scope.ServiceProvider.GetRequiredService<IAuditQueueService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entries = await auditQueueService.DequeueAsync(_batchSize);
        var entryList = entries.ToList();

        if (entryList.Count == 0)
            return;

        _logger.LogDebug("處理 {Count} 筆審計日誌", entryList.Count);

        var auditLogs = entryList.Select(e => new AuditLog
        {
            UserId = e.UserId,
            UserName = e.UserName,
            Action = e.Action,
            Module = e.Module,
            TargetId = e.TargetId,
            TargetType = e.TargetType,
            OldValue = e.OldValue,
            NewValue = e.NewValue,
            IpAddress = e.IpAddress,
            UserAgent = e.UserAgent,
            CreatedAt = e.CreatedAt
        }).ToList();

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync(stoppingToken);

        _logger.LogInformation("已寫入 {Count} 筆審計日誌到資料庫", entryList.Count);
    }
}
