using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Coupons;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 優惠券服務介面
/// </summary>
public interface ICouponService
{
    /// <summary>
    /// 取得優惠券列表
    /// </summary>
    Task<PaginatedResponse<CouponListDto>> GetCouponsAsync(PaginationRequest request);

    /// <summary>
    /// 取得優惠券詳細資訊
    /// </summary>
    Task<CouponDetailDto?> GetCouponByIdAsync(int id);

    /// <summary>
    /// 依代碼取得優惠券
    /// </summary>
    Task<CouponDetailDto?> GetCouponByCodeAsync(string code);

    /// <summary>
    /// 建立優惠券
    /// </summary>
    Task<int?> CreateCouponAsync(CreateCouponRequest request);

    /// <summary>
    /// 更新優惠券
    /// </summary>
    Task<bool> UpdateCouponAsync(int id, UpdateCouponRequest request);

    /// <summary>
    /// 刪除優惠券
    /// </summary>
    Task<bool> DeleteCouponAsync(int id);
}
