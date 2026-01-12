using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;
using System.Text.Json;

namespace DotnetApiDemo.Services.Implementations;

public class CustomReportService : ICustomReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomReportService> _logger;

    public CustomReportService(ApplicationDbContext context, ILogger<CustomReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Custom Reports

    public async Task<PaginatedResponse<CustomReportListDto>> GetCustomReportsAsync(PaginationRequest request, int userId, bool includePublic = true)
    {
        var query = _context.CustomReports.AsQueryable();

        if (includePublic)
            query = query.Where(r => r.CreatedById == userId || r.IsPublic);
        else
            query = query.Where(r => r.CreatedById == userId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(r => r.Name.Contains(request.Search) || r.Code.Contains(request.Search));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new CustomReportListDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                ReportType = r.ReportType,
                IsPublic = r.IsPublic,
                IsActive = r.IsActive,
                CreatedByName = r.CreatedBy.RealName ?? r.CreatedBy.UserName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<CustomReportListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<CustomReportDetailDto?> GetCustomReportByIdAsync(int id)
    {
        return await _context.CustomReports
            .Where(r => r.Id == id)
            .Select(r => new CustomReportDetailDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                ReportType = r.ReportType,
                FilterJson = r.FilterJson,
                ColumnsJson = r.ColumnsJson,
                SortJson = r.SortJson,
                IsPublic = r.IsPublic,
                IsActive = r.IsActive,
                CreatedByName = r.CreatedBy.RealName ?? r.CreatedBy.UserName,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateCustomReportAsync(CreateCustomReportRequest request, int userId)
    {
        if (await _context.CustomReports.AnyAsync(r => r.Code == request.Code))
        {
            _logger.LogWarning("建立自訂報表失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var report = new CustomReport
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ReportType = request.ReportType,
            FilterJson = request.FilterJson,
            ColumnsJson = request.ColumnsJson,
            SortJson = request.SortJson,
            IsPublic = request.IsPublic,
            IsActive = true,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomReports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立自訂報表成功 - {Code}: {Name}", report.Code, report.Name);
        return report.Id;
    }

    public async Task<bool> UpdateCustomReportAsync(int id, UpdateCustomReportRequest request, int userId)
    {
        var report = await _context.CustomReports.FindAsync(id);
        if (report == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Name))
            report.Name = request.Name;

        if (request.Description != null)
            report.Description = request.Description;

        if (request.FilterJson != null)
            report.FilterJson = request.FilterJson;

        if (request.ColumnsJson != null)
            report.ColumnsJson = request.ColumnsJson;

        if (request.SortJson != null)
            report.SortJson = request.SortJson;

        if (request.IsPublic.HasValue)
            report.IsPublic = request.IsPublic.Value;

        if (request.IsActive.HasValue)
            report.IsActive = request.IsActive.Value;

        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("更新自訂報表成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteCustomReportAsync(int id, int userId)
    {
        var report = await _context.CustomReports.FindAsync(id);
        if (report == null) return false;

        _context.CustomReports.Remove(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除自訂報表成功 - Id: {Id}", id);
        return true;
    }

    public async Task<CustomReportResultDto?> RunCustomReportAsync(int id, RunCustomReportRequest? request = null)
    {
        var report = await _context.CustomReports.FindAsync(id);
        if (report == null) return null;

        var result = new CustomReportResultDto
        {
            ReportName = report.Name,
            GeneratedAt = DateTime.UtcNow,
            Data = new List<Dictionary<string, object>>()
        };

        // Execute report based on ReportType
        switch (report.ReportType.ToLower())
        {
            case "sales":
                result = await ExecuteSalesReportAsync(report, request);
                break;
            case "inventory":
                result = await ExecuteInventoryReportAsync(report, request);
                break;
            case "customer":
                result = await ExecuteCustomerReportAsync(report, request);
                break;
            case "product":
                result = await ExecuteProductReportAsync(report, request);
                break;
            default:
                _logger.LogWarning("不支援的報表類型: {ReportType}", report.ReportType);
                return result;
        }

        result.ReportName = report.Name;
        result.GeneratedAt = DateTime.UtcNow;
        return result;
    }

    public async Task<byte[]?> ExportCustomReportAsync(int id, string format, RunCustomReportRequest? request = null)
    {
        var result = await RunCustomReportAsync(id, request);
        if (result == null) return null;

        if (format.ToLower() == "csv")
        {
            return GenerateCsvFromResult(result);
        }

        // Default to JSON
        return System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
    }

    private async Task<CustomReportResultDto> ExecuteSalesReportAsync(CustomReport report, RunCustomReportRequest? request)
    {
        var orders = await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Customer)
            .Where(o => o.Status == Models.Enums.OrderStatus.Completed)
            .OrderByDescending(o => o.OrderDate)
            .Take(1000)
            .ToListAsync();

        var data = orders.Select(o => new Dictionary<string, object>
        {
            ["訂單編號"] = o.OrderNo,
            ["訂單日期"] = o.OrderDate.ToString("yyyy-MM-dd"),
            ["門市"] = o.Store?.Name ?? "",
            ["客戶"] = o.Customer?.Name ?? "散客",
            ["金額"] = o.TotalAmount,
            ["折扣"] = o.DiscountAmount,
            ["實收"] = o.TotalAmount - o.DiscountAmount
        }).ToList();

        return new CustomReportResultDto
        {
            Columns = new[] { "訂單編號", "訂單日期", "門市", "客戶", "金額", "折扣", "實收" },
            Data = data,
            TotalRecords = data.Count
        };
    }

    private async Task<CustomReportResultDto> ExecuteInventoryReportAsync(CustomReport report, RunCustomReportRequest? request)
    {
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .ToListAsync();

        var data = inventories.Select(i => new Dictionary<string, object>
        {
            ["商品編號"] = i.Product?.Sku ?? "",
            ["商品名稱"] = i.Product?.Name ?? "",
            ["倉庫"] = i.Warehouse?.Name ?? "",
            ["庫存量"] = i.Quantity,
            ["保留量"] = i.ReservedQuantity,
            ["可用量"] = i.Quantity - i.ReservedQuantity
        }).ToList();

        return new CustomReportResultDto
        {
            Columns = new[] { "商品編號", "商品名稱", "倉庫", "庫存量", "保留量", "可用量" },
            Data = data,
            TotalRecords = data.Count
        };
    }

    private async Task<CustomReportResultDto> ExecuteCustomerReportAsync(CustomReport report, RunCustomReportRequest? request)
    {
        var customers = await _context.Customers.ToListAsync();

        var data = customers.Select(c => new Dictionary<string, object>
        {
            ["會員編號"] = c.MemberNo,
            ["姓名"] = c.Name,
            ["電話"] = c.Phone ?? "",
            ["Email"] = c.Email ?? "",
            ["累計消費"] = c.TotalSpent,
            ["目前點數"] = c.CurrentPoints
        }).ToList();

        return new CustomReportResultDto
        {
            Columns = new[] { "會員編號", "姓名", "電話", "Email", "累計消費", "目前點數" },
            Data = data,
            TotalRecords = data.Count
        };
    }

    private async Task<CustomReportResultDto> ExecuteProductReportAsync(CustomReport report, RunCustomReportRequest? request)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .ToListAsync();

        var data = products.Select(p => new Dictionary<string, object>
        {
            ["商品編號"] = p.Sku,
            ["商品名稱"] = p.Name,
            ["分類"] = p.Category?.Name ?? "",
            ["單位"] = p.Unit?.Name ?? "",
            ["售價"] = p.SellingPrice,
            ["成本"] = p.CostPrice,
            ["狀態"] = p.IsActive ? "啟用" : "停用"
        }).ToList();

        return new CustomReportResultDto
        {
            Columns = new[] { "商品編號", "商品名稱", "分類", "單位", "售價", "成本", "狀態" },
            Data = data,
            TotalRecords = data.Count
        };
    }

    private byte[] GenerateCsvFromResult(CustomReportResultDto result)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", result.Columns.Select(c => $"\"{c}\"")));

        foreach (var row in result.Data)
        {
            var values = result.Columns.Select(col =>
            {
                row.TryGetValue(col, out var val);
                return $"\"{val}\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    #endregion

    #region Scheduled Reports

    public async Task<PaginatedResponse<ScheduledReportListDto>> GetScheduledReportsAsync(PaginationRequest request, int userId)
    {
        var query = _context.ScheduledReports
            .Include(s => s.CustomReport)
            .Where(s => s.CreatedById == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ScheduledReportListDto
            {
                Id = s.Id,
                Name = s.Name,
                CustomReportName = s.CustomReport != null ? s.CustomReport.Name : null,
                ReportType = s.ReportType,
                Schedule = s.Schedule,
                OutputFormat = s.OutputFormat,
                DeliveryMethod = s.DeliveryMethod,
                IsActive = s.IsActive,
                LastRunAt = s.LastRunAt,
                NextRunAt = s.NextRunAt,
                LastRunStatus = s.LastRunStatus
            })
            .ToListAsync();

        return new PaginatedResponse<ScheduledReportListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ScheduledReportDetailDto?> GetScheduledReportByIdAsync(int id)
    {
        return await _context.ScheduledReports
            .Where(s => s.Id == id)
            .Select(s => new ScheduledReportDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                CustomReportId = s.CustomReportId,
                CustomReportName = s.CustomReport != null ? s.CustomReport.Name : null,
                ReportType = s.ReportType,
                FilterJson = s.FilterJson,
                Schedule = s.Schedule,
                ScheduleTime = s.ScheduleTime,
                ScheduleDayOfWeek = s.ScheduleDayOfWeek,
                ScheduleDayOfMonth = s.ScheduleDayOfMonth,
                OutputFormat = s.OutputFormat,
                DeliveryMethod = s.DeliveryMethod,
                RecipientEmails = s.RecipientEmails,
                IsActive = s.IsActive,
                LastRunAt = s.LastRunAt,
                NextRunAt = s.NextRunAt,
                LastRunStatus = s.LastRunStatus,
                LastRunError = s.LastRunError,
                CreatedByName = s.CreatedBy.RealName ?? s.CreatedBy.UserName,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateScheduledReportAsync(CreateScheduledReportRequest request, int userId)
    {
        var scheduled = new ScheduledReport
        {
            Name = request.Name,
            CustomReportId = request.CustomReportId,
            ReportType = request.ReportType,
            FilterJson = request.FilterJson,
            Schedule = request.Schedule,
            ScheduleTime = request.ScheduleTime,
            ScheduleDayOfWeek = request.ScheduleDayOfWeek,
            ScheduleDayOfMonth = request.ScheduleDayOfMonth,
            OutputFormat = request.OutputFormat,
            DeliveryMethod = request.DeliveryMethod,
            RecipientEmails = request.RecipientEmails,
            IsActive = true,
            NextRunAt = CalculateNextRunTime(request.Schedule, request.ScheduleTime, request.ScheduleDayOfWeek, request.ScheduleDayOfMonth),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ScheduledReports.Add(scheduled);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立排程報表成功 - {Name}", scheduled.Name);
        return scheduled.Id;
    }

    public async Task<bool> UpdateScheduledReportAsync(int id, UpdateScheduledReportRequest request, int userId)
    {
        var scheduled = await _context.ScheduledReports.FindAsync(id);
        if (scheduled == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Name))
            scheduled.Name = request.Name;

        if (request.CustomReportId.HasValue)
            scheduled.CustomReportId = request.CustomReportId;

        if (request.ReportType != null)
            scheduled.ReportType = request.ReportType;

        if (request.FilterJson != null)
            scheduled.FilterJson = request.FilterJson;

        if (!string.IsNullOrWhiteSpace(request.Schedule))
        {
            scheduled.Schedule = request.Schedule;
            scheduled.ScheduleTime = request.ScheduleTime;
            scheduled.ScheduleDayOfWeek = request.ScheduleDayOfWeek;
            scheduled.ScheduleDayOfMonth = request.ScheduleDayOfMonth;
            scheduled.NextRunAt = CalculateNextRunTime(request.Schedule, request.ScheduleTime, request.ScheduleDayOfWeek, request.ScheduleDayOfMonth);
        }

        if (!string.IsNullOrWhiteSpace(request.OutputFormat))
            scheduled.OutputFormat = request.OutputFormat;

        if (!string.IsNullOrWhiteSpace(request.DeliveryMethod))
            scheduled.DeliveryMethod = request.DeliveryMethod;

        if (request.RecipientEmails != null)
            scheduled.RecipientEmails = request.RecipientEmails;

        if (request.IsActive.HasValue)
            scheduled.IsActive = request.IsActive.Value;

        scheduled.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("更新排程報表成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteScheduledReportAsync(int id, int userId)
    {
        var scheduled = await _context.ScheduledReports.FindAsync(id);
        if (scheduled == null) return false;

        _context.ScheduledReports.Remove(scheduled);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除排程報表成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> RunScheduledReportNowAsync(int id)
    {
        var scheduled = await _context.ScheduledReports.FindAsync(id);
        if (scheduled == null) return false;

        var history = new ScheduledReportHistory
        {
            ScheduledReportId = id,
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };

        _context.ScheduledReportHistories.Add(history);

        try
        {
            // Execute report
            if (scheduled.CustomReportId.HasValue)
            {
                await RunCustomReportAsync(scheduled.CustomReportId.Value, null);
            }

            history.Status = "Completed";
            history.CompletedAt = DateTime.UtcNow;
            scheduled.LastRunAt = DateTime.UtcNow;
            scheduled.LastRunStatus = "Completed";
            scheduled.NextRunAt = CalculateNextRunTime(scheduled.Schedule, scheduled.ScheduleTime, scheduled.ScheduleDayOfWeek, scheduled.ScheduleDayOfMonth);
        }
        catch (Exception ex)
        {
            history.Status = "Failed";
            history.ErrorMessage = ex.Message;
            history.CompletedAt = DateTime.UtcNow;
            scheduled.LastRunStatus = "Failed";
            scheduled.LastRunError = ex.Message;
        }

        await _context.SaveChangesAsync();
        return history.Status == "Completed";
    }

    public async Task<IEnumerable<ScheduledReportHistoryDto>> GetScheduledReportHistoryAsync(int id, int limit = 10)
    {
        return await _context.ScheduledReportHistories
            .Where(h => h.ScheduledReportId == id)
            .OrderByDescending(h => h.StartedAt)
            .Take(limit)
            .Select(h => new ScheduledReportHistoryDto
            {
                Id = h.Id,
                StartedAt = h.StartedAt,
                CompletedAt = h.CompletedAt,
                Status = h.Status,
                ErrorMessage = h.ErrorMessage,
                RecordCount = h.RecordCount
            })
            .ToListAsync();
    }

    private DateTime? CalculateNextRunTime(string schedule, string? time, int? dayOfWeek, int? dayOfMonth)
    {
        var now = DateTime.UtcNow;
        var runTime = TimeOnly.TryParse(time ?? "08:00", out var t) ? t : new TimeOnly(8, 0);

        return schedule.ToLower() switch
        {
            "daily" => now.Date.AddDays(1).Add(runTime.ToTimeSpan()),
            "weekly" => GetNextWeekDay(now, dayOfWeek ?? 1, runTime),
            "monthly" => GetNextMonthDay(now, dayOfMonth ?? 1, runTime),
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
        var next = new DateTime(from.Year, from.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(from.Year, from.Month)));
        if (next <= from)
        {
            next = next.AddMonths(1);
            next = new DateTime(next.Year, next.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(next.Year, next.Month)));
        }
        return next.Add(time.ToTimeSpan());
    }

    #endregion
}
