using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Points;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class PointService : IPointService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PointService> _logger;

    public PointService(ApplicationDbContext context, ILogger<PointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PointTransactionListDto>> GetTransactionsAsync(PaginationRequest request, int? customerId = null)
    {
        var query = _context.PointTransactions
            .Include(p => p.Customer)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PointTransactionListDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.Name,
                TransactionType = p.TransactionType,
                Points = p.Points,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<PointTransactionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PointBalanceDto?> GetBalanceAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null) return null;

        var transactions = await _context.PointTransactions
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();

        var totalEarned = transactions.Where(p => p.TransactionType == "Earn" || (p.TransactionType == "Adjust" && p.Points > 0)).Sum(p => p.Points);
        var totalRedeemed = transactions.Where(p => p.TransactionType == "Redeem" || (p.TransactionType == "Adjust" && p.Points < 0)).Sum(p => Math.Abs(p.Points));

        var expiringPoints = await _context.PointTransactions
            .Where(p => p.CustomerId == customerId && p.TransactionType == "Earn" && p.ExpiresAt != null && p.ExpiresAt <= DateTime.UtcNow.AddDays(30))
            .SumAsync(p => p.Points);

        var nextExpiry = await _context.PointTransactions
            .Where(p => p.CustomerId == customerId && p.TransactionType == "Earn" && p.ExpiresAt != null && p.ExpiresAt > DateTime.UtcNow)
            .OrderBy(p => p.ExpiresAt)
            .Select(p => p.ExpiresAt)
            .FirstOrDefaultAsync();

        return new PointBalanceDto
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            CurrentPoints = customer.CurrentPoints,
            TotalEarned = totalEarned,
            TotalRedeemed = totalRedeemed,
            ExpiringPoints = expiringPoints,
            NextExpiryDate = nextExpiry
        };
    }

    public async Task<bool> EarnPointsAsync(EarnPointsRequest request, int userId)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null) return false;

        var transaction = new PointTransaction
        {
            CustomerId = request.CustomerId,
            TransactionType = "Earn",
            Points = request.Points,
            OrderId = request.OrderId,
            Description = request.Description ?? "點數獲得",
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(request.ExpiryDays.Value) : null
        };

        customer.CurrentPoints += request.Points;
        customer.TotalPoints += request.Points;

        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RedeemPointsAsync(RedeemPointsRequest request, int userId)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null || customer.CurrentPoints < request.Points) return false;

        var transaction = new PointTransaction
        {
            CustomerId = request.CustomerId,
            TransactionType = "Redeem",
            Points = -request.Points,
            OrderId = request.OrderId,
            Description = request.Description ?? "點數兌換",
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        customer.CurrentPoints -= request.Points;

        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AdjustPointsAsync(AdjustPointsRequest request, int userId)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null) return false;

        if (request.Points < 0 && customer.CurrentPoints < Math.Abs(request.Points))
            return false;

        var transaction = new PointTransaction
        {
            CustomerId = request.CustomerId,
            TransactionType = "Adjust",
            Points = request.Points,
            Description = request.Reason,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        customer.CurrentPoints += request.Points;
        if (request.Points > 0) customer.TotalPoints += request.Points;

        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task ExpirePointsAsync()
    {
        var expiredTransactions = await _context.PointTransactions
            .Include(p => p.Customer)
            .Where(p => p.TransactionType == "Earn" && p.ExpiresAt != null && p.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var transaction in expiredTransactions)
        {
            var expireTransaction = new PointTransaction
            {
                CustomerId = transaction.CustomerId,
                TransactionType = "Expire",
                Points = -transaction.Points,
                Description = $"點數到期 (原交易ID: {transaction.Id})",
                CreatedAt = DateTime.UtcNow
            };

            transaction.Customer.CurrentPoints -= transaction.Points;
            transaction.ExpiresAt = null; // 標記已處理

            _context.PointTransactions.Add(expireTransaction);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("點數過期處理完成 - 處理筆數: {Count}", expiredTransactions.Count);
    }
}
