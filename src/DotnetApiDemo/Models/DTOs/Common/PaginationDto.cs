namespace DotnetApiDemo.Models.DTOs.Common;

/// <summary>
/// 分頁請求參數
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// 頁碼 (從 1 開始)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 每頁筆數 (最大 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : (value < 1 ? 10 : value);
    }

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序排列
    /// </summary>
    public bool IsDescending { get; set; } = false;

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? Search { get; set; }
}

/// <summary>
/// 分頁回應結果
/// </summary>
/// <typeparam name="T">資料型別</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// 資料列表
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// 總筆數
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 當前頁碼
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 總頁數
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// 是否有上一頁
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// 是否有下一頁
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
