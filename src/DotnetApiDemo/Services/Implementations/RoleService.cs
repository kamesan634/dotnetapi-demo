using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Roles;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 角色服務實作
/// </summary>
public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<RoleService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public RoleService(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<RoleService> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<RoleListDto>> GetRolesAsync(PaginationRequest request)
    {
        var query = _context.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(r => r.Name!.Contains(request.Search) ||
                                     (r.Description != null && r.Description.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "sortorder" => request.IsDescending ? query.OrderByDescending(r => r.SortOrder) : query.OrderBy(r => r.SortOrder),
            _ => query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description,
                IsSystem = r.IsSystem,
                IsActive = r.IsActive,
                SortOrder = r.SortOrder,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<RoleListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<RoleDetailDto?> GetRoleByIdAsync(int id)
    {
        return await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDetailDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description,
                IsSystem = r.IsSystem,
                IsActive = r.IsActive,
                SortOrder = r.SortOrder,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateRoleAsync(CreateRoleRequest request)
    {
        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            _logger.LogWarning("建立角色失敗：名稱已存在 - {Name}", request.Name);
            return null;
        }

        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            _logger.LogWarning("建立角色失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        _logger.LogInformation("建立角色成功 - {Name}", role.Name);
        return role.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return false;
        }

        if (role.IsSystem)
        {
            _logger.LogWarning("無法更新系統角色 - Id: {Id}", id);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != role.Name)
        {
            if (await _roleManager.RoleExistsAsync(request.Name))
            {
                _logger.LogWarning("更新角色失敗：名稱已存在 - {Name}", request.Name);
                return false;
            }
            role.Name = request.Name;
        }

        if (request.Description != null)
            role.Description = request.Description;

        if (request.IsActive.HasValue)
            role.IsActive = request.IsActive.Value;

        if (request.SortOrder.HasValue)
            role.SortOrder = request.SortOrder.Value;

        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            _logger.LogWarning("更新角色失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        _logger.LogInformation("更新角色成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return false;
        }

        if (role.IsSystem)
        {
            _logger.LogWarning("無法刪除系統角色 - Id: {Id}", id);
            return false;
        }

        // 檢查是否有使用者使用此角色
        var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (hasUsers)
        {
            _logger.LogWarning("刪除角色失敗：存在使用此角色的使用者 - Id: {Id}", id);
            return false;
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            _logger.LogWarning("刪除角色失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        _logger.LogInformation("刪除角色成功 - Id: {Id}", id);
        return true;
    }
}
