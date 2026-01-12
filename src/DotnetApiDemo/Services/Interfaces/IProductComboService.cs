using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;

namespace DotnetApiDemo.Services.Interfaces;

public interface IProductComboService
{
    Task<PaginatedResponse<ProductComboListDto>> GetCombosAsync(PaginationRequest request, bool? activeOnly = null);
    Task<ProductComboDetailDto?> GetComboByIdAsync(int id);
    Task<ProductComboDetailDto?> GetComboByCodeAsync(string code);
    Task<int?> CreateComboAsync(CreateProductComboRequest request);
    Task<bool> UpdateComboAsync(int id, UpdateProductComboRequest request);
    Task<bool> DeleteComboAsync(int id);
    Task<bool> AddComboItemAsync(int comboId, CreateProductComboItemRequest request);
    Task<bool> RemoveComboItemAsync(int comboId, int productId);
    Task<IEnumerable<ProductComboListDto>> GetActiveCombosByProductAsync(int productId);
}
