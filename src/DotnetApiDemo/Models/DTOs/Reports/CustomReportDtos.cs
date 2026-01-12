using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Reports;

// Custom Report DTOs
public class CustomReportListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CustomReportDetailDto : CustomReportListDto
{
    public string? Description { get; set; }
    public string? FilterJson { get; set; }
    public string? ColumnsJson { get; set; }
    public string? SortJson { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCustomReportRequest
{
    [Required(ErrorMessage = "報表代碼為必填")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "報表名稱為必填")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "報表類型為必填")]
    public string ReportType { get; set; } = string.Empty;

    public string? FilterJson { get; set; }
    public string? ColumnsJson { get; set; }
    public string? SortJson { get; set; }
    public bool IsPublic { get; set; } = false;
}

public class UpdateCustomReportRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? FilterJson { get; set; }
    public string? ColumnsJson { get; set; }
    public string? SortJson { get; set; }
    public bool? IsPublic { get; set; }
    public bool? IsActive { get; set; }
}

public class RunCustomReportRequest
{
    public string? FilterOverrideJson { get; set; }
    public string OutputFormat { get; set; } = "JSON"; // JSON, CSV
}

public class CustomReportResultDto
{
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<string> Columns { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<Dictionary<string, object>> Data { get; set; } = Enumerable.Empty<Dictionary<string, object>>();
}

// Scheduled Report DTOs
public class ScheduledReportListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CustomReportName { get; set; }
    public string? ReportType { get; set; }
    public string Schedule { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? LastRunStatus { get; set; }
}

public class ScheduledReportDetailDto : ScheduledReportListDto
{
    public int? CustomReportId { get; set; }
    public string? FilterJson { get; set; }
    public string? ScheduleTime { get; set; }
    public int? ScheduleDayOfWeek { get; set; }
    public int? ScheduleDayOfMonth { get; set; }
    public string? RecipientEmails { get; set; }
    public string? LastRunError { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateScheduledReportRequest
{
    [Required(ErrorMessage = "排程名稱為必填")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? CustomReportId { get; set; }
    public string? ReportType { get; set; }
    public string? FilterJson { get; set; }

    [Required(ErrorMessage = "排程類型為必填")]
    public string Schedule { get; set; } = "Daily";

    public string? ScheduleTime { get; set; }
    public int? ScheduleDayOfWeek { get; set; }
    public int? ScheduleDayOfMonth { get; set; }

    [Required]
    public string OutputFormat { get; set; } = "CSV";

    [Required]
    public string DeliveryMethod { get; set; } = "Email";

    public string? RecipientEmails { get; set; }
}

public class UpdateScheduledReportRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    public int? CustomReportId { get; set; }
    public string? ReportType { get; set; }
    public string? FilterJson { get; set; }
    public string? Schedule { get; set; }
    public string? ScheduleTime { get; set; }
    public int? ScheduleDayOfWeek { get; set; }
    public int? ScheduleDayOfMonth { get; set; }
    public string? OutputFormat { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? RecipientEmails { get; set; }
    public bool? IsActive { get; set; }
}

public class ScheduledReportHistoryDto
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int? RecordCount { get; set; }
}
