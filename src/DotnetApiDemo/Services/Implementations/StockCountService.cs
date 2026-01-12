using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 庫存盤點服務實作
/// </summary>
/// <remarks>
/// 處理庫存盤點單的建立、執行、完成等作業
/// </remarks>
public class StockCountService : IStockCountService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockCountService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="context">資料庫上下文</param>
    /// <param name="logger">日誌服務</param>
    public StockCountService(ApplicationDbContext context, ILogger<StockCountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<StockCountListDto>> GetStockCountsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        StockCountStatus? status = null)
    {
        var query = _context.StockCounts
            .Include(sc => sc.Warehouse)
            .Include(sc => sc.Assignee)
            .AsQueryable();

        // 篩選條件
        if (warehouseId.HasValue)
        {
            query = query.Where(sc => sc.WarehouseId == warehouseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(sc => sc.Status == status.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(sc => sc.CountNo.Contains(request.Search) ||
                                      sc.Warehouse.Name.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "countno" => request.IsDescending ? query.OrderByDescending(sc => sc.CountNo) : query.OrderBy(sc => sc.CountNo),
            "countdate" => request.IsDescending ? query.OrderByDescending(sc => sc.CountDate) : query.OrderBy(sc => sc.CountDate),
            "status" => request.IsDescending ? query.OrderByDescending(sc => sc.Status) : query.OrderBy(sc => sc.Status),
            "warehouse" => request.IsDescending ? query.OrderByDescending(sc => sc.Warehouse.Name) : query.OrderBy(sc => sc.Warehouse.Name),
            _ => query.OrderByDescending(sc => sc.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sc => new StockCountListDto
            {
                Id = sc.Id,
                CountNo = sc.CountNo,
                WarehouseId = sc.WarehouseId,
                WarehouseName = sc.Warehouse.Name,
                CountType = sc.CountType,
                CountDate = sc.CountDate,
                TotalItems = sc.TotalItems,
                CountedItems = sc.CountedItems,
                VarianceItems = sc.VarianceItems,
                Status = sc.Status,
                AssigneeName = sc.Assignee.RealName ?? sc.Assignee.UserName ?? string.Empty,
                CreatedAt = sc.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<StockCountListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<StockCountDetailDto?> GetStockCountByIdAsync(int id)
    {
        return await _context.StockCounts
            .Where(sc => sc.Id == id)
            .Select(sc => new StockCountDetailDto
            {
                Id = sc.Id,
                CountNo = sc.CountNo,
                CountType = sc.CountType,
                WarehouseId = sc.WarehouseId,
                WarehouseName = sc.Warehouse.Name,
                CountDate = sc.CountDate,
                CountScope = sc.CountScope,
                FreezeInventory = sc.FreezeInventory,
                TotalItems = sc.TotalItems,
                CountedItems = sc.CountedItems,
                VarianceItems = sc.VarianceItems,
                VarianceAmount = sc.VarianceAmount,
                Status = sc.Status,
                AssignedTo = sc.AssignedTo,
                AssigneeName = sc.Assignee.RealName ?? sc.Assignee.UserName ?? string.Empty,
                ApproverName = sc.Approver != null ? (sc.Approver.RealName ?? sc.Approver.UserName) : null,
                ApprovedAt = sc.ApprovedAt,
                CompletedAt = sc.CompletedAt,
                Notes = sc.Notes,
                CreatedByName = sc.Creator.RealName ?? sc.Creator.UserName ?? string.Empty,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                Items = sc.Items.Select(i => new StockCountItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    SystemQuantity = i.SystemQuantity,
                    CountedQuantity = i.CountedQuantity,
                    VarianceQuantity = i.VarianceQuantity,
                    UnitCost = i.UnitCost,
                    VarianceAmount = i.VarianceAmount,
                    VarianceReason = i.VarianceReason,
                    CountedByName = i.Counter != null ? (i.Counter.RealName ?? i.Counter.UserName) : null,
                    CountedAt = i.CountedAt,
                    Notes = i.Notes
                })
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateStockCountAsync(CreateStockCountRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 產生盤點單號
            var countNo = await GenerateCountNoAsync();

            var stockCount = new StockCount
            {
                CountNo = countNo,
                CountType = request.CountType,
                WarehouseId = request.WarehouseId,
                CountDate = request.CountDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                CountScope = request.CountScope,
                FreezeInventory = request.FreezeInventory,
                AssignedTo = request.AssignedTo,
                Notes = request.Notes,
                Status = StockCountStatus.Draft,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockCounts.Add(stockCount);
            await _context.SaveChangesAsync();

            // 若有指定盤點明細，則加入指定的商品
            if (request.Items != null && request.Items.Any())
            {
                foreach (var itemRequest in request.Items)
                {
                    // 取得商品目前庫存
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == itemRequest.ProductId &&
                                                  i.WarehouseId == request.WarehouseId);

                    var product = await _context.Products.FindAsync(itemRequest.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("建立盤點明細失敗：找不到商品 - ProductId: {ProductId}", itemRequest.ProductId);
                        continue;
                    }

                    var countItem = new StockCountItem
                    {
                        CountId = stockCount.Id,
                        ProductId = itemRequest.ProductId,
                        SystemQuantity = inventory?.Quantity ?? 0,
                        UnitCost = product.CostPrice,
                        Notes = itemRequest.Notes
                    };

                    _context.StockCountItems.Add(countItem);
                }
            }
            else
            {
                // 自動產生盤點明細 (依據倉庫庫存)
                var inventories = await _context.Inventories
                    .Include(i => i.Product)
                    .Where(i => i.WarehouseId == request.WarehouseId)
                    .ToListAsync();

                foreach (var inventory in inventories)
                {
                    var countItem = new StockCountItem
                    {
                        CountId = stockCount.Id,
                        ProductId = inventory.ProductId,
                        SystemQuantity = inventory.Quantity,
                        UnitCost = inventory.Product.CostPrice
                    };

                    _context.StockCountItems.Add(countItem);
                }
            }

            await _context.SaveChangesAsync();

            // 更新盤點單統計
            stockCount.TotalItems = await _context.StockCountItems
                .CountAsync(i => i.CountId == stockCount.Id);
            stockCount.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立盤點單成功 - {CountNo}", countNo);
            return stockCount.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立盤點單失敗");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStockCountItemAsync(int stockCountId, int itemId, UpdateStockCountItemRequest request, int userId)
    {
        var stockCount = await _context.StockCounts.FindAsync(stockCountId);
        if (stockCount == null)
        {
            _logger.LogWarning("更新盤點明細失敗：找不到盤點單 - StockCountId: {StockCountId}", stockCountId);
            return false;
        }

        // 只有盤點中狀態才能更新明細
        if (stockCount.Status != StockCountStatus.InProgress)
        {
            _logger.LogWarning("更新盤點明細失敗：盤點單狀態不正確 - StockCountId: {StockCountId}, Status: {Status}",
                stockCountId, stockCount.Status);
            return false;
        }

        var item = await _context.StockCountItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CountId == stockCountId);

        if (item == null)
        {
            _logger.LogWarning("更新盤點明細失敗：找不到明細 - ItemId: {ItemId}", itemId);
            return false;
        }

        // 更新盤點結果
        var wasNotCounted = !item.CountedQuantity.HasValue;
        item.CountedQuantity = request.CountedQuantity;
        item.VarianceReason = request.VarianceReason;
        item.Notes = request.Notes;
        item.CountedBy = userId;
        item.CountedAt = DateTime.UtcNow;

        // 計算差異金額
        if (item.UnitCost.HasValue && item.VarianceQuantity.HasValue)
        {
            item.VarianceAmount = item.VarianceQuantity.Value * item.UnitCost.Value;
        }

        // 更新盤點單統計
        if (wasNotCounted)
        {
            stockCount.CountedItems += 1;
        }

        // 重新計算差異項數和金額
        var countedItems = await _context.StockCountItems
            .Where(i => i.CountId == stockCountId && i.CountedQuantity.HasValue)
            .ToListAsync();

        stockCount.VarianceItems = countedItems.Count(i => i.VarianceQuantity != 0);
        stockCount.VarianceAmount = countedItems
            .Where(i => i.VarianceAmount.HasValue)
            .Sum(i => i.VarianceAmount!.Value);
        stockCount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新盤點明細成功 - ItemId: {ItemId}", itemId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> StartStockCountAsync(int id, int userId)
    {
        var stockCount = await _context.StockCounts.FindAsync(id);
        if (stockCount == null)
        {
            _logger.LogWarning("開始盤點失敗：找不到盤點單 - StockCountId: {StockCountId}", id);
            return false;
        }

        // 只有草稿狀態才能開始盤點
        if (stockCount.Status != StockCountStatus.Draft)
        {
            _logger.LogWarning("開始盤點失敗：盤點單狀態不正確 - StockCountId: {StockCountId}, Status: {Status}",
                id, stockCount.Status);
            return false;
        }

        // 檢查是否有盤點明細
        var hasItems = await _context.StockCountItems.AnyAsync(i => i.CountId == id);
        if (!hasItems)
        {
            _logger.LogWarning("開始盤點失敗：盤點單無明細 - StockCountId: {StockCountId}", id);
            return false;
        }

        stockCount.Status = StockCountStatus.InProgress;
        stockCount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("盤點單已開始 - {CountNo}", stockCount.CountNo);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CompleteStockCountAsync(int id, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var stockCount = await _context.StockCounts
                .Include(sc => sc.Items)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (stockCount == null)
            {
                _logger.LogWarning("完成盤點失敗：找不到盤點單 - StockCountId: {StockCountId}", id);
                return false;
            }

            // 只有盤點中或待審核狀態才能完成
            if (stockCount.Status != StockCountStatus.InProgress &&
                stockCount.Status != StockCountStatus.PendingReview)
            {
                _logger.LogWarning("完成盤點失敗：盤點單狀態不正確 - StockCountId: {StockCountId}, Status: {Status}",
                    id, stockCount.Status);
                return false;
            }

            // 檢查是否全部盤點完成
            if (stockCount.CountedItems < stockCount.TotalItems)
            {
                _logger.LogWarning("完成盤點失敗：尚有項目未盤點 - StockCountId: {StockCountId}, Counted: {Counted}/{Total}",
                    id, stockCount.CountedItems, stockCount.TotalItems);
                return false;
            }

            // 產生庫存調整
            foreach (var item in stockCount.Items.Where(i => i.VarianceQuantity.HasValue && i.VarianceQuantity != 0))
            {
                // 產生調整單號
                var adjustmentNo = await GenerateAdjustmentNoAsync();

                // 取得或建立庫存記錄
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId &&
                                              i.WarehouseId == stockCount.WarehouseId);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        WarehouseId = stockCount.WarehouseId,
                        Quantity = 0,
                        ReservedQuantity = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Inventories.Add(inventory);
                    await _context.SaveChangesAsync();
                }

                var beforeQuantity = inventory.Quantity;
                var adjustQuantity = item.VarianceQuantity!.Value;
                var afterQuantity = inventory.Quantity + adjustQuantity;

                // 建立調整單
                var adjustment = new StockAdjustment
                {
                    AdjustmentNo = adjustmentNo,
                    WarehouseId = stockCount.WarehouseId,
                    AdjustmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ProductId = item.ProductId,
                    BeforeQuantity = beforeQuantity,
                    AfterQuantity = afterQuantity,
                    AdjustmentQuantity = adjustQuantity,
                    AdjustmentReason = item.VarianceReason ?? AdjustmentReason.Error,
                    ReasonNotes = $"盤點單 {stockCount.CountNo} 差異調整",
                    Status = AdjustmentStatus.Completed,
                    RequestedBy = userId,
                    ApprovedBy = userId,
                    ApprovedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockAdjustments.Add(adjustment);

                // 更新庫存
                inventory.Quantity = afterQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // 記錄異動
                var movement = new InventoryMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = stockCount.WarehouseId,
                    MovementType = adjustQuantity > 0
                        ? InventoryMovementType.AdjustIn
                        : InventoryMovementType.AdjustOut,
                    Quantity = Math.Abs(adjustQuantity),
                    BeforeQuantity = beforeQuantity,
                    AfterQuantity = afterQuantity,
                    ReferenceNo = stockCount.CountNo,
                    Notes = $"盤點調整 - {item.VarianceReason}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(movement);
            }

            // 更新盤點單狀態
            stockCount.Status = StockCountStatus.Completed;
            stockCount.ApprovedBy = userId;
            stockCount.ApprovedAt = DateTime.UtcNow;
            stockCount.CompletedAt = DateTime.UtcNow;
            stockCount.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("盤點單已完成並產生調整 - {CountNo}", stockCount.CountNo);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "完成盤點失敗");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelStockCountAsync(int id, int userId)
    {
        var stockCount = await _context.StockCounts.FindAsync(id);
        if (stockCount == null)
        {
            _logger.LogWarning("取消盤點失敗：找不到盤點單 - StockCountId: {StockCountId}", id);
            return false;
        }

        // 已完成或已取消的不能再取消
        if (stockCount.Status == StockCountStatus.Completed ||
            stockCount.Status == StockCountStatus.Cancelled)
        {
            _logger.LogWarning("取消盤點失敗：盤點單狀態不正確 - StockCountId: {StockCountId}, Status: {Status}",
                id, stockCount.Status);
            return false;
        }

        stockCount.Status = StockCountStatus.Cancelled;
        stockCount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("盤點單已取消 - {CountNo}", stockCount.CountNo);
        return true;
    }

    /// <summary>
    /// 產生盤點單號
    /// </summary>
    /// <returns>盤點單號 (格式: SC + 日期 + 序號)</returns>
    private async Task<string> GenerateCountNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"SC{today}";

        var lastCount = await _context.StockCounts
            .Where(sc => sc.CountNo.StartsWith(prefix))
            .OrderByDescending(sc => sc.CountNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastCount != null && lastCount.CountNo.Length > prefix.Length)
        {
            if (int.TryParse(lastCount.CountNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    /// <summary>
    /// 產生調整單號
    /// </summary>
    /// <returns>調整單號</returns>
    private async Task<string> GenerateAdjustmentNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ADJ{today}";

        var lastAdjustment = await _context.StockAdjustments
            .Where(a => a.AdjustmentNo.StartsWith(prefix))
            .OrderByDescending(a => a.AdjustmentNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastAdjustment != null && lastAdjustment.AdjustmentNo.Length > prefix.Length)
        {
            if (int.TryParse(lastAdjustment.AdjustmentNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
