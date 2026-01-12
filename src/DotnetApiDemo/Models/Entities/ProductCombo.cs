namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 商品組合實體
/// </summary>
public class ProductCombo
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal SaveAmount => OriginalPrice - SellingPrice;
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxQuantity { get; set; }
    public int SoldQuantity { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // 導航屬性
    public virtual ICollection<ProductComboItem> Items { get; set; } = new List<ProductComboItem>();
}

/// <summary>
/// 商品組合項目實體
/// </summary>
public class ProductComboItem
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public int SortOrder { get; set; } = 0;

    // 導航屬性
    public virtual ProductCombo Combo { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
