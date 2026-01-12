using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 計量單位服務介面
/// </summary>
public interface IUnitService
{
    /// <summary>
    /// 取得單位列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="activeOnly">是否只取啟用的單位</param>
    /// <returns>分頁單位列表</returns>
    Task<PaginatedResponse<UnitListDto>> GetUnitsAsync(PaginationRequest request, bool? activeOnly = null);

    /// <summary>
    /// 取得所有啟用的單位（不分頁）
    /// </summary>
    /// <returns>單位列表</returns>
    Task<IEnumerable<UnitDto>> GetActiveUnitsAsync();

    /// <summary>
    /// 取得單位詳細資訊
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <returns>單位詳細資訊</returns>
    Task<UnitDetailDto?> GetUnitByIdAsync(int id);

    /// <summary>
    /// 根據代碼取得單位
    /// </summary>
    /// <param name="code">單位代碼</param>
    /// <returns>單位資訊</returns>
    Task<UnitDto?> GetUnitByCodeAsync(string code);

    /// <summary>
    /// 建立單位
    /// </summary>
    /// <param name="request">建立單位請求</param>
    /// <returns>建立的單位 ID，失敗返回 null</returns>
    Task<int?> CreateUnitAsync(CreateUnitRequest request);

    /// <summary>
    /// 更新單位
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <param name="request">更新單位請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateUnitAsync(int id, UpdateUnitRequest request);

    /// <summary>
    /// 刪除單位
    /// </summary>
    /// <param name="id">單位 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteUnitAsync(int id);
}
