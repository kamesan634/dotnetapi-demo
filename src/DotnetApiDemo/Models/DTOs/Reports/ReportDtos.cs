namespace DotnetApiDemo.Models.DTOs.Reports;

/// <summary>
/// 儀表板摘要 DTO
/// </summary>
/// <remarks>
/// 提供今日關鍵營運指標
/// </remarks>
public class DashboardSummaryDto
{
    /// <summary>
    /// 今日銷售額
    /// </summary>
    public decimal TodaySales { get; set; }

    /// <summary>
    /// 今日訂單數
    /// </summary>
    public int TodayOrders { get; set; }

    /// <summary>
    /// 今日來客數
    /// </summary>
    public int TodayCustomers { get; set; }

    /// <summary>
    /// 低庫存商品數
    /// </summary>
    public int LowStockCount { get; set; }

    /// <summary>
    /// 昨日銷售額 (用於計算成長率)
    /// </summary>
    public decimal YesterdaySales { get; set; }

    /// <summary>
    /// 銷售成長率 (%)
    /// </summary>
    public decimal SalesGrowthRate { get; set; }

    /// <summary>
    /// 今日平均客單價
    /// </summary>
    public decimal TodayAverageOrderValue { get; set; }

    /// <summary>
    /// 待處理訂單數
    /// </summary>
    public int PendingOrdersCount { get; set; }
}

/// <summary>
/// 銷售報表 DTO
/// </summary>
/// <remarks>
/// 記錄每日銷售統計資料
/// </remarks>
public class SalesReportDto
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 總銷售額
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// 訂單數
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 平均客單價
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// 退貨金額
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 淨銷售額
    /// </summary>
    public decimal NetSales { get; set; }

    /// <summary>
    /// 稅額
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// 折扣金額
    /// </summary>
    public decimal DiscountAmount { get; set; }
}

/// <summary>
/// 熱銷商品 DTO
/// </summary>
/// <remarks>
/// 記錄商品銷售排行
/// </remarks>
public class TopProductDto
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 銷售數量
    /// </summary>
    public int QuantitySold { get; set; }

    /// <summary>
    /// 銷售金額
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// 訂單數
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 排名
    /// </summary>
    public int Rank { get; set; }
}

/// <summary>
/// 庫存報表 DTO
/// </summary>
/// <remarks>
/// 提供庫存狀態摘要
/// </remarks>
public class InventoryReportDto
{
    /// <summary>
    /// 總商品數
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// 總庫存數量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 總庫存價值 (以成本計算)
    /// </summary>
    public decimal TotalStockValue { get; set; }

    /// <summary>
    /// 低庫存商品數
    /// </summary>
    public int LowStockCount { get; set; }

    /// <summary>
    /// 缺貨商品數
    /// </summary>
    public int OutOfStockCount { get; set; }

    /// <summary>
    /// 過剩庫存商品數
    /// </summary>
    public int OverStockCount { get; set; }

    /// <summary>
    /// 低庫存商品列表
    /// </summary>
    public IEnumerable<LowStockItemDto> LowStockItems { get; set; } = Enumerable.Empty<LowStockItemDto>();
}

/// <summary>
/// 低庫存項目 DTO
/// </summary>
public class LowStockItemDto
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品編號
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 商品名稱
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// 目前庫存
    /// </summary>
    public int CurrentQuantity { get; set; }

    /// <summary>
    /// 安全庫存量
    /// </summary>
    public int SafetyStock { get; set; }

    /// <summary>
    /// 缺口數量
    /// </summary>
    public int ShortageQuantity { get; set; }
}

/// <summary>
/// 採購報表 DTO
/// </summary>
/// <remarks>
/// 提供採購統計摘要
/// </remarks>
public class PurchaseReportDto
{
    /// <summary>
    /// 總採購單數
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// 總採購金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 已完成採購單數
    /// </summary>
    public int CompletedOrders { get; set; }

    /// <summary>
    /// 待處理採購單數
    /// </summary>
    public int PendingOrders { get; set; }

    /// <summary>
    /// 平均採購單金額
    /// </summary>
    public decimal AverageOrderAmount { get; set; }

    /// <summary>
    /// 供應商統計列表
    /// </summary>
    public IEnumerable<SupplierPurchaseSummaryDto> SupplierSummaries { get; set; } = Enumerable.Empty<SupplierPurchaseSummaryDto>();
}

/// <summary>
/// 供應商採購摘要 DTO
/// </summary>
public class SupplierPurchaseSummaryDto
{
    /// <summary>
    /// 供應商 ID
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// 供應商名稱
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 採購單數
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 採購金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 佔比 (%)
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// 客戶報表 DTO
/// </summary>
/// <remarks>
/// 提供客戶分析摘要
/// </remarks>
public class CustomerReportDto
{
    /// <summary>
    /// 總客戶數
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// 新客戶數 (本月)
    /// </summary>
    public int NewCustomersThisMonth { get; set; }

    /// <summary>
    /// 活躍客戶數 (近30天有消費)
    /// </summary>
    public int ActiveCustomers { get; set; }

    /// <summary>
    /// 沉睡客戶數 (超過90天未消費)
    /// </summary>
    public int DormantCustomers { get; set; }

    /// <summary>
    /// 平均客戶消費金額
    /// </summary>
    public decimal AverageCustomerSpending { get; set; }

    /// <summary>
    /// 總會員點數
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// VIP 客戶數
    /// </summary>
    public int VipCustomers { get; set; }

    /// <summary>
    /// 客戶等級分佈
    /// </summary>
    public IEnumerable<CustomerLevelDistributionDto> LevelDistribution { get; set; } = Enumerable.Empty<CustomerLevelDistributionDto>();

    /// <summary>
    /// 頂級客戶列表
    /// </summary>
    public IEnumerable<TopCustomerDto> TopCustomers { get; set; } = Enumerable.Empty<TopCustomerDto>();
}

/// <summary>
/// 客戶等級分佈 DTO
/// </summary>
public class CustomerLevelDistributionDto
{
    /// <summary>
    /// 等級 ID
    /// </summary>
    public int LevelId { get; set; }

    /// <summary>
    /// 等級名稱
    /// </summary>
    public string LevelName { get; set; } = string.Empty;

    /// <summary>
    /// 客戶數
    /// </summary>
    public int CustomerCount { get; set; }

    /// <summary>
    /// 佔比 (%)
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// 頂級客戶 DTO
/// </summary>
public class TopCustomerDto
{
    /// <summary>
    /// 客戶 ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// 會員編號
    /// </summary>
    public string MemberNo { get; set; } = string.Empty;

    /// <summary>
    /// 客戶姓名
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// 會員等級
    /// </summary>
    public string LevelName { get; set; } = string.Empty;

    /// <summary>
    /// 總消費金額
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// 總訂單數
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// 最後消費日期
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }

    /// <summary>
    /// 排名
    /// </summary>
    public int Rank { get; set; }
}

/// <summary>
/// 銷售趨勢 DTO
/// </summary>
public class SalesTrendDto
{
    /// <summary>
    /// 期間 (日期或週/月標籤)
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 銷售額
    /// </summary>
    public decimal Sales { get; set; }

    /// <summary>
    /// 訂單數
    /// </summary>
    public int OrderCount { get; set; }
}
