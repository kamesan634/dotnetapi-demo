using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Users;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 使用者服務實作
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public UserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<UserListDto>> GetUsersAsync(PaginationRequest request)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u => u.UserName!.Contains(request.Search) ||
                                     u.Email!.Contains(request.Search) ||
                                     (u.RealName != null && u.RealName.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "username" => request.IsDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "email" => request.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "createdat" => request.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = new List<UserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.RealName ?? user.UserName!,
                Phone = user.PhoneNumber,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt
            });
        }

        return new PaginatedResponse<UserListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<UserDetailDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserStores)
            .ThenInclude(us => us.Store)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = await _context.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description
            })
            .ToListAsync();

        return new UserDetailDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.RealName ?? user.UserName!,
            Phone = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roles,
            Stores = user.UserStores.Select(us => new UserStoreDto
            {
                StoreId = us.StoreId,
                StoreName = us.Store.Name,
                IsDefault = us.IsPrimary
            }),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    /// <inheritdoc />
    public async Task<(bool Success, int? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            RealName = request.FullName,
            PhoneNumber = request.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("建立使用者失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, null, result.Errors.Select(e => e.Description));
        }

        // 指派角色
        if (request.RoleIds?.Any() == true)
        {
            var roles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync();

            if (roles.Any())
            {
                await _userManager.AddToRolesAsync(user, roles);
            }
        }

        // 指派門市
        if (request.StoreIds?.Any() == true)
        {
            var userStores = request.StoreIds.Select((storeId, index) => new Models.Entities.UserStore
            {
                UserId = user.Id,
                StoreId = storeId,
                IsPrimary = index == 0
            });
            _context.Set<Models.Entities.UserStore>().AddRange(userStores);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("建立使用者成功 - {UserName}", user.UserName);
        return (true, user.Id, Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return (false, new[] { "找不到使用者" });
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.RealName = request.FullName;

        if (request.Phone != null)
            user.PhoneNumber = request.Phone;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogWarning("更新使用者失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description));
        }

        // 更新角色
        if (request.RoleIds != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var newRoles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync();

            if (newRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, newRoles);
            }
        }

        // 更新門市
        if (request.StoreIds != null)
        {
            var existingStores = await _context.Set<Models.Entities.UserStore>()
                .Where(us => us.UserId == id)
                .ToListAsync();
            _context.Set<Models.Entities.UserStore>().RemoveRange(existingStores);

            if (request.StoreIds.Any())
            {
                var userStores = request.StoreIds.Select((storeId, index) => new Models.Entities.UserStore
                {
                    UserId = user.Id,
                    StoreId = storeId,
                    IsPrimary = index == 0
                });
                _context.Set<Models.Entities.UserStore>().AddRange(userStores);
            }

            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("更新使用者成功 - Id: {Id}", id);
        return (true, Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return false;
        }

        // 軟刪除：停用帳號
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogWarning("刪除使用者失敗 - {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        _logger.LogInformation("刪除使用者成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetRolesAsync()
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.SortOrder)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description
            })
            .ToListAsync();
    }
}
