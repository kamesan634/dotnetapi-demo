using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Stores;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 門市服務實作
/// </summary>
public class StoreService : IStoreService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StoreService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public StoreService(ApplicationDbContext context, ILogger<StoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<StoreListDto>> GetStoresAsync(PaginationRequest request)
    {
        var query = _context.Stores.Include(s => s.Warehouses).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.Name.Contains(request.Search) || s.Code.Contains(request.Search));
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
            .Select(s => new StoreListDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                IsActive = s.IsActive,
                WarehouseCount = s.Warehouses.Count
            })
            .ToListAsync();

        return new PaginatedResponse<StoreListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<StoreDetailDto?> GetStoreByIdAsync(int id)
    {
        return await _context.Stores
            .Where(s => s.Id == id)
            .Select(s => new StoreDetailDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                IsActive = s.IsActive,
                Warehouses = s.Warehouses.Select(w => new WarehouseListDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    StoreId = w.StoreId,
                    StoreName = s.Name,
                    IsActive = w.IsActive
                }),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateStoreAsync(CreateStoreRequest request)
    {
        if (await _context.Stores.AnyAsync(s => s.Code == request.Code))
        {
            _logger.LogWarning("建立門市失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var store = new Store
        {
            Code = request.Code,
            Name = request.Name,
            Phone = request.Phone,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Stores.Add(store);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立門市成功 - {Code}: {Name}", store.Code, store.Name);
        return store.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStoreAsync(int id, UpdateStoreRequest request)
    {
        var store = await _context.Stores.FindAsync(id);
        if (store == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            store.Name = request.Name;

        if (request.Phone != null)
            store.Phone = request.Phone;

        if (request.Address != null)
            store.Address = request.Address;

        if (request.IsActive.HasValue)
            store.IsActive = request.IsActive.Value;

        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新門市成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteStoreAsync(int id)
    {
        var store = await _context.Stores
            .Include(s => s.Warehouses)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null)
        {
            return false;
        }

        if (store.Warehouses.Any())
        {
            _logger.LogWarning("刪除門市失敗：存在關聯倉庫 - Id: {Id}", id);
            return false;
        }

        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除門市成功 - Id: {Id}", id);
        return true;
    }
}

/// <summary>
/// 倉庫服務實作
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WarehouseService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public WarehouseService(ApplicationDbContext context, ILogger<WarehouseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<WarehouseListDto>> GetWarehousesAsync(PaginationRequest request, int? storeId = null)
    {
        var query = _context.Warehouses.Include(w => w.Store).AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(w => w.StoreId == storeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(w => w.Name.Contains(request.Search) || w.Code.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(w => w.Code) : query.OrderBy(w => w.Code),
            "name" => request.IsDescending ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
            _ => query.OrderBy(w => w.Code)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarehouseListDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                StoreId = w.StoreId,
                StoreName = w.Store != null ? w.Store.Name : null,
                IsActive = w.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<WarehouseListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<WarehouseDetailDto?> GetWarehouseByIdAsync(int id)
    {
        return await _context.Warehouses
            .Where(w => w.Id == id)
            .Select(w => new WarehouseDetailDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                StoreId = w.StoreId,
                StoreName = w.Store != null ? w.Store.Name : null,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateWarehouseAsync(CreateWarehouseRequest request)
    {
        if (await _context.Warehouses.AnyAsync(w => w.Code == request.Code))
        {
            _logger.LogWarning("建立倉庫失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            StoreId = request.StoreId,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立倉庫成功 - {Code}: {Name}", warehouse.Code, warehouse.Name);
        return warehouse.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateWarehouseAsync(int id, UpdateWarehouseRequest request)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            warehouse.Name = request.Name;

        if (request.Address != null)
            warehouse.Address = request.Address;

        if (request.IsActive.HasValue)
            warehouse.IsActive = request.IsActive.Value;

        warehouse.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新倉庫成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteWarehouseAsync(int id)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Inventories)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null)
        {
            return false;
        }

        if (warehouse.Inventories.Any(i => i.Quantity > 0))
        {
            _logger.LogWarning("刪除倉庫失敗：倉庫尚有庫存 - Id: {Id}", id);
            return false;
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除倉庫成功 - Id: {Id}", id);
        return true;
    }
}
