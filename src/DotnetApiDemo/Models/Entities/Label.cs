namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 標籤模板實體
/// </summary>
public class LabelTemplate
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Price"; // Price, Barcode, Shelf
    public int Width { get; set; } // mm
    public int Height { get; set; } // mm
    public string? LayoutJson { get; set; } // JSON layout configuration
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 列印任務實體
/// </summary>
public class PrintJob
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public int TotalLabels { get; set; }
    public int PrintedLabels { get; set; } = 0;
    public string? PrinterName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedById { get; set; }

    // 導航屬性
    public virtual LabelTemplate Template { get; set; } = null!;
    public virtual ApplicationUser CreatedBy { get; set; } = null!;
    public virtual ICollection<PrintJobItem> Items { get; set; } = new List<PrintJobItem>();
}

/// <summary>
/// 列印任務項目實體
/// </summary>
public class PrintJobItem
{
    public int Id { get; set; }
    public int PrintJobId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? CustomText { get; set; }

    // 導航屬性
    public virtual PrintJob PrintJob { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
