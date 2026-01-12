using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.SystemSettings;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 系統設定服務介面
/// </summary>
public interface ISystemSettingService
{
    /// <summary>
    /// 取得系統設定列表
    /// </summary>
    Task<PaginatedResponse<SystemSettingListDto>> GetSettingsAsync(PaginationRequest request);

    /// <summary>
    /// 依分類取得設定
    /// </summary>
    Task<IEnumerable<SystemSettingListDto>> GetSettingsByCategoryAsync(string category);

    /// <summary>
    /// 取得設定詳細資訊
    /// </summary>
    Task<SystemSettingDetailDto?> GetSettingByIdAsync(int id);

    /// <summary>
    /// 依鍵值取得設定
    /// </summary>
    Task<SystemSettingDetailDto?> GetSettingByKeyAsync(string category, string key);

    /// <summary>
    /// 取得設定值
    /// </summary>
    Task<string?> GetValueAsync(string category, string key);

    /// <summary>
    /// 建立系統設定
    /// </summary>
    Task<int?> CreateSettingAsync(CreateSystemSettingRequest request);

    /// <summary>
    /// 更新系統設定
    /// </summary>
    Task<bool> UpdateSettingAsync(int id, UpdateSystemSettingRequest request, int userId);

    /// <summary>
    /// 刪除系統設定
    /// </summary>
    Task<bool> DeleteSettingAsync(int id);

    /// <summary>
    /// 取得所有分類
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();
}
