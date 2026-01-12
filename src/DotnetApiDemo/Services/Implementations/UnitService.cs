using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 計量單位服務實作
/// </summary>
public class UnitService : IUnitService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitService> _logger;

    public UnitService(ApplicationDbContext context, ILogger<UnitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<UnitListDto>> GetUnitsAsync(PaginationRequest request, bool? activeOnly = null)
    {
        var query = _context.Units.AsQueryable();

        if (activeOnly == true)
        {
            query = query.Where(u => u.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u => u.Name.Contains(request.Search) || u.Code.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(u => u.Code) : query.OrderBy(u => u.Code),
            "name" => request.IsDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
            _ => query.OrderBy(u => u.SortOrder).ThenBy(u => u.Code)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UnitListDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Description = u.Description,
                IsSystem = u.IsSystem,
                IsActive = u.IsActive,
                SortOrder = u.SortOrder
            })
            .ToListAsync();

        return new PaginatedResponse<UnitListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UnitDto>> GetActiveUnitsAsync()
    {
        return await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.SortOrder)
            .ThenBy(u => u.Name)
            .Select(u => new UnitDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<UnitDetailDto?> GetUnitByIdAsync(int id)
    {
        return await _context.Units
            .Where(u => u.Id == id)
            .Select(u => new UnitDetailDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Description = u.Description,
                IsSystem = u.IsSystem,
                IsActive = u.IsActive,
                SortOrder = u.SortOrder,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<UnitDto?> GetUnitByCodeAsync(string code)
    {
        return await _context.Units
            .Where(u => u.Code == code)
            .Select(u => new UnitDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateUnitAsync(CreateUnitRequest request)
    {
        if (await _context.Units.AnyAsync(u => u.Code == request.Code))
        {
            _logger.LogWarning("建立單位失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var unit = new Unit
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsSystem = false,
            IsActive = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立單位成功 - {Code}: {Name}", unit.Code, unit.Name);
        return unit.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUnitAsync(int id, UpdateUnitRequest request)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            unit.Name = request.Name;

        if (request.Description != null)
            unit.Description = request.Description;

        if (request.SortOrder.HasValue)
            unit.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            unit.IsActive = request.IsActive.Value;

        unit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新單位成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUnitAsync(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
        {
            return false;
        }

        if (unit.IsSystem)
        {
            _logger.LogWarning("刪除單位失敗：系統單位不可刪除 - Id: {Id}", id);
            return false;
        }

        // 檢查是否有商品使用此單位
        var hasProducts = await _context.Products.AnyAsync(p => p.UnitId == id);
        if (hasProducts)
        {
            _logger.LogWarning("刪除單位失敗：有商品使用此單位 - Id: {Id}", id);
            return false;
        }

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除單位成功 - Id: {Id}", id);
        return true;
    }
}
