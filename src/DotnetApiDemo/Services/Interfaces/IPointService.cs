using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Points;

namespace DotnetApiDemo.Services.Interfaces;

public interface IPointService
{
    Task<PaginatedResponse<PointTransactionListDto>> GetTransactionsAsync(PaginationRequest request, int? customerId = null);
    Task<PointBalanceDto?> GetBalanceAsync(int customerId);
    Task<bool> EarnPointsAsync(EarnPointsRequest request, int userId);
    Task<bool> RedeemPointsAsync(RedeemPointsRequest request, int userId);
    Task<bool> AdjustPointsAsync(AdjustPointsRequest request, int userId);
    Task ExpirePointsAsync();
}
