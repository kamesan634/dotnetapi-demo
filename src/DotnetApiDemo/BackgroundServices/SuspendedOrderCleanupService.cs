using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.BackgroundServices;

/// <summary>
/// 過期掛單清理背景服務
/// </summary>
public class SuspendedOrderCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SuspendedOrderCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(15);

    public SuspendedOrderCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SuspendedOrderCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("過期掛單清理服務啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期掛單時發生錯誤");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("過期掛單清理服務停止");
    }

    private async Task CleanupExpiredOrdersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var suspendedOrderService = scope.ServiceProvider.GetRequiredService<ISuspendedOrderService>();

        var cleanedCount = await suspendedOrderService.CleanupExpiredOrdersAsync();

        if (cleanedCount > 0)
        {
            _logger.LogInformation("已清理 {Count} 筆過期掛單", cleanedCount);
        }
    }
}
