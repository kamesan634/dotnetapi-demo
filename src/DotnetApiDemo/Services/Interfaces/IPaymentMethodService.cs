using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.PaymentMethods;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 付款方式服務介面
/// </summary>
public interface IPaymentMethodService
{
    /// <summary>
    /// 取得付款方式列表
    /// </summary>
    Task<PaginatedResponse<PaymentMethodListDto>> GetPaymentMethodsAsync(PaginationRequest request);

    /// <summary>
    /// 取得所有啟用的付款方式
    /// </summary>
    Task<IEnumerable<PaymentMethodListDto>> GetActivePaymentMethodsAsync();

    /// <summary>
    /// 取得付款方式詳細資訊
    /// </summary>
    Task<PaymentMethodDetailDto?> GetPaymentMethodByIdAsync(int id);

    /// <summary>
    /// 依代碼取得付款方式
    /// </summary>
    Task<PaymentMethodDetailDto?> GetPaymentMethodByCodeAsync(string code);

    /// <summary>
    /// 建立付款方式
    /// </summary>
    Task<int?> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);

    /// <summary>
    /// 更新付款方式
    /// </summary>
    Task<bool> UpdatePaymentMethodAsync(int id, UpdatePaymentMethodRequest request);

    /// <summary>
    /// 刪除付款方式
    /// </summary>
    Task<bool> DeletePaymentMethodAsync(int id);
}
