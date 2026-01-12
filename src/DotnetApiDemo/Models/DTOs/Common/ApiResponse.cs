namespace DotnetApiDemo.Models.DTOs.Common;

/// <summary>
/// API 統一回應格式
/// </summary>
/// <typeparam name="T">資料型別</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訊息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 資料
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 錯誤列表
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }

    /// <summary>
    /// 建立成功回應
    /// </summary>
    /// <param name="data">資料</param>
    /// <param name="message">訊息</param>
    /// <returns>API 回應</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "操作成功",
            Data = data
        };
    }

    /// <summary>
    /// 建立失敗回應
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    /// <param name="errors">錯誤列表</param>
    /// <returns>API 回應</returns>
    public static ApiResponse<T> FailResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// API 統一回應格式 (無資料)
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訊息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 錯誤列表
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }

    /// <summary>
    /// 建立成功回應
    /// </summary>
    /// <param name="message">訊息</param>
    /// <returns>API 回應</returns>
    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "操作成功"
        };
    }

    /// <summary>
    /// 建立失敗回應
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    /// <param name="errors">錯誤列表</param>
    /// <returns>API 回應</returns>
    public static ApiResponse FailResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
