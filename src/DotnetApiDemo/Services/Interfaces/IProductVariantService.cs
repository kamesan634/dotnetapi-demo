using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.ProductVariants;

namespace DotnetApiDemo.Services.Interfaces;

public interface IProductVariantService
{
    Task<PaginatedResponse<ProductVariantListDto>> GetVariantsAsync(PaginationRequest request);
    Task<IEnumerable<ProductVariantListDto>> GetVariantsByProductAsync(int productId);
    Task<ProductVariantDetailDto?> GetVariantByIdAsync(int id);
    Task<int?> CreateVariantAsync(CreateProductVariantRequest request);
    Task<bool> UpdateVariantAsync(int id, UpdateProductVariantRequest request);
    Task<bool> DeleteVariantAsync(int id);
}
