using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.BackgroundServices;

/// <summary>
/// 排程報表處理背景服務
/// </summary>
/// <remarks>
/// 定期檢查並執行到期的排程報表
/// </remarks>
public class ScheduledReportProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledReportProcessorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public ScheduledReportProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledReportProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("排程報表處理服務啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueReportsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理排程報表時發生錯誤");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("排程報表處理服務停止");
    }

    private async Task ProcessDueReportsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customReportService = scope.ServiceProvider.GetRequiredService<ICustomReportService>();

        var now = DateTime.UtcNow;

        // 取得需要執行的排程報表
        var dueReports = await context.ScheduledReports
            .Include(s => s.CustomReport)
            .Where(s => s.IsActive && s.NextRunAt.HasValue && s.NextRunAt <= now)
            .ToListAsync(stoppingToken);

        if (!dueReports.Any())
        {
            return;
        }

        _logger.LogInformation("發現 {Count} 個需要執行的排程報表", dueReports.Count);

        foreach (var scheduledReport in dueReports)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ExecuteScheduledReportAsync(context, customReportService, scheduledReport, stoppingToken);
        }
    }

    private async Task ExecuteScheduledReportAsync(
        ApplicationDbContext context,
        ICustomReportService customReportService,
        ScheduledReport scheduledReport,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("開始執行排程報表 - Id: {Id}, Name: {Name}",
            scheduledReport.Id, scheduledReport.Name);

        var history = new ScheduledReportHistory
        {
            ScheduledReportId = scheduledReport.Id,
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };

        context.ScheduledReportHistories.Add(history);

        try
        {
            byte[]? reportData = null;
            int? recordCount = null;

            // 根據報表來源執行
            if (scheduledReport.CustomReportId.HasValue)
            {
                reportData = await customReportService.ExportCustomReportAsync(
                    scheduledReport.CustomReportId.Value,
                    scheduledReport.OutputFormat.ToLower(),
                    null);
            }
            else if (!string.IsNullOrEmpty(scheduledReport.ReportType))
            {
                reportData = await ExecuteBuiltInReportAsync(
                    context,
                    scheduledReport.ReportType,
                    scheduledReport.OutputFormat);
            }

            // 儲存報表檔案
            string? outputPath = null;
            if (reportData != null && reportData.Length > 0)
            {
                outputPath = await SaveReportFileAsync(scheduledReport, reportData);
                recordCount = reportData.Length; // 簡化記錄數量為檔案大小
            }

            // 發送報表（如果設定為 Email）
            if (scheduledReport.DeliveryMethod == "Email" && !string.IsNullOrEmpty(scheduledReport.RecipientEmails))
            {
                await SendReportEmailAsync(scheduledReport, reportData, outputPath);
            }

            history.Status = "Completed";
            history.CompletedAt = DateTime.UtcNow;
            history.OutputPath = outputPath;
            history.RecordCount = recordCount;

            scheduledReport.LastRunAt = DateTime.UtcNow;
            scheduledReport.LastRunStatus = "Completed";
            scheduledReport.LastRunError = null;

            _logger.LogInformation("排程報表執行成功 - Id: {Id}", scheduledReport.Id);
        }
        catch (Exception ex)
        {
            history.Status = "Failed";
            history.CompletedAt = DateTime.UtcNow;
            history.ErrorMessage = ex.Message;

            scheduledReport.LastRunAt = DateTime.UtcNow;
            scheduledReport.LastRunStatus = "Failed";
            scheduledReport.LastRunError = ex.Message;

            _logger.LogError(ex, "排程報表執行失敗 - Id: {Id}", scheduledReport.Id);
        }

        // 計算下次執行時間
        scheduledReport.NextRunAt = CalculateNextRunTime(scheduledReport);

        await context.SaveChangesAsync(stoppingToken);
    }

    private async Task<byte[]?> ExecuteBuiltInReportAsync(
        ApplicationDbContext context,
        string reportType,
        string outputFormat)
    {
        var sb = new System.Text.StringBuilder();

        switch (reportType.ToLower())
        {
            case "sales":
                var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var startDate = endDate.AddDays(-30);

                var orders = await context.Orders
                    .Include(o => o.Store)
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                sb.AppendLine("訂單編號,訂單日期,門市,金額");
                foreach (var order in orders)
                {
                    sb.AppendLine($"\"{order.OrderNo}\",\"{order.OrderDate:yyyy-MM-dd}\",\"{order.Store?.Name}\",{order.TotalAmount}");
                }
                break;

            case "inventory":
                var inventories = await context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .ToListAsync();

                sb.AppendLine("商品編號,商品名稱,倉庫,庫存量,可用量");
                foreach (var inv in inventories)
                {
                    sb.AppendLine($"\"{inv.Product?.Sku}\",\"{inv.Product?.Name}\",\"{inv.Warehouse?.Name}\",{inv.Quantity},{inv.Quantity - inv.ReservedQuantity}");
                }
                break;

            case "lowstock":
                var lowStockItems = await context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.Quantity < i.Product.SafetyStock && i.Product.IsActive)
                    .ToListAsync();

                sb.AppendLine("商品編號,商品名稱,倉庫,現有庫存,安全庫存,缺口");
                foreach (var item in lowStockItems)
                {
                    var shortage = item.Product.SafetyStock - item.Quantity;
                    sb.AppendLine($"\"{item.Product?.Sku}\",\"{item.Product?.Name}\",\"{item.Warehouse?.Name}\",{item.Quantity},{item.Product?.SafetyStock},{shortage}");
                }
                break;

            default:
                return null;
        }

        return System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();
    }

    private async Task<string?> SaveReportFileAsync(ScheduledReport scheduledReport, byte[] data)
    {
        var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "reports", "scheduled");
        Directory.CreateDirectory(reportsDir);

        var extension = scheduledReport.OutputFormat.ToLower() switch
        {
            "excel" => ".xlsx",
            "pdf" => ".pdf",
            _ => ".csv"
        };

        var fileName = $"{scheduledReport.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(reportsDir, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }

    private Task SendReportEmailAsync(ScheduledReport scheduledReport, byte[]? data, string? filePath)
    {
        // 這裡可以整合實際的 Email 發送服務
        // 目前僅記錄日誌
        _logger.LogInformation(
            "排程報表 Email 發送 - Id: {Id}, Recipients: {Recipients}, FilePath: {FilePath}",
            scheduledReport.Id,
            scheduledReport.RecipientEmails,
            filePath);

        return Task.CompletedTask;
    }

    private DateTime? CalculateNextRunTime(ScheduledReport report)
    {
        var now = DateTime.UtcNow;
        var runTime = TimeOnly.TryParse(report.ScheduleTime ?? "08:00", out var t)
            ? t
            : new TimeOnly(8, 0);

        return report.Schedule.ToLower() switch
        {
            "daily" => now.Date.AddDays(1).Add(runTime.ToTimeSpan()),
            "weekly" => GetNextWeekDay(now, report.ScheduleDayOfWeek ?? 1, runTime),
            "monthly" => GetNextMonthDay(now, report.ScheduleDayOfMonth ?? 1, runTime),
            "hourly" => now.AddHours(1),
            _ => now.Date.AddDays(1).Add(runTime.ToTimeSpan())
        };
    }

    private DateTime GetNextWeekDay(DateTime from, int dayOfWeek, TimeOnly time)
    {
        var daysUntil = ((dayOfWeek - (int)from.DayOfWeek + 7) % 7);
        if (daysUntil == 0) daysUntil = 7;
        return from.Date.AddDays(daysUntil).Add(time.ToTimeSpan());
    }

    private DateTime GetNextMonthDay(DateTime from, int dayOfMonth, TimeOnly time)
    {
        var next = new DateTime(from.Year, from.Month,
            Math.Min(dayOfMonth, DateTime.DaysInMonth(from.Year, from.Month)));
        if (next <= from)
        {
            next = next.AddMonths(1);
            next = new DateTime(next.Year, next.Month,
                Math.Min(dayOfMonth, DateTime.DaysInMonth(next.Year, next.Month)));
        }
        return next.Add(time.ToTimeSpan());
    }
}
