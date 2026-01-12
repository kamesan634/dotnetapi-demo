using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class ProductComboService : IProductComboService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductComboService> _logger;

    public ProductComboService(ApplicationDbContext context, ILogger<ProductComboService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductComboListDto>> GetCombosAsync(PaginationRequest request, bool? activeOnly = null)
    {
        var query = _context.ProductCombos
            .Include(c => c.Items)
            .AsQueryable();

        if (activeOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c => c.IsActive &&
                (c.StartDate == null || c.StartDate <= now) &&
                (c.EndDate == null || c.EndDate >= now));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Name.Contains(request.Search) || c.Code.Contains(request.Search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ProductComboListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                OriginalPrice = c.OriginalPrice,
                SellingPrice = c.SellingPrice,
                SaveAmount = c.OriginalPrice - c.SellingPrice,
                IsActive = c.IsActive,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ItemCount = c.Items.Count,
                SoldQuantity = c.SoldQuantity
            })
            .ToListAsync();

        return new PaginatedResponse<ProductComboListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductComboDetailDto?> GetComboByIdAsync(int id)
    {
        return await GetComboDetailAsync(c => c.Id == id);
    }

    public async Task<ProductComboDetailDto?> GetComboByCodeAsync(string code)
    {
        return await GetComboDetailAsync(c => c.Code == code);
    }

    private async Task<ProductComboDetailDto?> GetComboDetailAsync(System.Linq.Expressions.Expression<Func<ProductCombo, bool>> predicate)
    {
        return await _context.ProductCombos
            .Where(predicate)
            .Select(c => new ProductComboDetailDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                OriginalPrice = c.OriginalPrice,
                SellingPrice = c.SellingPrice,
                SaveAmount = c.OriginalPrice - c.SellingPrice,
                IsActive = c.IsActive,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MaxQuantity = c.MaxQuantity,
                SoldQuantity = c.SoldQuantity,
                ItemCount = c.Items.Count,
                Items = c.Items.OrderBy(i => i.SortOrder).Select(i => new ProductComboItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    ProductPrice = i.Product.SellingPrice,
                    Quantity = i.Quantity,
                    SortOrder = i.SortOrder
                }),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateComboAsync(CreateProductComboRequest request)
    {
        if (await _context.ProductCombos.AnyAsync(c => c.Code == request.Code))
        {
            _logger.LogWarning("建立商品組合失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.SellingPrice);

        if (products.Count != productIds.Distinct().Count())
        {
            _logger.LogWarning("建立商品組合失敗：部分商品不存在");
            return null;
        }

        decimal originalPrice = 0;
        foreach (var item in request.Items)
        {
            if (products.TryGetValue(item.ProductId, out var price))
            {
                originalPrice += price * item.Quantity;
            }
        }

        var combo = new ProductCombo
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            OriginalPrice = originalPrice,
            SellingPrice = request.SellingPrice,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MaxQuantity = request.MaxQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            combo.Items.Add(new ProductComboItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                SortOrder = item.SortOrder
            });
        }

        _context.ProductCombos.Add(combo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立商品組合成功 - {Code}: {Name}", combo.Code, combo.Name);
        return combo.Id;
    }

    public async Task<bool> UpdateComboAsync(int id, UpdateProductComboRequest request)
    {
        var combo = await _context.ProductCombos.FindAsync(id);
        if (combo == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Name))
            combo.Name = request.Name;

        if (request.Description != null)
            combo.Description = request.Description;

        if (request.SellingPrice.HasValue)
            combo.SellingPrice = request.SellingPrice.Value;

        if (request.StartDate.HasValue)
            combo.StartDate = request.StartDate;

        if (request.EndDate.HasValue)
            combo.EndDate = request.EndDate;

        if (request.MaxQuantity.HasValue)
            combo.MaxQuantity = request.MaxQuantity;

        if (request.IsActive.HasValue)
            combo.IsActive = request.IsActive.Value;

        combo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("更新商品組合成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteComboAsync(int id)
    {
        var combo = await _context.ProductCombos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (combo == null) return false;

        _context.ProductComboItems.RemoveRange(combo.Items);
        _context.ProductCombos.Remove(combo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除商品組合成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> AddComboItemAsync(int comboId, CreateProductComboItemRequest request)
    {
        var combo = await _context.ProductCombos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == comboId);

        if (combo == null) return false;

        if (combo.Items.Any(i => i.ProductId == request.ProductId))
        {
            _logger.LogWarning("新增組合項目失敗：商品已存在於組合中");
            return false;
        }

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return false;

        combo.Items.Add(new ProductComboItem
        {
            ComboId = comboId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            SortOrder = request.SortOrder
        });

        combo.OriginalPrice += product.SellingPrice * request.Quantity;
        combo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveComboItemAsync(int comboId, int productId)
    {
        var combo = await _context.ProductCombos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == comboId);

        if (combo == null) return false;

        var item = combo.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;

        if (combo.Items.Count <= 2)
        {
            _logger.LogWarning("移除組合項目失敗：組合至少需要兩個項目");
            return false;
        }

        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            combo.OriginalPrice -= product.SellingPrice * item.Quantity;
        }

        combo.Items.Remove(item);
        combo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductComboListDto>> GetActiveCombosByProductAsync(int productId)
    {
        var now = DateTime.UtcNow;
        return await _context.ProductCombos
            .Where(c => c.IsActive &&
                (c.StartDate == null || c.StartDate <= now) &&
                (c.EndDate == null || c.EndDate >= now) &&
                c.Items.Any(i => i.ProductId == productId))
            .Select(c => new ProductComboListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                OriginalPrice = c.OriginalPrice,
                SellingPrice = c.SellingPrice,
                SaveAmount = c.OriginalPrice - c.SellingPrice,
                IsActive = c.IsActive,
                ItemCount = c.Items.Count
            })
            .ToListAsync();
    }
}
