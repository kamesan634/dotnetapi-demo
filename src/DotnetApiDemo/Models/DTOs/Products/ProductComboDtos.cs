using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.Products;

public class ProductComboListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal SaveAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ItemCount { get; set; }
    public int SoldQuantity { get; set; }
}

public class ProductComboDetailDto : ProductComboListDto
{
    public string? Description { get; set; }
    public int? MaxQuantity { get; set; }
    public IEnumerable<ProductComboItemDto> Items { get; set; } = Enumerable.Empty<ProductComboItemDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductComboItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public int SortOrder { get; set; }
}

public class CreateProductComboRequest
{
    [Required(ErrorMessage = "組合代碼為必填")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "組合名稱為必填")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "售價為必填")]
    [Range(0, double.MaxValue)]
    public decimal SellingPrice { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxQuantity { get; set; }

    [Required(ErrorMessage = "組合項目為必填")]
    [MinLength(2, ErrorMessage = "至少需要兩個組合項目")]
    public IEnumerable<CreateProductComboItemRequest> Items { get; set; } = Enumerable.Empty<CreateProductComboItemRequest>();
}

public class CreateProductComboItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public int SortOrder { get; set; } = 0;
}

public class UpdateProductComboRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SellingPrice { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxQuantity { get; set; }

    public bool? IsActive { get; set; }
}
