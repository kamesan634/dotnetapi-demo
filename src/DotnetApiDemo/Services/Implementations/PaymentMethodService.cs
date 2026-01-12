using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.PaymentMethods;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 付款方式服務實作
/// </summary>
public class PaymentMethodService : IPaymentMethodService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentMethodService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public PaymentMethodService(ApplicationDbContext context, ILogger<PaymentMethodService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PaymentMethodListDto>> GetPaymentMethodsAsync(PaginationRequest request)
    {
        var query = _context.PaymentMethods.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(pm => pm.Code.Contains(request.Search) ||
                                       pm.Name.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(pm => pm.Code) : query.OrderBy(pm => pm.Code),
            "name" => request.IsDescending ? query.OrderByDescending(pm => pm.Name) : query.OrderBy(pm => pm.Name),
            "sortorder" => request.IsDescending ? query.OrderByDescending(pm => pm.SortOrder) : query.OrderBy(pm => pm.SortOrder),
            _ => query.OrderBy(pm => pm.SortOrder).ThenBy(pm => pm.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pm => new PaymentMethodListDto
            {
                Id = pm.Id,
                Code = pm.Code,
                Name = pm.Name,
                IsDefault = pm.IsDefault,
                IsActive = pm.IsActive,
                SortOrder = pm.SortOrder,
                FeeRate = pm.FeeRate
            })
            .ToListAsync();

        return new PaginatedResponse<PaymentMethodListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PaymentMethodListDto>> GetActivePaymentMethodsAsync()
    {
        return await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.SortOrder)
            .ThenBy(pm => pm.Name)
            .Select(pm => new PaymentMethodListDto
            {
                Id = pm.Id,
                Code = pm.Code,
                Name = pm.Name,
                IsDefault = pm.IsDefault,
                IsActive = pm.IsActive,
                SortOrder = pm.SortOrder,
                FeeRate = pm.FeeRate
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PaymentMethodDetailDto?> GetPaymentMethodByIdAsync(int id)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.Id == id)
            .Select(pm => new PaymentMethodDetailDto
            {
                Id = pm.Id,
                Code = pm.Code,
                Name = pm.Name,
                Description = pm.Description,
                IsDefault = pm.IsDefault,
                IsActive = pm.IsActive,
                SortOrder = pm.SortOrder,
                FeeRate = pm.FeeRate,
                IconUrl = pm.IconUrl,
                CreatedAt = pm.CreatedAt,
                UpdatedAt = pm.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<PaymentMethodDetailDto?> GetPaymentMethodByCodeAsync(string code)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.Code == code)
            .Select(pm => new PaymentMethodDetailDto
            {
                Id = pm.Id,
                Code = pm.Code,
                Name = pm.Name,
                Description = pm.Description,
                IsDefault = pm.IsDefault,
                IsActive = pm.IsActive,
                SortOrder = pm.SortOrder,
                FeeRate = pm.FeeRate,
                IconUrl = pm.IconUrl,
                CreatedAt = pm.CreatedAt,
                UpdatedAt = pm.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        if (await _context.PaymentMethods.AnyAsync(pm => pm.Code == request.Code))
        {
            _logger.LogWarning("建立付款方式失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        // 如果設為預設，先將其他付款方式的預設取消
        if (request.IsDefault)
        {
            await _context.PaymentMethods
                .Where(pm => pm.IsDefault)
                .ExecuteUpdateAsync(pm => pm.SetProperty(x => x.IsDefault, false));
        }

        var paymentMethod = new PaymentMethod
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault,
            IsActive = true,
            SortOrder = request.SortOrder,
            FeeRate = request.FeeRate,
            IconUrl = request.IconUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立付款方式成功 - {Code}", paymentMethod.Code);
        return paymentMethod.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePaymentMethodAsync(int id, UpdatePaymentMethodRequest request)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(id);
        if (paymentMethod == null)
        {
            return false;
        }

        // 如果設為預設，先將其他付款方式的預設取消
        if (request.IsDefault == true && !paymentMethod.IsDefault)
        {
            await _context.PaymentMethods
                .Where(pm => pm.IsDefault && pm.Id != id)
                .ExecuteUpdateAsync(pm => pm.SetProperty(x => x.IsDefault, false));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            paymentMethod.Name = request.Name;

        if (request.Description != null)
            paymentMethod.Description = request.Description;

        if (request.IsDefault.HasValue)
            paymentMethod.IsDefault = request.IsDefault.Value;

        if (request.IsActive.HasValue)
            paymentMethod.IsActive = request.IsActive.Value;

        if (request.SortOrder.HasValue)
            paymentMethod.SortOrder = request.SortOrder.Value;

        if (request.FeeRate.HasValue)
            paymentMethod.FeeRate = request.FeeRate.Value;

        if (request.IconUrl != null)
            paymentMethod.IconUrl = request.IconUrl;

        paymentMethod.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新付款方式成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeletePaymentMethodAsync(int id)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(id);
        if (paymentMethod == null)
        {
            return false;
        }

        // 檢查是否有關聯的付款記錄
        var hasPayments = await _context.Payments.AnyAsync(p => p.PaymentMethod.ToString() == paymentMethod.Code);
        if (hasPayments)
        {
            _logger.LogWarning("刪除付款方式失敗：存在關聯付款記錄 - Id: {Id}", id);
            return false;
        }

        _context.PaymentMethods.Remove(paymentMethod);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除付款方式成功 - Id: {Id}", id);
        return true;
    }
}
