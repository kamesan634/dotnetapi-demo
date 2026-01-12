namespace DotnetApiDemo.Services.Interfaces;

/// <summary>
/// 快取服務介面
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 取得快取資料
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    /// <param name="key">快取鍵</param>
    /// <returns>快取資料，若不存在則回傳 default</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 設定快取資料
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    /// <param name="key">快取鍵</param>
    /// <param name="value">快取值</param>
    /// <param name="expiration">過期時間（可選）</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 移除快取資料
    /// </summary>
    /// <param name="key">快取鍵</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// 檢查快取是否存在
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 取得或設定快取資料（若不存在則執行 factory 並快取）
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    /// <param name="key">快取鍵</param>
    /// <param name="factory">產生資料的方法</param>
    /// <param name="expiration">過期時間（可選）</param>
    /// <returns>快取資料</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// 依照模式移除快取（支援萬用字元）
    /// </summary>
    /// <param name="pattern">模式，例如 "products:*"</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// 設定字串快取
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">字串值</param>
    /// <param name="expiration">過期時間</param>
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);

    /// <summary>
    /// 取得字串快取
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>字串值</returns>
    Task<string?> GetStringAsync(string key);

    /// <summary>
    /// 設定 Hash 欄位
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="field">欄位名稱</param>
    /// <param name="value">值</param>
    Task HashSetAsync(string key, string field, string value);

    /// <summary>
    /// 取得 Hash 欄位
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="field">欄位名稱</param>
    /// <returns>值</returns>
    Task<string?> HashGetAsync(string key, string field);

    /// <summary>
    /// 取得 Hash 所有欄位
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>欄位與值的字典</returns>
    Task<Dictionary<string, string>> HashGetAllAsync(string key);

    /// <summary>
    /// 刪除 Hash 欄位
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="field">欄位名稱</param>
    Task HashDeleteAsync(string key, string field);

    /// <summary>
    /// 設定過期時間
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="expiration">過期時間</param>
    Task SetExpirationAsync(string key, TimeSpan expiration);

    /// <summary>
    /// 遞增計數器
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">遞增值（預設 1）</param>
    /// <returns>遞增後的值</returns>
    Task<long> IncrementAsync(string key, long value = 1);

    /// <summary>
    /// 推送至 List 左側
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">值</param>
    Task ListLeftPushAsync(string key, string value);

    /// <summary>
    /// 從 List 右側取出
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>值</returns>
    Task<string?> ListRightPopAsync(string key);

    /// <summary>
    /// 取得 List 長度
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>長度</returns>
    Task<long> ListLengthAsync(string key);

    /// <summary>
    /// 新增至 Set
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">值</param>
    Task SetAddAsync(string key, string value);

    /// <summary>
    /// 從 Set 移除
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">值</param>
    Task SetRemoveAsync(string key, string value);

    /// <summary>
    /// 取得 Set 所有成員
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>成員清單</returns>
    Task<IEnumerable<string>> SetMembersAsync(string key);

    /// <summary>
    /// 檢查 Set 是否包含成員
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">值</param>
    /// <returns>是否包含</returns>
    Task<bool> SetContainsAsync(string key, string value);

    /// <summary>
    /// 發布訊息至頻道
    /// </summary>
    /// <param name="channel">頻道名稱</param>
    /// <param name="message">訊息</param>
    Task PublishAsync(string channel, string message);
}
