using DotnetApiDemo.Models.DTOs.CashierShifts;
using DotnetApiDemo.Models.DTOs.Common;

namespace DotnetApiDemo.Services.Interfaces;

public interface ICashierShiftService
{
    Task<PaginatedResponse<CashierShiftListDto>> GetShiftsAsync(PaginationRequest request);
    Task<CashierShiftDetailDto?> GetShiftByIdAsync(int id);
    Task<CashierShiftDetailDto?> GetCurrentShiftAsync(int cashierId);
    Task<int?> OpenShiftAsync(OpenShiftRequest request, int cashierId);
    Task<bool> CloseShiftAsync(int id, CloseShiftRequest request);
}
