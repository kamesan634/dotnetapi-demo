namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 自訂報表實體
/// </summary>
public class CustomReport
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReportType { get; set; } = string.Empty; // Sales, Inventory, Customer, etc.
    public string? FilterJson { get; set; } // JSON for filter configuration
    public string? ColumnsJson { get; set; } // JSON for column selection
    public string? SortJson { get; set; } // JSON for sorting configuration
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性
    public virtual ApplicationUser CreatedBy { get; set; } = null!;
}

/// <summary>
/// 排程報表實體
/// </summary>
public class ScheduledReport
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CustomReportId { get; set; }
    public string? ReportType { get; set; } // Sales, Inventory, etc. (if not using CustomReport)
    public string? FilterJson { get; set; }
    public string Schedule { get; set; } = string.Empty; // Cron expression or simple: Daily, Weekly, Monthly
    public string? ScheduleTime { get; set; } // HH:mm format
    public int? ScheduleDayOfWeek { get; set; } // 0-6 for weekly
    public int? ScheduleDayOfMonth { get; set; } // 1-31 for monthly
    public string OutputFormat { get; set; } = "CSV"; // CSV, Excel, PDF
    public string DeliveryMethod { get; set; } = "Email"; // Email, Download
    public string? RecipientEmails { get; set; } // Comma-separated emails
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? LastRunStatus { get; set; }
    public string? LastRunError { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性
    public virtual CustomReport? CustomReport { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; } = null!;
}

/// <summary>
/// 排程報表執行記錄實體
/// </summary>
public class ScheduledReportHistory
{
    public int Id { get; set; }
    public int ScheduledReportId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Running"; // Running, Completed, Failed
    public string? ErrorMessage { get; set; }
    public string? OutputPath { get; set; }
    public int? RecordCount { get; set; }

    // 導航屬性
    public virtual ScheduledReport ScheduledReport { get; set; } = null!;
}
