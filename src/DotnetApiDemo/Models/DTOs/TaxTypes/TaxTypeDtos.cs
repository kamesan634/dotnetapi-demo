namespace DotnetApiDemo.Models.DTOs.TaxTypes;

/// <summary>
/// 稅別資訊 DTO
/// </summary>
public class TaxTypeDto
{
    /// <summary>
    /// 稅別代碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 稅別代碼名稱
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// 稅別名稱
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 稅別說明
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// 稅率 (%)
    /// </summary>
    public decimal TaxRate { get; set; }
}
