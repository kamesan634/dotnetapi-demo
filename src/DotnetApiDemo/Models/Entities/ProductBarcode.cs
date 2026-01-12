namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 商品條碼實體
/// </summary>
/// <remarks>
/// 一個商品可以有多個條碼
/// </remarks>
public class ProductBarcode
{
    /// <summary>
    /// 條碼 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 條碼
    /// </summary>
    /// <remarks>
    /// EAN-13、EAN-8、UPC 等格式
    /// </remarks>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// 條碼類型
    /// </summary>
    /// <remarks>
    /// 如：EAN13、EAN8、UPC、CODE39 等
    /// </remarks>
    public string? BarcodeType { get; set; }

    /// <summary>
    /// 是否為主條碼
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 導航屬性

    /// <summary>
    /// 所屬商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
