using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Labels;

public class LabelTemplateListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}

public class LabelTemplateDetailDto : LabelTemplateListDto
{
    public string? LayoutJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLabelTemplateRequest
{
    [Required(ErrorMessage = "模板代碼為必填")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "模板名稱為必填")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "Price";

    [Required]
    [Range(10, 500)]
    public int Width { get; set; }

    [Required]
    [Range(10, 500)]
    public int Height { get; set; }

    public string? LayoutJson { get; set; }
    public bool IsDefault { get; set; } = false;
}

public class UpdateLabelTemplateRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    public string? Type { get; set; }
    [Range(10, 500)]
    public int? Width { get; set; }
    [Range(10, 500)]
    public int? Height { get; set; }
    public string? LayoutJson { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
}

public class PrintJobListDto
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalLabels { get; set; }
    public int PrintedLabels { get; set; }
    public string? PrinterName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class PrintJobDetailDto : PrintJobListDto
{
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public IEnumerable<PrintJobItemDto> Items { get; set; } = Enumerable.Empty<PrintJobItemDto>();
}

public class PrintJobItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? Barcode { get; set; }
    public int Quantity { get; set; }
    public string? CustomText { get; set; }
}

public class CreatePrintJobRequest
{
    [Required]
    public int TemplateId { get; set; }

    public string? PrinterName { get; set; }

    [Required(ErrorMessage = "列印項目為必填")]
    [MinLength(1)]
    public IEnumerable<CreatePrintJobItemRequest> Items { get; set; } = Enumerable.Empty<CreatePrintJobItemRequest>();
}

public class CreatePrintJobItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;

    public string? CustomText { get; set; }
}

public class LabelPreviewDto
{
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Barcode { get; set; }
    public string? CategoryName { get; set; }
    public string? UnitName { get; set; }
    public string BarcodeImageBase64 { get; set; } = string.Empty;
}

public class BatchPrintRequest
{
    [Required]
    public int TemplateId { get; set; }

    public string? PrinterName { get; set; }

    [Required]
    public IEnumerable<int> ProductIds { get; set; } = Enumerable.Empty<int>();

    [Range(1, 100)]
    public int QuantityPerProduct { get; set; } = 1;
}
