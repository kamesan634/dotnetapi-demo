using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Coupons;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 優惠券服務實作
/// </summary>
public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CouponService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CouponService(ApplicationDbContext context, ILogger<CouponService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<CouponListDto>> GetCouponsAsync(PaginationRequest request)
    {
        var query = _context.Coupons
            .Include(c => c.Promotion)
            .Include(c => c.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Code.Contains(request.Search) ||
                                     c.Promotion.Name.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "validfrom" => request.IsDescending ? query.OrderByDescending(c => c.ValidFrom) : query.OrderBy(c => c.ValidFrom),
            "validto" => request.IsDescending ? query.OrderByDescending(c => c.ValidTo) : query.OrderBy(c => c.ValidTo),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CouponListDto
            {
                Id = c.Id,
                Code = c.Code,
                PromotionId = c.PromotionId,
                PromotionName = c.Promotion.Name,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                IsUsed = c.IsUsed,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<CouponListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<CouponDetailDto?> GetCouponByIdAsync(int id)
    {
        return await _context.Coupons
            .Include(c => c.Promotion)
            .Include(c => c.Customer)
            .Where(c => c.Id == id)
            .Select(c => new CouponDetailDto
            {
                Id = c.Id,
                Code = c.Code,
                PromotionId = c.PromotionId,
                PromotionName = c.Promotion.Name,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                IsUsed = c.IsUsed,
                UsedAt = c.UsedAt,
                UsedOrderId = c.UsedOrderId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<CouponDetailDto?> GetCouponByCodeAsync(string code)
    {
        return await _context.Coupons
            .Include(c => c.Promotion)
            .Include(c => c.Customer)
            .Where(c => c.Code == code)
            .Select(c => new CouponDetailDto
            {
                Id = c.Id,
                Code = c.Code,
                PromotionId = c.PromotionId,
                PromotionName = c.Promotion.Name,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                IsUsed = c.IsUsed,
                UsedAt = c.UsedAt,
                UsedOrderId = c.UsedOrderId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateCouponAsync(CreateCouponRequest request)
    {
        if (await _context.Coupons.AnyAsync(c => c.Code == request.Code))
        {
            _logger.LogWarning("建立優惠券失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var promotion = await _context.Promotions.FindAsync(request.PromotionId);
        if (promotion == null)
        {
            _logger.LogWarning("建立優惠券失敗：找不到促銷活動 - PromotionId: {PromotionId}", request.PromotionId);
            return null;
        }

        if (request.CustomerId.HasValue)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
            if (customer == null)
            {
                _logger.LogWarning("建立優惠券失敗：找不到客戶 - CustomerId: {CustomerId}", request.CustomerId);
                return null;
            }
        }

        var coupon = new Coupon
        {
            Code = request.Code,
            PromotionId = request.PromotionId,
            CustomerId = request.CustomerId,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsUsed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立優惠券成功 - {Code}", coupon.Code);
        return coupon.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateCouponAsync(int id, UpdateCouponRequest request)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
        {
            return false;
        }

        if (coupon.IsUsed)
        {
            _logger.LogWarning("更新優惠券失敗：優惠券已使用 - Id: {Id}", id);
            return false;
        }

        if (request.CustomerId.HasValue)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
            if (customer == null)
            {
                _logger.LogWarning("更新優惠券失敗：找不到客戶 - CustomerId: {CustomerId}", request.CustomerId);
                return false;
            }
            coupon.CustomerId = request.CustomerId.Value;
        }

        if (request.ValidFrom.HasValue)
            coupon.ValidFrom = request.ValidFrom.Value;

        if (request.ValidTo.HasValue)
            coupon.ValidTo = request.ValidTo.Value;

        if (request.IsActive.HasValue)
            coupon.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新優惠券成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCouponAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
        {
            return false;
        }

        if (coupon.IsUsed)
        {
            _logger.LogWarning("刪除優惠券失敗：優惠券已使用 - Id: {Id}", id);
            return false;
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除優惠券成功 - Id: {Id}", id);
        return true;
    }
}
