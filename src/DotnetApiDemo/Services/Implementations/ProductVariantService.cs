using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.ProductVariants;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class ProductVariantService : IProductVariantService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductVariantService> _logger;

    public ProductVariantService(ApplicationDbContext context, ILogger<ProductVariantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductVariantListDto>> GetVariantsAsync(PaginationRequest request)
    {
        var query = _context.ProductVariants.Include(v => v.Product).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(v => v.Sku.Contains(request.Search) ||
                                      v.VariantName!.Contains(request.Search) ||
                                      v.Product.Name.Contains(request.Search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(v => v.Product.Name).ThenBy(v => v.VariantName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new ProductVariantListDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                VariantName = v.VariantName,
                Color = v.Color,
                Size = v.Size,
                AdditionalPrice = v.AdditionalPrice,
                Stock = v.Stock,
                IsActive = v.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<ProductVariantListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<IEnumerable<ProductVariantListDto>> GetVariantsByProductAsync(int productId)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.VariantName)
            .Select(v => new ProductVariantListDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                VariantName = v.VariantName,
                Color = v.Color,
                Size = v.Size,
                AdditionalPrice = v.AdditionalPrice,
                Stock = v.Stock,
                IsActive = v.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductVariantDetailDto?> GetVariantByIdAsync(int id)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Id == id)
            .Select(v => new ProductVariantDetailDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                VariantName = v.VariantName,
                Color = v.Color,
                Size = v.Size,
                Material = v.Material,
                AdditionalPrice = v.AdditionalPrice,
                CostPrice = v.CostPrice,
                Stock = v.Stock,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateVariantAsync(CreateProductVariantRequest request)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
        {
            _logger.LogWarning("建立規格失敗：商品不存在 - ProductId: {ProductId}", request.ProductId);
            return null;
        }

        if (await _context.ProductVariants.AnyAsync(v => v.Sku == request.Sku))
        {
            _logger.LogWarning("建立規格失敗：SKU 已存在 - {Sku}", request.Sku);
            return null;
        }

        var variant = new ProductVariant
        {
            ProductId = request.ProductId,
            Sku = request.Sku,
            VariantName = request.VariantName,
            Color = request.Color,
            Size = request.Size,
            Material = request.Material,
            AdditionalPrice = request.AdditionalPrice,
            CostPrice = request.CostPrice,
            Stock = request.Stock,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        return variant.Id;
    }

    public async Task<bool> UpdateVariantAsync(int id, UpdateProductVariantRequest request)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            if (await _context.ProductVariants.AnyAsync(v => v.Sku == request.Sku && v.Id != id))
                return false;
            variant.Sku = request.Sku;
        }

        if (request.VariantName != null) variant.VariantName = request.VariantName;
        if (request.Color != null) variant.Color = request.Color;
        if (request.Size != null) variant.Size = request.Size;
        if (request.Material != null) variant.Material = request.Material;
        if (request.AdditionalPrice.HasValue) variant.AdditionalPrice = request.AdditionalPrice;
        if (request.CostPrice.HasValue) variant.CostPrice = request.CostPrice;
        if (request.Stock.HasValue) variant.Stock = request.Stock;
        if (request.IsActive.HasValue) variant.IsActive = request.IsActive.Value;

        variant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteVariantAsync(int id)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant == null) return false;

        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync();

        return true;
    }
}
