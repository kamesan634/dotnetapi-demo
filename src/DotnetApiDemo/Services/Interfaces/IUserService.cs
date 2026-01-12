using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Users;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 使用者服務介面
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得使用者列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <returns>分頁使用者列表</returns>
    Task<PaginatedResponse<UserListDto>> GetUsersAsync(PaginationRequest request);

    /// <summary>
    /// 取得使用者詳細資訊
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>使用者詳細資訊</returns>
    Task<UserDetailDto?> GetUserByIdAsync(int id);

    /// <summary>
    /// 建立使用者
    /// </summary>
    /// <param name="request">建立使用者請求</param>
    /// <returns>建立結果</returns>
    Task<(bool Success, int? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// 更新使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="request">更新使用者請求</param>
    /// <returns>更新結果</returns>
    Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(int id, UpdateUserRequest request);

    /// <summary>
    /// 刪除使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteUserAsync(int id);

    /// <summary>
    /// 取得所有角色
    /// </summary>
    /// <returns>角色列表</returns>
    Task<IEnumerable<RoleDto>> GetRolesAsync();
}
