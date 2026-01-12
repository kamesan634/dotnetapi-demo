using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 供應商服務實作
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SupplierService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public SupplierService(ApplicationDbContext context, ILogger<SupplierService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<SupplierListDto>> GetSuppliersAsync(PaginationRequest request)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.Name.Contains(request.Search) ||
                                     s.Code.Contains(request.Search) ||
                                     (s.ContactName != null && s.ContactName.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(s => s.Code) : query.OrderBy(s => s.Code),
            "name" => request.IsDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            _ => query.OrderBy(s => s.Code)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SupplierListDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactPerson = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                PaymentTerms = s.PaymentTerms,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<SupplierListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<SupplierDetailDto?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.SupplierPrices)
            .ThenInclude(sp => sp.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return null;

        return new SupplierDetailDto
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            ContactPerson = supplier.ContactName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            TaxId = supplier.TaxId,
            BankAccount = supplier.BankAccount,
            PaymentTerms = supplier.PaymentTerms,
            Notes = supplier.Remarks,
            IsActive = supplier.IsActive,
            Products = supplier.SupplierPrices.Select(sp => new SupplierProductDto
            {
                ProductId = sp.ProductId,
                ProductSku = sp.Product.Sku,
                ProductName = sp.Product.Name,
                SupplierProductCode = sp.SupplierSku,
                PurchasePrice = sp.UnitPrice,
                MinOrderQuantity = sp.MinOrderQuantity ?? 0,
                LeadTimeDays = sp.LeadTimeDays ?? 0
            }),
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<int?> CreateSupplierAsync(CreateSupplierRequest request)
    {
        if (await _context.Suppliers.AnyAsync(s => s.Code == request.Code))
        {
            _logger.LogWarning("建立供應商失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var supplier = new Supplier
        {
            Code = request.Code,
            Name = request.Name,
            ContactName = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            TaxId = request.TaxId,
            BankAccount = request.BankAccount,
            PaymentTerms = request.PaymentTerms,
            Remarks = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立供應商成功 - {Code}: {Name}", supplier.Code, supplier.Name);
        return supplier.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            supplier.Name = request.Name;

        if (request.ContactPerson != null)
            supplier.ContactName = request.ContactPerson;

        if (request.Phone != null)
            supplier.Phone = request.Phone;

        if (request.Email != null)
            supplier.Email = request.Email;

        if (request.Address != null)
            supplier.Address = request.Address;

        if (request.TaxId != null)
            supplier.TaxId = request.TaxId;

        if (request.BankAccount != null)
            supplier.BankAccount = request.BankAccount;

        if (request.PaymentTerms.HasValue)
            supplier.PaymentTerms = request.PaymentTerms.Value;

        if (request.Notes != null)
            supplier.Remarks = request.Notes;

        if (request.IsActive.HasValue)
            supplier.IsActive = request.IsActive.Value;

        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新供應商成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            return false;
        }

        if (supplier.PurchaseOrders.Any())
        {
            _logger.LogWarning("刪除供應商失敗：存在關聯採購單 - Id: {Id}", id);
            return false;
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除供應商成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SetSupplierPriceAsync(int supplierId, SupplierPriceRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(supplierId);
        if (supplier == null)
        {
            return false;
        }

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return false;
        }

        var existingPrice = await _context.SupplierPrices
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId && sp.ProductId == request.ProductId);

        if (existingPrice != null)
        {
            existingPrice.SupplierSku = request.SupplierProductCode;
            existingPrice.UnitPrice = request.PurchasePrice;
            existingPrice.MinOrderQuantity = request.MinOrderQuantity;
            existingPrice.LeadTimeDays = request.LeadTimeDays;
            existingPrice.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var supplierPrice = new SupplierPrice
            {
                SupplierId = supplierId,
                ProductId = request.ProductId,
                SupplierSku = request.SupplierProductCode,
                UnitPrice = request.PurchasePrice,
                MinOrderQuantity = request.MinOrderQuantity,
                LeadTimeDays = request.LeadTimeDays,
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            };
            _context.SupplierPrices.Add(supplierPrice);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("設定供應商價格成功 - SupplierId: {SupplierId}, ProductId: {ProductId}", supplierId, request.ProductId);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(int supplierId)
    {
        return await _context.SupplierPrices
            .Include(sp => sp.Product)
            .Where(sp => sp.SupplierId == supplierId)
            .Select(sp => new SupplierProductDto
            {
                ProductId = sp.ProductId,
                ProductSku = sp.Product.Sku,
                ProductName = sp.Product.Name,
                SupplierProductCode = sp.SupplierSku,
                PurchasePrice = sp.UnitPrice,
                MinOrderQuantity = sp.MinOrderQuantity ?? 0,
                LeadTimeDays = sp.LeadTimeDays ?? 0
            })
            .ToListAsync();
    }
}
