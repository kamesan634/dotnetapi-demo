namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 商品規格實體
/// </summary>
public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
