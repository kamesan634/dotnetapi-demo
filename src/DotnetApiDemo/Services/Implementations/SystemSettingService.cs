using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SystemSettings;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 系統設定服務實作
/// </summary>
public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SystemSettingService> _logger;

    public SystemSettingService(ApplicationDbContext context, ILogger<SystemSettingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<SystemSettingListDto>> GetSettingsAsync(PaginationRequest request)
    {
        var query = _context.SystemSettings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.Key.Contains(request.Search) ||
                                      s.Category.Contains(request.Search) ||
                                      s.Value.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "key" => request.IsDescending ? query.OrderByDescending(s => s.Key) : query.OrderBy(s => s.Key),
            "category" => request.IsDescending ? query.OrderByDescending(s => s.Category) : query.OrderBy(s => s.Category),
            _ => query.OrderBy(s => s.Category).ThenBy(s => s.Key)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SystemSettingListDto
            {
                Id = s.Id,
                Category = s.Category,
                Key = s.Key,
                Value = s.Value,
                ValueType = s.ValueType,
                IsSystem = s.IsSystem,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<SystemSettingListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<IEnumerable<SystemSettingListDto>> GetSettingsByCategoryAsync(string category)
    {
        return await _context.SystemSettings
            .Where(s => s.Category == category)
            .OrderBy(s => s.Key)
            .Select(s => new SystemSettingListDto
            {
                Id = s.Id,
                Category = s.Category,
                Key = s.Key,
                Value = s.Value,
                ValueType = s.ValueType,
                IsSystem = s.IsSystem,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<SystemSettingDetailDto?> GetSettingByIdAsync(int id)
    {
        return await _context.SystemSettings
            .Include(s => s.UpdatedBy)
            .Where(s => s.Id == id)
            .Select(s => new SystemSettingDetailDto
            {
                Id = s.Id,
                Category = s.Category,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                ValueType = s.ValueType,
                IsSystem = s.IsSystem,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                UpdatedByName = s.UpdatedBy != null ? s.UpdatedBy.RealName : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SystemSettingDetailDto?> GetSettingByKeyAsync(string category, string key)
    {
        return await _context.SystemSettings
            .Include(s => s.UpdatedBy)
            .Where(s => s.Category == category && s.Key == key)
            .Select(s => new SystemSettingDetailDto
            {
                Id = s.Id,
                Category = s.Category,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                ValueType = s.ValueType,
                IsSystem = s.IsSystem,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                UpdatedByName = s.UpdatedBy != null ? s.UpdatedBy.RealName : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetValueAsync(string category, string key)
    {
        return await _context.SystemSettings
            .Where(s => s.Category == category && s.Key == key && s.IsActive)
            .Select(s => s.Value)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateSettingAsync(CreateSystemSettingRequest request)
    {
        if (await _context.SystemSettings.AnyAsync(s => s.Category == request.Category && s.Key == request.Key))
        {
            _logger.LogWarning("建立系統設定失敗：設定已存在 - {Category}.{Key}", request.Category, request.Key);
            return null;
        }

        var setting = new SystemSetting
        {
            Category = request.Category,
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            ValueType = request.ValueType,
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.SystemSettings.Add(setting);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立系統設定成功 - {Category}.{Key}", request.Category, request.Key);
        return setting.Id;
    }

    public async Task<bool> UpdateSettingAsync(int id, UpdateSystemSettingRequest request, int userId)
    {
        var setting = await _context.SystemSettings.FindAsync(id);
        if (setting == null)
        {
            return false;
        }

        if (request.Value != null)
            setting.Value = request.Value;

        if (request.Description != null)
            setting.Description = request.Description;

        if (request.IsActive.HasValue)
            setting.IsActive = request.IsActive.Value;

        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedById = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新系統設定成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteSettingAsync(int id)
    {
        var setting = await _context.SystemSettings.FindAsync(id);
        if (setting == null)
        {
            return false;
        }

        if (setting.IsSystem)
        {
            _logger.LogWarning("刪除系統設定失敗：不可刪除系統設定 - Id: {Id}", id);
            return false;
        }

        _context.SystemSettings.Remove(setting);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除系統設定成功 - Id: {Id}", id);
        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.SystemSettings
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
