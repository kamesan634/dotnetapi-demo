using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.AuditLogs;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AuditLogListDto>> GetLogsAsync(AuditLogQueryRequest request)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.EndDate.Value);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogListDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Description = a.Description,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<AuditLogListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<AuditLogDetailDto?> GetLogByIdAsync(int id)
    {
        return await _context.AuditLogs
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDetailDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Description = a.Description,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task LogAsync(int? userId, string userName, string action, string entityType, string? entityId,
        string? oldValues, string? newValues, string? description, string? ipAddress, string? userAgent)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetActionsAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetEntityTypesAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.EntityType)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
    }
}
