using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;

namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 會員等級服務介面
/// </summary>
public interface ICustomerLevelService
{
    /// <summary>
    /// 取得會員等級列表
    /// </summary>
    /// <returns>會員等級列表</returns>
    Task<IEnumerable<CustomerLevelListDto>> GetCustomerLevelsAsync();

    /// <summary>
    /// 建立會員等級
    /// </summary>
    /// <param name="request">建立會員等級請求</param>
    /// <returns>建立的等級 ID</returns>
    Task<int?> CreateCustomerLevelAsync(CreateCustomerLevelRequest request);
}

/// <summary>
/// 客戶/會員服務介面
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// 取得客戶列表
    /// </summary>
    /// <param name="request">分頁參數</param>
    /// <param name="levelId">會員等級 ID (可選)</param>
    /// <returns>分頁客戶列表</returns>
    Task<PaginatedResponse<CustomerListDto>> GetCustomersAsync(PaginationRequest request, int? levelId = null);

    /// <summary>
    /// 取得客戶詳細資訊
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <returns>客戶詳細資訊</returns>
    Task<CustomerDetailDto?> GetCustomerByIdAsync(int id);

    /// <summary>
    /// 根據會員編號取得客戶
    /// </summary>
    /// <param name="memberNo">會員編號</param>
    /// <returns>客戶詳細資訊</returns>
    Task<CustomerDetailDto?> GetCustomerByMemberNoAsync(string memberNo);

    /// <summary>
    /// 根據電話取得客戶
    /// </summary>
    /// <param name="phone">電話</param>
    /// <returns>客戶詳細資訊</returns>
    Task<CustomerDetailDto?> GetCustomerByPhoneAsync(string phone);

    /// <summary>
    /// 建立客戶
    /// </summary>
    /// <param name="request">建立客戶請求</param>
    /// <returns>建立的客戶 ID</returns>
    Task<int?> CreateCustomerAsync(CreateCustomerRequest request);

    /// <summary>
    /// 更新客戶
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <param name="request">更新客戶請求</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateCustomerAsync(int id, UpdateCustomerRequest request);

    /// <summary>
    /// 刪除客戶
    /// </summary>
    /// <param name="id">客戶 ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteCustomerAsync(int id);
}
