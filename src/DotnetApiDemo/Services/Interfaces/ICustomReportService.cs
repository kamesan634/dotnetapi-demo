using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;

namespace DotnetApiDemo.Services.Interfaces;

public interface ICustomReportService
{
    // Custom Reports
    Task<PaginatedResponse<CustomReportListDto>> GetCustomReportsAsync(PaginationRequest request, int userId, bool includePublic = true);
    Task<CustomReportDetailDto?> GetCustomReportByIdAsync(int id);
    Task<int?> CreateCustomReportAsync(CreateCustomReportRequest request, int userId);
    Task<bool> UpdateCustomReportAsync(int id, UpdateCustomReportRequest request, int userId);
    Task<bool> DeleteCustomReportAsync(int id, int userId);
    Task<CustomReportResultDto?> RunCustomReportAsync(int id, RunCustomReportRequest? request = null);
    Task<byte[]?> ExportCustomReportAsync(int id, string format, RunCustomReportRequest? request = null);

    // Scheduled Reports
    Task<PaginatedResponse<ScheduledReportListDto>> GetScheduledReportsAsync(PaginationRequest request, int userId);
    Task<ScheduledReportDetailDto?> GetScheduledReportByIdAsync(int id);
    Task<int?> CreateScheduledReportAsync(CreateScheduledReportRequest request, int userId);
    Task<bool> UpdateScheduledReportAsync(int id, UpdateScheduledReportRequest request, int userId);
    Task<bool> DeleteScheduledReportAsync(int id, int userId);
    Task<bool> RunScheduledReportNowAsync(int id);
    Task<IEnumerable<ScheduledReportHistoryDto>> GetScheduledReportHistoryAsync(int id, int limit = 10);
}
