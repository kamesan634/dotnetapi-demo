using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.CashierShifts;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class CashierShiftService : ICashierShiftService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CashierShiftService> _logger;

    public CashierShiftService(ApplicationDbContext context, ILogger<CashierShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<CashierShiftListDto>> GetShiftsAsync(PaginationRequest request)
    {
        var query = _context.CashierShifts
            .Include(s => s.Store)
            .Include(s => s.Cashier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.ShiftNumber.Contains(request.Search) ||
                                      s.Cashier.RealName.Contains(request.Search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.StartTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new CashierShiftListDto
            {
                Id = s.Id,
                ShiftNumber = s.ShiftNumber,
                StoreName = s.Store.Name,
                CashierName = s.Cashier.RealName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                TotalSales = s.TotalSales,
                TotalTransactions = s.TotalTransactions,
                Status = s.Status
            })
            .ToListAsync();

        return new PaginatedResponse<CashierShiftListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<CashierShiftDetailDto?> GetShiftByIdAsync(int id)
    {
        return await _context.CashierShifts
            .Include(s => s.Store)
            .Include(s => s.Cashier)
            .Where(s => s.Id == id)
            .Select(s => new CashierShiftDetailDto
            {
                Id = s.Id,
                StoreId = s.StoreId,
                StoreName = s.Store.Name,
                CashierId = s.CashierId,
                CashierName = s.Cashier.RealName,
                ShiftNumber = s.ShiftNumber,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                OpeningAmount = s.OpeningAmount,
                ClosingAmount = s.ClosingAmount,
                ExpectedAmount = s.ExpectedAmount,
                Difference = s.Difference,
                TotalTransactions = s.TotalTransactions,
                TotalSales = s.TotalSales,
                TotalRefunds = s.TotalRefunds,
                Status = s.Status,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CashierShiftDetailDto?> GetCurrentShiftAsync(int cashierId)
    {
        var shift = await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.CashierId == cashierId && s.Status == "Open");

        if (shift == null) return null;
        return await GetShiftByIdAsync(shift.Id);
    }

    public async Task<int?> OpenShiftAsync(OpenShiftRequest request, int cashierId)
    {
        if (await _context.CashierShifts.AnyAsync(s => s.CashierId == cashierId && s.Status == "Open"))
        {
            _logger.LogWarning("開班失敗：收銀員已有未結班別 - CashierId: {CashierId}", cashierId);
            return null;
        }

        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.CashierShifts.CountAsync(s => s.ShiftNumber.StartsWith($"SH{today}"));
        var shiftNumber = $"SH{today}{(count + 1):D4}";

        var shift = new CashierShift
        {
            StoreId = request.StoreId,
            CashierId = cashierId,
            ShiftNumber = shiftNumber,
            StartTime = DateTime.UtcNow,
            OpeningAmount = request.OpeningAmount,
            Status = "Open",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.CashierShifts.Add(shift);
        await _context.SaveChangesAsync();

        return shift.Id;
    }

    public async Task<bool> CloseShiftAsync(int id, CloseShiftRequest request)
    {
        var shift = await _context.CashierShifts.FindAsync(id);
        if (shift == null || shift.Status != "Open") return false;

        // 計算班別銷售
        var salesData = await _context.Orders
            .Where(o => o.CreatedAt >= shift.StartTime && o.CreatedAt <= DateTime.UtcNow)
            .GroupBy(o => 1)
            .Select(g => new { Total = g.Sum(o => o.TotalAmount), Count = g.Count() })
            .FirstOrDefaultAsync();

        shift.EndTime = DateTime.UtcNow;
        shift.ClosingAmount = request.ClosingAmount;
        shift.TotalSales = salesData?.Total ?? 0;
        shift.TotalTransactions = salesData?.Count ?? 0;
        shift.ExpectedAmount = shift.OpeningAmount + shift.TotalSales - shift.TotalRefunds;
        shift.Difference = request.ClosingAmount - shift.ExpectedAmount;
        shift.Status = "Closed";
        shift.Notes = request.Notes ?? shift.Notes;
        shift.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
