using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 供應商報價服務實作
/// </summary>
public class SupplierPriceService : ISupplierPriceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SupplierPriceService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SupplierPriceService(ApplicationDbContext context, ILogger<SupplierPriceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<SupplierPriceListDto>> GetSupplierPricesAsync(
        PaginationRequest request,
        int? supplierId = null,
        int? productId = null)
    {
        var query = _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Product)
            .AsQueryable();

        // 依供應商篩選
        if (supplierId.HasValue)
        {
            query = query.Where(sp => sp.SupplierId == supplierId.Value);
        }

        // 依商品篩選
        if (productId.HasValue)
        {
            query = query.Where(sp => sp.ProductId == productId.Value);
        }

        // 搜尋關鍵字
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(sp =>
                sp.Supplier.Name.Contains(request.Search) ||
                sp.Product.Name.Contains(request.Search) ||
                sp.Product.Sku.Contains(request.Search) ||
                (sp.SupplierSku != null && sp.SupplierSku.Contains(request.Search)));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "suppliername" => request.IsDescending
                ? query.OrderByDescending(sp => sp.Supplier.Name)
                : query.OrderBy(sp => sp.Supplier.Name),
            "productname" => request.IsDescending
                ? query.OrderByDescending(sp => sp.Product.Name)
                : query.OrderBy(sp => sp.Product.Name),
            "unitprice" => request.IsDescending
                ? query.OrderByDescending(sp => sp.UnitPrice)
                : query.OrderBy(sp => sp.UnitPrice),
            "effectivedate" => request.IsDescending
                ? query.OrderByDescending(sp => sp.EffectiveDate)
                : query.OrderBy(sp => sp.EffectiveDate),
            _ => query.OrderByDescending(sp => sp.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sp => new SupplierPriceListDto
            {
                Id = sp.Id,
                SupplierId = sp.SupplierId,
                SupplierName = sp.Supplier.Name,
                ProductId = sp.ProductId,
                ProductName = sp.Product.Name,
                ProductSku = sp.Product.Sku,
                SupplierSku = sp.SupplierSku,
                UnitPrice = sp.UnitPrice,
                Currency = sp.Currency,
                MinOrderQuantity = sp.MinOrderQuantity,
                EffectiveDate = sp.EffectiveDate,
                ExpiryDate = sp.ExpiryDate,
                IsPrimary = sp.IsPrimary
            })
            .ToListAsync();

        return new PaginatedResponse<SupplierPriceListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<SupplierPriceDetailDto?> GetSupplierPriceByIdAsync(int id)
    {
        var supplierPrice = await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Product)
            .FirstOrDefaultAsync(sp => sp.Id == id);

        if (supplierPrice == null)
            return null;

        return new SupplierPriceDetailDto
        {
            Id = supplierPrice.Id,
            SupplierId = supplierPrice.SupplierId,
            SupplierCode = supplierPrice.Supplier.Code,
            SupplierName = supplierPrice.Supplier.Name,
            ProductId = supplierPrice.ProductId,
            ProductSku = supplierPrice.Product.Sku,
            ProductName = supplierPrice.Product.Name,
            SupplierSku = supplierPrice.SupplierSku,
            UnitPrice = supplierPrice.UnitPrice,
            Currency = supplierPrice.Currency,
            MinOrderQuantity = supplierPrice.MinOrderQuantity,
            PackSize = supplierPrice.PackSize,
            LeadTimeDays = supplierPrice.LeadTimeDays,
            EffectiveDate = supplierPrice.EffectiveDate,
            ExpiryDate = supplierPrice.ExpiryDate,
            IsPrimary = supplierPrice.IsPrimary,
            Notes = supplierPrice.Notes,
            CreatedAt = supplierPrice.CreatedAt,
            UpdatedAt = supplierPrice.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplierPriceListDto>> GetPricesByProductAsync(int productId)
    {
        return await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Product)
            .Where(sp => sp.ProductId == productId)
            .OrderByDescending(sp => sp.IsPrimary)
            .ThenBy(sp => sp.UnitPrice)
            .Select(sp => new SupplierPriceListDto
            {
                Id = sp.Id,
                SupplierId = sp.SupplierId,
                SupplierName = sp.Supplier.Name,
                ProductId = sp.ProductId,
                ProductName = sp.Product.Name,
                ProductSku = sp.Product.Sku,
                SupplierSku = sp.SupplierSku,
                UnitPrice = sp.UnitPrice,
                Currency = sp.Currency,
                MinOrderQuantity = sp.MinOrderQuantity,
                EffectiveDate = sp.EffectiveDate,
                ExpiryDate = sp.ExpiryDate,
                IsPrimary = sp.IsPrimary
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplierPriceListDto>> GetPricesBySupplierAsync(int supplierId)
    {
        return await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Product)
            .Where(sp => sp.SupplierId == supplierId)
            .OrderBy(sp => sp.Product.Name)
            .Select(sp => new SupplierPriceListDto
            {
                Id = sp.Id,
                SupplierId = sp.SupplierId,
                SupplierName = sp.Supplier.Name,
                ProductId = sp.ProductId,
                ProductName = sp.Product.Name,
                ProductSku = sp.Product.Sku,
                SupplierSku = sp.SupplierSku,
                UnitPrice = sp.UnitPrice,
                Currency = sp.Currency,
                MinOrderQuantity = sp.MinOrderQuantity,
                EffectiveDate = sp.EffectiveDate,
                ExpiryDate = sp.ExpiryDate,
                IsPrimary = sp.IsPrimary
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateSupplierPriceAsync(CreateSupplierPriceRequest request)
    {
        // 驗證供應商是否存在
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
        if (!supplierExists)
        {
            _logger.LogWarning("建立供應商報價失敗：供應商不存在 - SupplierId: {SupplierId}", request.SupplierId);
            return null;
        }

        // 驗證商品是否存在
        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
        if (!productExists)
        {
            _logger.LogWarning("建立供應商報價失敗：商品不存在 - ProductId: {ProductId}", request.ProductId);
            return null;
        }

        // 檢查是否已存在相同的供應商商品組合且日期重疊
        var existingPrice = await _context.SupplierPrices
            .AnyAsync(sp => sp.SupplierId == request.SupplierId &&
                           sp.ProductId == request.ProductId &&
                           sp.EffectiveDate == request.EffectiveDate);

        if (existingPrice)
        {
            _logger.LogWarning("建立供應商報價失敗：相同生效日期的報價已存在 - SupplierId: {SupplierId}, ProductId: {ProductId}, EffectiveDate: {EffectiveDate}",
                request.SupplierId, request.ProductId, request.EffectiveDate);
            return null;
        }

        // 如果設為主要供應商，取消其他主要供應商標記
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.SupplierPrices
                .Where(sp => sp.ProductId == request.ProductId && sp.IsPrimary)
                .ToListAsync();

            foreach (var primary in existingPrimary)
            {
                primary.IsPrimary = false;
                primary.UpdatedAt = DateTime.UtcNow;
            }
        }

        var supplierPrice = new SupplierPrice
        {
            SupplierId = request.SupplierId,
            ProductId = request.ProductId,
            SupplierSku = request.SupplierSku,
            UnitPrice = request.UnitPrice,
            Currency = request.Currency,
            MinOrderQuantity = request.MinOrderQuantity,
            PackSize = request.PackSize,
            LeadTimeDays = request.LeadTimeDays,
            EffectiveDate = request.EffectiveDate,
            ExpiryDate = request.ExpiryDate,
            IsPrimary = request.IsPrimary,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.SupplierPrices.Add(supplierPrice);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立供應商報價成功 - Id: {Id}, SupplierId: {SupplierId}, ProductId: {ProductId}",
            supplierPrice.Id, request.SupplierId, request.ProductId);

        return supplierPrice.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateSupplierPriceAsync(int id, UpdateSupplierPriceRequest request)
    {
        var supplierPrice = await _context.SupplierPrices.FindAsync(id);
        if (supplierPrice == null)
        {
            return false;
        }

        // 如果要設為主要供應商，取消其他主要供應商標記
        if (request.IsPrimary == true && !supplierPrice.IsPrimary)
        {
            var existingPrimary = await _context.SupplierPrices
                .Where(sp => sp.ProductId == supplierPrice.ProductId && sp.IsPrimary && sp.Id != id)
                .ToListAsync();

            foreach (var primary in existingPrimary)
            {
                primary.IsPrimary = false;
                primary.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (request.SupplierSku != null)
            supplierPrice.SupplierSku = request.SupplierSku;

        if (request.UnitPrice.HasValue)
            supplierPrice.UnitPrice = request.UnitPrice.Value;

        if (request.Currency != null)
            supplierPrice.Currency = request.Currency;

        if (request.MinOrderQuantity.HasValue)
            supplierPrice.MinOrderQuantity = request.MinOrderQuantity.Value;

        if (request.PackSize.HasValue)
            supplierPrice.PackSize = request.PackSize.Value;

        if (request.LeadTimeDays.HasValue)
            supplierPrice.LeadTimeDays = request.LeadTimeDays.Value;

        if (request.EffectiveDate.HasValue)
            supplierPrice.EffectiveDate = request.EffectiveDate.Value;

        if (request.ExpiryDate.HasValue)
            supplierPrice.ExpiryDate = request.ExpiryDate.Value;

        if (request.IsPrimary.HasValue)
            supplierPrice.IsPrimary = request.IsPrimary.Value;

        if (request.Notes != null)
            supplierPrice.Notes = request.Notes;

        supplierPrice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新供應商報價成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSupplierPriceAsync(int id)
    {
        var supplierPrice = await _context.SupplierPrices.FindAsync(id);
        if (supplierPrice == null)
        {
            return false;
        }

        _context.SupplierPrices.Remove(supplierPrice);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除供應商報價成功 - Id: {Id}", id);
        return true;
    }
}
