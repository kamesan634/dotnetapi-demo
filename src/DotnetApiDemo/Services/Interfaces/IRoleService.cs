using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Roles;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 角色服務介面
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 取得角色列表
    /// </summary>
    Task<PaginatedResponse<RoleListDto>> GetRolesAsync(PaginationRequest request);

    /// <summary>
    /// 取得角色詳細資訊
    /// </summary>
    Task<RoleDetailDto?> GetRoleByIdAsync(int id);

    /// <summary>
    /// 建立角色
    /// </summary>
    Task<int?> CreateRoleAsync(CreateRoleRequest request);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<bool> UpdateRoleAsync(int id, UpdateRoleRequest request);

    /// <summary>
    /// 刪除角色
    /// </summary>
    Task<bool> DeleteRoleAsync(int id);
}
