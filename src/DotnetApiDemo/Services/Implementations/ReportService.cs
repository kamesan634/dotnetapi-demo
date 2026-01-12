using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 報表服務實作
/// </summary>
/// <remarks>
/// 使用 ApplicationDbContext 進行資料查詢與統計計算
/// </remarks>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        // 今日銷售統計
        var todayOrders = await _context.Orders
            .Where(o => o.OrderDate == today && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var todaySales = todayOrders.Sum(o => o.TotalAmount);
        var todayOrderCount = todayOrders.Count;
        var todayCustomerCount = todayOrders
            .Where(o => o.CustomerId.HasValue)
            .Select(o => o.CustomerId)
            .Distinct()
            .Count();

        // 今日平均客單價
        var todayAverageOrderValue = todayOrderCount > 0 ? todaySales / todayOrderCount : 0;

        // 昨日銷售額 (用於計算成長率)
        var yesterdaySales = await _context.Orders
            .Where(o => o.OrderDate == yesterday && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);

        // 銷售成長率
        var salesGrowthRate = yesterdaySales > 0
            ? Math.Round((todaySales - yesterdaySales) / yesterdaySales * 100, 2)
            : 0;

        // 低庫存商品數
        var lowStockCount = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity <= i.Product.SafetyStock && i.Product.IsActive)
            .Select(i => i.ProductId)
            .Distinct()
            .CountAsync();

        // 待處理訂單數
        var pendingOrdersCount = await _context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);

        _logger.LogInformation("取得儀表板摘要成功 - 今日銷售: {TodaySales}, 訂單數: {OrderCount}", todaySales, todayOrderCount);

        return new DashboardSummaryDto
        {
            TodaySales = todaySales,
            TodayOrders = todayOrderCount,
            TodayCustomers = todayCustomerCount,
            LowStockCount = lowStockCount,
            YesterdaySales = yesterdaySales,
            SalesGrowthRate = salesGrowthRate,
            TodayAverageOrderValue = todayAverageOrderValue,
            PendingOrdersCount = pendingOrdersCount
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateOnly startDate, DateOnly endDate)
    {
        var salesData = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.OrderDate)
            .Select(g => new SalesReportDto
            {
                Date = g.Key,
                TotalSales = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                AverageOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0,
                RefundAmount = 0, // TODO: 實作退貨金額計算
                NetSales = g.Sum(o => o.TotalAmount),
                TaxAmount = g.Sum(o => o.TaxAmount),
                DiscountAmount = g.Sum(o => o.DiscountAmount)
            })
            .OrderBy(s => s.Date)
            .ToListAsync();

        _logger.LogInformation("取得銷售報表成功 - 期間: {StartDate} ~ {EndDate}, 筆數: {Count}",
            startDate, endDate, salesData.Count);

        return salesData;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int limit, DateOnly startDate, DateOnly endDate)
    {
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.OrderDate >= startDate &&
                         oi.Order.OrderDate <= endDate &&
                         oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Sku, oi.Product.Name, CategoryName = oi.Product.Category.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                Sku = g.Key.Sku,
                ProductName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Subtotal),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(p => p.Revenue)
            .Take(limit)
            .ToListAsync();

        // 設定排名
        for (int i = 0; i < topProducts.Count; i++)
        {
            topProducts[i].Rank = i + 1;
        }

        _logger.LogInformation("取得熱銷商品成功 - 期間: {StartDate} ~ {EndDate}, 前 {Limit} 名",
            startDate, endDate, limit);

        return topProducts;
    }

    /// <inheritdoc />
    public async Task<InventoryReportDto> GetInventorySummaryAsync()
    {
        // 總商品數 (有庫存的)
        var totalProducts = await _context.Inventories
            .Select(i => i.ProductId)
            .Distinct()
            .CountAsync();

        // 總庫存數量
        var totalQuantity = await _context.Inventories
            .SumAsync(i => i.Quantity);

        // 總庫存價值
        var stockValue = await _context.Inventories
            .Include(i => i.Product)
            .SumAsync(i => i.Quantity * i.Product.CostPrice);

        // 低庫存商品 (庫存量低於安全庫存)
        var lowStockItems = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity <= i.Product.SafetyStock && i.Quantity > 0 && i.Product.IsActive)
            .Select(i => new LowStockItemDto
            {
                ProductId = i.ProductId,
                Sku = i.Product.Sku,
                ProductName = i.Product.Name,
                WarehouseName = i.Warehouse.Name,
                CurrentQuantity = i.Quantity,
                SafetyStock = i.Product.SafetyStock,
                ShortageQuantity = i.Product.SafetyStock - i.Quantity
            })
            .OrderBy(i => i.CurrentQuantity)
            .Take(20)
            .ToListAsync();

        var lowStockCount = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity <= i.Product.SafetyStock && i.Quantity > 0 && i.Product.IsActive)
            .Select(i => i.ProductId)
            .Distinct()
            .CountAsync();

        // 缺貨商品數
        var outOfStockCount = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity == 0 && i.Product.IsActive)
            .Select(i => i.ProductId)
            .Distinct()
            .CountAsync();

        // 過剩庫存商品數 (庫存量超過安全庫存的 3 倍)
        var overStockCount = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity > i.Product.SafetyStock * 3 && i.Product.SafetyStock > 0 && i.Product.IsActive)
            .Select(i => i.ProductId)
            .Distinct()
            .CountAsync();

        _logger.LogInformation("取得庫存摘要成功 - 總商品數: {TotalProducts}, 低庫存: {LowStock}, 缺貨: {OutOfStock}",
            totalProducts, lowStockCount, outOfStockCount);

        return new InventoryReportDto
        {
            TotalProducts = totalProducts,
            TotalQuantity = totalQuantity,
            TotalStockValue = stockValue,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            OverStockCount = overStockCount,
            LowStockItems = lowStockItems
        };
    }

    /// <inheritdoc />
    public async Task<PurchaseReportDto> GetPurchaseSummaryAsync(DateOnly startDate, DateOnly endDate)
    {
        var purchaseOrders = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate)
            .ToListAsync();

        var totalOrders = purchaseOrders.Count;
        var totalAmount = purchaseOrders.Sum(po => po.TotalAmount);
        var completedOrders = purchaseOrders.Count(po => po.Status == PurchaseOrderStatus.Completed);
        var pendingOrders = purchaseOrders.Count(po => po.Status == PurchaseOrderStatus.Pending ||
                                                        po.Status == PurchaseOrderStatus.Approved);
        var averageOrderAmount = totalOrders > 0 ? totalAmount / totalOrders : 0;

        // 供應商統計
        var supplierSummaries = purchaseOrders
            .GroupBy(po => new { po.SupplierId, po.Supplier.Name })
            .Select(g => new SupplierPurchaseSummaryDto
            {
                SupplierId = g.Key.SupplierId,
                SupplierName = g.Key.Name,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(po => po.TotalAmount),
                Percentage = totalAmount > 0 ? Math.Round(g.Sum(po => po.TotalAmount) / totalAmount * 100, 2) : 0
            })
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToList();

        _logger.LogInformation("取得採購摘要成功 - 期間: {StartDate} ~ {EndDate}, 總採購單數: {TotalOrders}",
            startDate, endDate, totalOrders);

        return new PurchaseReportDto
        {
            TotalOrders = totalOrders,
            TotalAmount = totalAmount,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            AverageOrderAmount = averageOrderAmount,
            SupplierSummaries = supplierSummaries
        };
    }

    /// <inheritdoc />
    public async Task<CustomerReportDto> GetCustomerSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var thirtyDaysAgo = now.AddDays(-30);
        var ninetyDaysAgo = now.AddDays(-90);

        // 總客戶數
        var totalCustomers = await _context.Customers
            .Where(c => c.IsActive)
            .CountAsync();

        // 本月新客戶數
        var newCustomersThisMonth = await _context.Customers
            .Where(c => c.JoinDate >= thisMonthStart && c.IsActive)
            .CountAsync();

        // 活躍客戶數 (近30天有消費)
        var activeCustomers = await _context.Customers
            .Where(c => c.LastPurchaseDate >= thirtyDaysAgo && c.IsActive)
            .CountAsync();

        // 沉睡客戶數 (超過90天未消費)
        var dormantCustomers = await _context.Customers
            .Where(c => (c.LastPurchaseDate == null || c.LastPurchaseDate < ninetyDaysAgo) && c.IsActive)
            .CountAsync();

        // 平均客戶消費金額
        var averageSpending = await _context.Customers
            .Where(c => c.IsActive && c.TotalSpent > 0)
            .AverageAsync(c => (decimal?)c.TotalSpent) ?? 0;

        // 總會員點數
        var totalPoints = await _context.Customers
            .Where(c => c.IsActive)
            .SumAsync(c => c.CurrentPoints);

        // 客戶等級分佈
        var levelDistribution = await _context.Customers
            .Include(c => c.CustomerLevel)
            .Where(c => c.IsActive)
            .GroupBy(c => new { c.CustomerLevelId, c.CustomerLevel.Name })
            .Select(g => new CustomerLevelDistributionDto
            {
                LevelId = g.Key.CustomerLevelId,
                LevelName = g.Key.Name,
                CustomerCount = g.Count(),
                Percentage = 0 // 稍後計算
            })
            .ToListAsync();

        // 計算佔比
        foreach (var level in levelDistribution)
        {
            level.Percentage = totalCustomers > 0
                ? Math.Round((decimal)level.CustomerCount / totalCustomers * 100, 2)
                : 0;
        }

        // VIP 客戶數 (假設等級名稱包含 VIP 或 Gold/Platinum 等)
        var vipCustomers = await _context.Customers
            .Include(c => c.CustomerLevel)
            .Where(c => c.IsActive &&
                       (c.CustomerLevel.Name.Contains("VIP") ||
                        c.CustomerLevel.Name.Contains("Gold") ||
                        c.CustomerLevel.Name.Contains("Platinum") ||
                        c.CustomerLevel.Name.Contains("金") ||
                        c.CustomerLevel.Name.Contains("白金")))
            .CountAsync();

        // 頂級客戶
        var topCustomers = await _context.Customers
            .Include(c => c.CustomerLevel)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .Select(c => new TopCustomerDto
            {
                CustomerId = c.Id,
                MemberNo = c.MemberNo,
                CustomerName = c.Name,
                LevelName = c.CustomerLevel.Name,
                TotalSpent = c.TotalSpent,
                TotalOrders = c.TotalOrders,
                LastPurchaseDate = c.LastPurchaseDate
            })
            .ToListAsync();

        // 設定排名
        for (int i = 0; i < topCustomers.Count; i++)
        {
            topCustomers[i].Rank = i + 1;
        }

        _logger.LogInformation("取得客戶分析成功 - 總客戶數: {TotalCustomers}, 活躍: {Active}, 沉睡: {Dormant}",
            totalCustomers, activeCustomers, dormantCustomers);

        return new CustomerReportDto
        {
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomersThisMonth,
            ActiveCustomers = activeCustomers,
            DormantCustomers = dormantCustomers,
            AverageCustomerSpending = averageSpending,
            TotalPoints = totalPoints,
            VipCustomers = vipCustomers,
            LevelDistribution = levelDistribution,
            TopCustomers = topCustomers
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SalesTrendDto>> GetSalesTrendAsync(DateOnly startDate, DateOnly endDate)
    {
        var salesTrend = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.OrderDate)
            .Select(g => new SalesTrendDto
            {
                Period = g.Key.ToString("yyyy-MM-dd"),
                Sales = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(s => s.Period)
            .ToListAsync();

        _logger.LogInformation("取得銷售趨勢成功 - 期間: {StartDate} ~ {EndDate}", startDate, endDate);

        return salesTrend;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportSalesReportToCsvAsync(DateOnly startDate, DateOnly endDate)
    {
        var data = await GetSalesReportAsync(startDate, endDate);
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("日期,總銷售額,訂單數,平均客單價,退貨金額,淨銷售額,稅額,折扣金額");

        // Data
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.TotalSales},{item.OrderCount},{item.AverageOrderValue},{item.RefundAmount},{item.NetSales},{item.TaxAmount},{item.DiscountAmount}");
        }

        _logger.LogInformation("匯出銷售報表 CSV 成功 - 期間: {StartDate} ~ {EndDate}", startDate, endDate);
        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportInventoryReportToCsvAsync()
    {
        var report = await GetInventorySummaryAsync();
        var csv = new System.Text.StringBuilder();

        // Summary section
        csv.AppendLine("庫存摘要報表");
        csv.AppendLine($"總商品數,{report.TotalProducts}");
        csv.AppendLine($"總庫存數量,{report.TotalQuantity}");
        csv.AppendLine($"總庫存價值,{report.TotalStockValue}");
        csv.AppendLine($"低庫存商品數,{report.LowStockCount}");
        csv.AppendLine($"缺貨商品數,{report.OutOfStockCount}");
        csv.AppendLine($"過剩庫存商品數,{report.OverStockCount}");
        csv.AppendLine();

        // Low stock items
        csv.AppendLine("低庫存商品明細");
        csv.AppendLine("商品ID,商品編號,商品名稱,倉庫,目前庫存,安全庫存,缺口數量");
        foreach (var item in report.LowStockItems)
        {
            csv.AppendLine($"{item.ProductId},{item.Sku},{EscapeCsvField(item.ProductName)},{EscapeCsvField(item.WarehouseName)},{item.CurrentQuantity},{item.SafetyStock},{item.ShortageQuantity}");
        }

        _logger.LogInformation("匯出庫存報表 CSV 成功");
        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportTopProductsToCsvAsync(int limit, DateOnly startDate, DateOnly endDate)
    {
        var data = await GetTopProductsAsync(limit, startDate, endDate);
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("排名,商品ID,商品編號,商品名稱,分類,銷售數量,銷售金額,訂單數");

        // Data
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Rank},{item.ProductId},{item.Sku},{EscapeCsvField(item.ProductName)},{EscapeCsvField(item.CategoryName)},{item.QuantitySold},{item.Revenue},{item.OrderCount}");
        }

        _logger.LogInformation("匯出熱銷商品 CSV 成功 - 期間: {StartDate} ~ {EndDate}", startDate, endDate);
        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportCustomerReportToCsvAsync()
    {
        var report = await GetCustomerSummaryAsync();
        var csv = new System.Text.StringBuilder();

        // Summary section
        csv.AppendLine("客戶分析報表");
        csv.AppendLine($"總客戶數,{report.TotalCustomers}");
        csv.AppendLine($"本月新客戶數,{report.NewCustomersThisMonth}");
        csv.AppendLine($"活躍客戶數,{report.ActiveCustomers}");
        csv.AppendLine($"沉睡客戶數,{report.DormantCustomers}");
        csv.AppendLine($"平均客戶消費金額,{report.AverageCustomerSpending}");
        csv.AppendLine($"總會員點數,{report.TotalPoints}");
        csv.AppendLine($"VIP客戶數,{report.VipCustomers}");
        csv.AppendLine();

        // Level distribution
        csv.AppendLine("客戶等級分佈");
        csv.AppendLine("等級ID,等級名稱,客戶數,佔比(%)");
        foreach (var level in report.LevelDistribution)
        {
            csv.AppendLine($"{level.LevelId},{EscapeCsvField(level.LevelName)},{level.CustomerCount},{level.Percentage}");
        }
        csv.AppendLine();

        // Top customers
        csv.AppendLine("頂級客戶");
        csv.AppendLine("排名,客戶ID,會員編號,客戶姓名,會員等級,總消費金額,總訂單數,最後消費日期");
        foreach (var customer in report.TopCustomers)
        {
            csv.AppendLine($"{customer.Rank},{customer.CustomerId},{customer.MemberNo},{EscapeCsvField(customer.CustomerName)},{EscapeCsvField(customer.LevelName)},{customer.TotalSpent},{customer.TotalOrders},{customer.LastPurchaseDate:yyyy-MM-dd}");
        }

        _logger.LogInformation("匯出客戶報表 CSV 成功");
        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportPurchaseReportToCsvAsync(DateOnly startDate, DateOnly endDate)
    {
        var report = await GetPurchaseSummaryAsync(startDate, endDate);
        var csv = new System.Text.StringBuilder();

        // Summary section
        csv.AppendLine("採購報表");
        csv.AppendLine($"報表期間,{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}");
        csv.AppendLine($"總採購單數,{report.TotalOrders}");
        csv.AppendLine($"總採購金額,{report.TotalAmount}");
        csv.AppendLine($"已完成採購單數,{report.CompletedOrders}");
        csv.AppendLine($"待處理採購單數,{report.PendingOrders}");
        csv.AppendLine($"平均採購單金額,{report.AverageOrderAmount}");
        csv.AppendLine();

        // Supplier summaries
        csv.AppendLine("供應商統計");
        csv.AppendLine("供應商ID,供應商名稱,採購單數,採購金額,佔比(%)");
        foreach (var supplier in report.SupplierSummaries)
        {
            csv.AppendLine($"{supplier.SupplierId},{EscapeCsvField(supplier.SupplierName)},{supplier.OrderCount},{supplier.TotalAmount},{supplier.Percentage}");
        }

        _logger.LogInformation("匯出採購報表 CSV 成功 - 期間: {StartDate} ~ {EndDate}", startDate, endDate);
        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    /// <summary>
    /// 處理 CSV 欄位中的特殊字元
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    /// <inheritdoc />
    public async Task<ProfitReportDto> GetProfitReportAsync(DateOnly startDate, DateOnly endDate)
    {
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.OrderDate >= startDate &&
                         oi.Order.OrderDate <= endDate &&
                         oi.Order.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var totalRevenue = orderItems.Sum(oi => oi.Subtotal);
        var totalCost = orderItems.Sum(oi => oi.Product.CostPrice * oi.Quantity);
        var grossProfit = totalRevenue - totalCost;
        var grossProfitMargin = totalRevenue > 0 ? Math.Round(grossProfit / totalRevenue * 100, 2) : 0;

        var orderCount = orderItems.Select(oi => oi.OrderId).Distinct().Count();
        var avgOrderProfit = orderCount > 0 ? grossProfit / orderCount : 0;

        var categoryProfits = orderItems
            .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
            .Select(g => new CategoryProfitDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Revenue = g.Sum(oi => oi.Subtotal),
                Cost = g.Sum(oi => oi.Product.CostPrice * oi.Quantity),
                Profit = g.Sum(oi => oi.Subtotal) - g.Sum(oi => oi.Product.CostPrice * oi.Quantity),
                ProfitMargin = g.Sum(oi => oi.Subtotal) > 0
                    ? Math.Round((g.Sum(oi => oi.Subtotal) - g.Sum(oi => oi.Product.CostPrice * oi.Quantity)) / g.Sum(oi => oi.Subtotal) * 100, 2)
                    : 0
            })
            .OrderByDescending(c => c.Profit)
            .ToList();

        var dailyProfits = orderItems
            .GroupBy(oi => oi.Order.OrderDate)
            .Select(g => new DailyProfitDto
            {
                Date = g.Key,
                Revenue = g.Sum(oi => oi.Subtotal),
                Cost = g.Sum(oi => oi.Product.CostPrice * oi.Quantity),
                Profit = g.Sum(oi => oi.Subtotal) - g.Sum(oi => oi.Product.CostPrice * oi.Quantity)
            })
            .OrderBy(d => d.Date)
            .ToList();

        _logger.LogInformation("取得利潤報表成功 - 期間: {StartDate} ~ {EndDate}", startDate, endDate);

        return new ProfitReportDto
        {
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            GrossProfit = grossProfit,
            GrossProfitMargin = grossProfitMargin,
            TotalOrders = orderCount,
            AverageOrderProfit = avgOrderProfit,
            CategoryProfits = categoryProfits,
            DailyProfits = dailyProfits
        };
    }

    /// <inheritdoc />
    public async Task<ComparisonReportDto> GetComparisonReportAsync(DateOnly period1Start, DateOnly period1End, DateOnly period2Start, DateOnly period2End)
    {
        var period1Summary = await GetPeriodSummaryAsync(period1Start, period1End);
        var period2Summary = await GetPeriodSummaryAsync(period2Start, period2End);

        var salesGrowth = period1Summary.TotalSales > 0
            ? Math.Round((period2Summary.TotalSales - period1Summary.TotalSales) / period1Summary.TotalSales * 100, 2)
            : 0;
        var orderGrowth = period1Summary.TotalOrders > 0
            ? Math.Round((decimal)(period2Summary.TotalOrders - period1Summary.TotalOrders) / period1Summary.TotalOrders * 100, 2)
            : 0;
        var customerGrowth = period1Summary.TotalCustomers > 0
            ? Math.Round((decimal)(period2Summary.TotalCustomers - period1Summary.TotalCustomers) / period1Summary.TotalCustomers * 100, 2)
            : 0;
        var aovGrowth = period1Summary.AverageOrderValue > 0
            ? Math.Round((period2Summary.AverageOrderValue - period1Summary.AverageOrderValue) / period1Summary.AverageOrderValue * 100, 2)
            : 0;

        _logger.LogInformation("取得比較報表成功");

        return new ComparisonReportDto
        {
            Period1 = period1Summary,
            Period2 = period2Summary,
            SalesGrowth = salesGrowth,
            OrderGrowth = orderGrowth,
            CustomerGrowth = customerGrowth,
            AovGrowth = aovGrowth
        };
    }

    private async Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateOnly startDate, DateOnly endDate)
    {
        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var totalSales = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var totalCustomers = orders.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId).Distinct().Count();
        var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        return new PeriodSummaryDto
        {
            Period = $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}",
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            TotalCustomers = totalCustomers,
            AverageOrderValue = avgOrderValue
        };
    }
}
