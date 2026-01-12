using Microsoft.AspNetCore.SignalR;
using DotnetApiDemo.Hubs;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 通知服務實作
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendToUserAsync(int userId, NotificationMessage notification)
    {
        var groupName = $"user:{userId}";
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        _logger.LogDebug("發送通知給使用者: UserId={UserId}, Type={Type}", userId, notification.Type);
    }

    /// <inheritdoc />
    public async Task SendToUsersAsync(IEnumerable<int> userIds, NotificationMessage notification)
    {
        var tasks = userIds.Select(userId => SendToUserAsync(userId, notification));
        await Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public async Task SendToGroupAsync(string groupName, NotificationMessage notification)
    {
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        _logger.LogDebug("發送通知給群組: Group={GroupName}, Type={Type}", groupName, notification.Type);
    }

    /// <inheritdoc />
    public async Task BroadcastAsync(NotificationMessage notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("廣播通知: Type={Type}, Title={Title}", notification.Type, notification.Title);
    }

    /// <inheritdoc />
    public async Task SendLowStockAlertAsync(int productId, string productName, int currentStock, int safetyStock)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.LowStock,
            Title = "庫存警示",
            Content = $"商品「{productName}」庫存不足，目前庫存: {currentStock}，安全庫存: {safetyStock}",
            Data = new
            {
                productId,
                productName,
                currentStock,
                safetyStock,
                shortage = safetyStock - currentStock
            }
        };

        // 發送給管理人員群組
        await SendToGroupAsync("managers", notification);
        _logger.LogWarning("庫存警示: ProductId={ProductId}, Name={Name}, Stock={Stock}/{SafetyStock}",
            productId, productName, currentStock, safetyStock);
    }

    /// <inheritdoc />
    public async Task SendNewOrderNotificationAsync(int orderId, string orderNo, decimal amount, int storeId)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.NewOrder,
            Title = "新訂單",
            Content = $"收到新訂單 {orderNo}，金額: {amount:N0}",
            Data = new
            {
                orderId,
                orderNo,
                amount,
                storeId
            }
        };

        // 發送給門市群組
        await SendToGroupAsync($"store:{storeId}", notification);
        _logger.LogInformation("新訂單通知: OrderNo={OrderNo}, StoreId={StoreId}, Amount={Amount}",
            orderNo, storeId, amount);
    }
}
