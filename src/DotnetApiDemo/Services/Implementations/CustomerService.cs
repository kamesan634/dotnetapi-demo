using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 會員等級服務實作
/// </summary>
public class CustomerLevelService : ICustomerLevelService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerLevelService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CustomerLevelService(ApplicationDbContext context, ILogger<CustomerLevelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerLevelListDto>> GetCustomerLevelsAsync()
    {
        return await _context.CustomerLevels
            .Select(l => new CustomerLevelListDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                DiscountRate = l.DiscountRate,
                MinSpendAmount = l.MinSpendAmount,
                CustomerCount = l.Customers.Count
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateCustomerLevelAsync(CreateCustomerLevelRequest request)
    {
        if (await _context.CustomerLevels.AnyAsync(l => l.Code == request.Code))
        {
            _logger.LogWarning("建立會員等級失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var level = new CustomerLevel
        {
            Code = request.Code,
            Name = request.Name,
            DiscountRate = request.DiscountRate,
            MinSpendAmount = request.MinSpendAmount,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerLevels.Add(level);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立會員等級成功 - {Code}: {Name}", level.Code, level.Name);
        return level.Id;
    }
}

/// <summary>
/// 客戶/會員服務實作
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<CustomerListDto>> GetCustomersAsync(PaginationRequest request, int? levelId = null)
    {
        var query = _context.Customers.Include(c => c.CustomerLevel).AsQueryable();

        if (levelId.HasValue)
        {
            query = query.Where(c => c.CustomerLevelId == levelId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Name.Contains(request.Search) ||
                                     c.MemberNo.Contains(request.Search) ||
                                     (c.Phone != null && c.Phone.Contains(request.Search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "memberno" => request.IsDescending ? query.OrderByDescending(c => c.MemberNo) : query.OrderBy(c => c.MemberNo),
            "totalspent" => request.IsDescending ? query.OrderByDescending(c => c.TotalSpent) : query.OrderBy(c => c.TotalSpent),
            _ => query.OrderBy(c => c.MemberNo)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerListDto
            {
                Id = c.Id,
                MemberNo = c.MemberNo,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                LevelName = c.CustomerLevel.Name,
                TotalSpent = c.TotalSpent,
                TotalPoints = c.TotalPoints,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return new PaginatedResponse<CustomerListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int id)
    {
        return await GetCustomerDetailAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<CustomerDetailDto?> GetCustomerByMemberNoAsync(string memberNo)
    {
        return await GetCustomerDetailAsync(c => c.MemberNo == memberNo);
    }

    /// <inheritdoc />
    public async Task<CustomerDetailDto?> GetCustomerByPhoneAsync(string phone)
    {
        return await GetCustomerDetailAsync(c => c.Phone == phone);
    }

    private async Task<CustomerDetailDto?> GetCustomerDetailAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate)
    {
        return await _context.Customers
            .Where(predicate)
            .Select(c => new CustomerDetailDto
            {
                Id = c.Id,
                MemberNo = c.MemberNo,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Birthday = c.Birthday,
                Gender = c.Gender,
                LevelId = c.CustomerLevelId,
                LevelName = c.CustomerLevel.Name,
                TotalSpent = c.TotalSpent,
                TotalPoints = c.TotalPoints,
                Notes = c.Remarks,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                LastPurchaseAt = c.LastPurchaseDate
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // 產生會員編號
        var memberNo = await GenerateMemberNoAsync();

        // 取得預設等級
        var defaultLevelId = request.LevelId ?? await _context.CustomerLevels
            .Where(l => l.MinSpendAmount == 0)
            .Select(l => l.Id)
            .FirstOrDefaultAsync();

        var customer = new Customer
        {
            MemberNo = memberNo,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Birthday = request.Birthday,
            Gender = request.Gender,
            CustomerLevelId = defaultLevelId,
            TotalSpent = 0,
            TotalPoints = 0,
            Remarks = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立會員成功 - {MemberNo}: {Name}", memberNo, customer.Name);
        return customer.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            customer.Name = request.Name;

        if (request.Phone != null)
            customer.Phone = request.Phone;

        if (request.Email != null)
            customer.Email = request.Email;

        if (request.Address != null)
            customer.Address = request.Address;

        if (request.Birthday.HasValue)
            customer.Birthday = request.Birthday;

        if (request.Gender != null)
            customer.Gender = request.Gender;

        if (request.LevelId.HasValue)
            customer.CustomerLevelId = request.LevelId.Value;

        if (request.Notes != null)
            customer.Remarks = request.Notes;

        if (request.IsActive.HasValue)
            customer.IsActive = request.IsActive.Value;

        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新會員成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除會員成功 - Id: {Id}", id);
        return true;
    }

    private async Task<string> GenerateMemberNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"M{today}";

        var lastCustomer = await _context.Customers
            .Where(c => c.MemberNo.StartsWith(prefix))
            .OrderByDescending(c => c.MemberNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastCustomer != null && lastCustomer.MemberNo.Length > prefix.Length)
        {
            if (int.TryParse(lastCustomer.MemberNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
