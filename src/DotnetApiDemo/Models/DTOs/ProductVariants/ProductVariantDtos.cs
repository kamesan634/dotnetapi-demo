using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.ProductVariants;

public class ProductVariantListDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public int? Stock { get; set; }
    public bool IsActive { get; set; }
}

public class ProductVariantDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductVariantRequest
{
    [Required] public int ProductId { get; set; }
    [Required] public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? Stock { get; set; }
}

public class UpdateProductVariantRequest
{
    public string? Sku { get; set; }
    public string? VariantName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? Stock { get; set; }
    public bool? IsActive { get; set; }
}
