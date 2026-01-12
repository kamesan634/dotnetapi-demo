namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 通知服務介面
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 發送通知給指定使用者
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="notification">通知內容</param>
    Task SendToUserAsync(int userId, NotificationMessage notification);

    /// <summary>
    /// 發送通知給多個使用者
    /// </summary>
    /// <param name="userIds">使用者 ID 清單</param>
    /// <param name="notification">通知內容</param>
    Task SendToUsersAsync(IEnumerable<int> userIds, NotificationMessage notification);

    /// <summary>
    /// 發送通知給指定群組
    /// </summary>
    /// <param name="groupName">群組名稱</param>
    /// <param name="notification">通知內容</param>
    Task SendToGroupAsync(string groupName, NotificationMessage notification);

    /// <summary>
    /// 發送廣播通知（所有連線使用者）
    /// </summary>
    /// <param name="notification">通知內容</param>
    Task BroadcastAsync(NotificationMessage notification);

    /// <summary>
    /// 發送庫存警示通知
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <param name="productName">商品名稱</param>
    /// <param name="currentStock">目前庫存</param>
    /// <param name="safetyStock">安全庫存</param>
    Task SendLowStockAlertAsync(int productId, string productName, int currentStock, int safetyStock);

    /// <summary>
    /// 發送新訂單通知
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="orderNo">訂單編號</param>
    /// <param name="amount">金額</param>
    /// <param name="storeId">門市 ID</param>
    Task SendNewOrderNotificationAsync(int orderId, string orderNo, decimal amount, int storeId);
}

/// <summary>
/// 通知訊息
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// 訊息 ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 通知類型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 附加資料
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 通知類型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 一般訊息
    /// </summary>
    Info,

    /// <summary>
    /// 成功訊息
    /// </summary>
    Success,

    /// <summary>
    /// 警告訊息
    /// </summary>
    Warning,

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    Error,

    /// <summary>
    /// 新訂單
    /// </summary>
    NewOrder,

    /// <summary>
    /// 庫存警示
    /// </summary>
    LowStock,

    /// <summary>
    /// 系統公告
    /// </summary>
    SystemAnnouncement
}
