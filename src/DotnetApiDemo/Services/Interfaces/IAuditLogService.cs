using DotnetApiDemo.Models.DTOs.AuditLogs;
using DotnetApiDemo.Models.DTOs.Common;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 稽核日誌服務介面
/// </summary>
public interface IAuditLogService
{
    Task<PaginatedResponse<AuditLogListDto>> GetLogsAsync(AuditLogQueryRequest request);
    Task<AuditLogDetailDto?> GetLogByIdAsync(int id);
    Task LogAsync(int? userId, string userName, string action, string entityType, string? entityId,
        string? oldValues, string? newValues, string? description, string? ipAddress, string? userAgent);
    Task<IEnumerable<string>> GetActionsAsync();
    Task<IEnumerable<string>> GetEntityTypesAsync();
}
