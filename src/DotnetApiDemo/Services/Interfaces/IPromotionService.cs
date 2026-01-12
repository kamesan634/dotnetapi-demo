using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 促銷活動服務介面
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// 取得促銷活動列表
    /// </summary>
    Task<PaginatedResponse<PromotionListDto>> GetPromotionsAsync(PaginationRequest request);

    /// <summary>
    /// 取得促銷活動詳細資訊
    /// </summary>
    Task<PromotionDetailDto?> GetPromotionByIdAsync(int id);

    /// <summary>
    /// 建立促銷活動
    /// </summary>
    Task<int?> CreatePromotionAsync(CreatePromotionRequest request);

    /// <summary>
    /// 更新促銷活動
    /// </summary>
    Task<bool> UpdatePromotionAsync(int id, UpdatePromotionRequest request);

    /// <summary>
    /// 刪除促銷活動
    /// </summary>
    Task<bool> DeletePromotionAsync(int id);
}
