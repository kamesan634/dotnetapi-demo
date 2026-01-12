using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 促銷活動服務實作
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromotionService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PromotionService(ApplicationDbContext context, ILogger<PromotionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PromotionListDto>> GetPromotionsAsync(PaginationRequest request)
    {
        var query = _context.Promotions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) ||
                                     p.Code.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
            "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "startdate" => request.IsDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PromotionListDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                PromotionType = p.PromotionType,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountValue = p.DiscountValue,
                UsedCount = p.UsedCount,
                MaxUsageCount = p.MaxUsageCount
            })
            .ToListAsync();

        return new PaginatedResponse<PromotionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PromotionDetailDto?> GetPromotionByIdAsync(int id)
    {
        return await _context.Promotions
            .Where(p => p.Id == id)
            .Select(p => new PromotionDetailDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PromotionType = p.PromotionType,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountValue = p.DiscountValue,
                MinPurchaseAmount = p.MinPurchaseAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MaxUsageCount = p.MaxUsageCount,
                UsedCount = p.UsedCount,
                MaxUsagePerCustomer = p.MaxUsagePerCustomer,
                CanCombine = p.CanCombine,
                Priority = p.Priority,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreatePromotionAsync(CreatePromotionRequest request)
    {
        if (await _context.Promotions.AnyAsync(p => p.Code == request.Code))
        {
            _logger.LogWarning("建立促銷活動失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var promotion = new Promotion
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            PromotionType = request.PromotionType,
            Status = PromotionStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DiscountValue = request.DiscountValue,
            MinPurchaseAmount = request.MinPurchaseAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MaxUsageCount = request.MaxUsageCount,
            MaxUsagePerCustomer = request.MaxUsagePerCustomer,
            CanCombine = request.CanCombine,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立促銷活動成功 - {Code}: {Name}", promotion.Code, promotion.Name);
        return promotion.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            promotion.Name = request.Name;

        if (request.Description != null)
            promotion.Description = request.Description;

        if (request.Status.HasValue)
            promotion.Status = request.Status.Value;

        if (request.StartDate.HasValue)
            promotion.StartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            promotion.EndDate = request.EndDate.Value;

        if (request.DiscountValue.HasValue)
            promotion.DiscountValue = request.DiscountValue.Value;

        if (request.MinPurchaseAmount.HasValue)
            promotion.MinPurchaseAmount = request.MinPurchaseAmount.Value;

        if (request.MaxDiscountAmount.HasValue)
            promotion.MaxDiscountAmount = request.MaxDiscountAmount.Value;

        if (request.MaxUsageCount.HasValue)
            promotion.MaxUsageCount = request.MaxUsageCount.Value;

        if (request.MaxUsagePerCustomer.HasValue)
            promotion.MaxUsagePerCustomer = request.MaxUsagePerCustomer.Value;

        if (request.CanCombine.HasValue)
            promotion.CanCombine = request.CanCombine.Value;

        if (request.Priority.HasValue)
            promotion.Priority = request.Priority.Value;

        promotion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新促銷活動成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);

        if (promotion == null)
        {
            return false;
        }

        // 檢查是否有已使用的優惠券
        var coupons = await _context.Coupons.Where(c => c.PromotionId == id).ToListAsync();
        if (coupons.Any(c => c.IsUsed))
        {
            _logger.LogWarning("刪除促銷活動失敗：存在已使用的優惠券 - Id: {Id}", id);
            return false;
        }

        // 刪除關聯的優惠券
        _context.Coupons.RemoveRange(coupons);
        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除促銷活動成功 - Id: {Id}", id);
        return true;
    }
}
