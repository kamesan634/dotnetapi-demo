namespace DotnetApiDemo.Models.Entities;

/// <summary>
/// 庫存調撥單明細實體
/// </summary>
/// <remarks>
/// 記錄調撥單中的各項商品
/// </remarks>
public class StockTransferItem
{
    /// <summary>
    /// 調撥明細 ID (主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 調撥單 ID
    /// </summary>
    public int TransferId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 申請數量
    /// </summary>
    public int RequestedQuantity { get; set; }

    /// <summary>
    /// 實際出庫數量
    /// </summary>
    public int? ShippedQuantity { get; set; }

    /// <summary>
    /// 實際入庫數量
    /// </summary>
    public int? ReceivedQuantity { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Notes { get; set; }

    // 導航屬性

    /// <summary>
    /// 所屬調撥單
    /// </summary>
    public virtual StockTransfer Transfer { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
