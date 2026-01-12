using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 庫存服務實作
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<InventoryListDto>> GetInventoriesAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? productId = null)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(i => i.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(i => i.Product.Name.Contains(request.Search) ||
                                     i.Product.Sku.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "product" => request.IsDescending ? query.OrderByDescending(i => i.Product.Name) : query.OrderBy(i => i.Product.Name),
            "quantity" => request.IsDescending ? query.OrderByDescending(i => i.Quantity) : query.OrderBy(i => i.Quantity),
            "warehouse" => request.IsDescending ? query.OrderByDescending(i => i.Warehouse.Name) : query.OrderBy(i => i.Warehouse.Name),
            _ => query.OrderBy(i => i.Product.Sku)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InventoryListDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductSku = i.Product.Sku,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                Quantity = i.Quantity,
                ReservedQuantity = i.ReservedQuantity,
                SafetyStock = i.Product.SafetyStock
            })
            .ToListAsync();

        return new PaginatedResponse<InventoryListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryListDto>> GetProductInventoriesAsync(int productId)
    {
        return await _context.Inventories
            .Where(i => i.ProductId == productId)
            .Select(i => new InventoryListDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductSku = i.Product.Sku,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                Quantity = i.Quantity,
                ReservedQuantity = i.ReservedQuantity,
                SafetyStock = i.Product.SafetyStock
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryListDto>> GetWarehouseInventoriesAsync(int warehouseId)
    {
        return await _context.Inventories
            .Where(i => i.WarehouseId == warehouseId)
            .Select(i => new InventoryListDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductSku = i.Product.Sku,
                ProductName = i.Product.Name,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                Quantity = i.Quantity,
                ReservedQuantity = i.ReservedQuantity,
                SafetyStock = i.Product.SafetyStock
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int? warehouseId = null)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity < i.Product.SafetyStock);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        return await query
            .Select(i => new LowStockAlertDto
            {
                ProductId = i.ProductId,
                ProductSku = i.Product.Sku,
                ProductName = i.Product.Name,
                WarehouseName = i.Warehouse.Name,
                CurrentStock = i.Quantity,
                SafetyStock = i.Product.SafetyStock
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<InventoryMovementDto>> GetInventoryMovementsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? productId = null)
    {
        var query = _context.InventoryMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.Creator)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(m => m.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(m => m.ProductId == productId.Value);
        }

        query = query.OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ProductSku = m.Product.Sku,
                ProductName = m.Product.Name,
                WarehouseName = m.Warehouse.Name,
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                BeforeQuantity = m.BeforeQuantity,
                AfterQuantity = m.AfterQuantity,
                ReferenceNo = m.ReferenceNo,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt,
                CreatedByName = m.Creator != null ? m.Creator.RealName ?? m.Creator.UserName : null
            })
            .ToListAsync();

        return new PaginatedResponse<InventoryMovementDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<int?> CreateStockAdjustmentAsync(StockAdjustmentRequest request, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            int? firstAdjustmentId = null;

            foreach (var item in request.Items)
            {
                // 取得或建立庫存記錄
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId &&
                                              i.WarehouseId == request.WarehouseId);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        WarehouseId = request.WarehouseId,
                        Quantity = 0,
                        ReservedQuantity = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Inventories.Add(inventory);
                    await _context.SaveChangesAsync();
                }

                var beforeQuantity = inventory.Quantity;
                var afterQuantity = inventory.Quantity + item.AdjustQuantity;

                if (afterQuantity < 0)
                {
                    _logger.LogWarning("庫存調整失敗：調整後數量不可為負 - ProductId: {ProductId}", item.ProductId);
                    await transaction.RollbackAsync();
                    return null;
                }

                // 產生調整單號
                var adjustmentNo = await GenerateAdjustmentNoAsync();

                // 建立調整單 (每個商品一筆調整記錄)
                var adjustment = new StockAdjustment
                {
                    AdjustmentNo = adjustmentNo,
                    WarehouseId = request.WarehouseId,
                    AdjustmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ProductId = item.ProductId,
                    BeforeQuantity = beforeQuantity,
                    AfterQuantity = afterQuantity,
                    AdjustmentQuantity = item.AdjustQuantity,
                    AdjustmentReason = request.Reason,
                    ReasonNotes = request.Notes ?? string.Empty,
                    Status = AdjustmentStatus.Completed,
                    RequestedBy = userId,
                    ApprovedBy = userId,
                    ApprovedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockAdjustments.Add(adjustment);
                await _context.SaveChangesAsync();

                if (firstAdjustmentId == null)
                {
                    firstAdjustmentId = adjustment.Id;
                }

                // 更新庫存
                inventory.Quantity = afterQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // 記錄異動
                var movement = new InventoryMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = request.WarehouseId,
                    MovementType = item.AdjustQuantity > 0
                        ? InventoryMovementType.AdjustIn
                        : InventoryMovementType.AdjustOut,
                    Quantity = Math.Abs(item.AdjustQuantity),
                    BeforeQuantity = beforeQuantity,
                    AfterQuantity = afterQuantity,
                    ReferenceNo = adjustmentNo,
                    Notes = item.Notes,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(movement);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("建立庫存調整單成功");
            return firstAdjustmentId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "建立庫存調整單失敗");
            throw;
        }
    }

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

    /// <inheritdoc />
    public async Task<IEnumerable<ReplenishmentSuggestionDto>> GetReplenishmentSuggestionsAsync(int? warehouseId = null)
    {
        // 查詢庫存低於安全庫存的商品
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity < i.Product.SafetyStock);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        var lowStockItems = await query.ToListAsync();

        // 取得相關商品的供應商報價 (優先取得主要供應商)
        var productIds = lowStockItems.Select(i => i.ProductId).Distinct().ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var supplierPrices = await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Where(sp => productIds.Contains(sp.ProductId) &&
                         sp.EffectiveDate <= today &&
                         (sp.ExpiryDate == null || sp.ExpiryDate >= today) &&
                         sp.Supplier.IsActive)
            .OrderByDescending(sp => sp.IsPrimary)
            .ThenBy(sp => sp.UnitPrice)
            .ToListAsync();

        var suggestions = lowStockItems.Select(inventory =>
        {
            // 取得該商品的建議供應商 (優先主要供應商，否則取最低價)
            var preferredPrice = supplierPrices
                .FirstOrDefault(sp => sp.ProductId == inventory.ProductId);

            // 計算建議補貨數量 (安全庫存 * 2 - 現有庫存)
            var suggestedQuantity = Math.Max(0, inventory.Product.SafetyStock * 2 - inventory.Quantity);

            return new ReplenishmentSuggestionDto
            {
                ProductId = inventory.ProductId,
                ProductSku = inventory.Product.Sku,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                CurrentStock = inventory.Quantity,
                SafetyStock = inventory.Product.SafetyStock,
                SuggestedQuantity = suggestedQuantity,
                PreferredSupplierId = preferredPrice?.SupplierId,
                PreferredSupplierName = preferredPrice?.Supplier.Name,
                LastPurchasePrice = preferredPrice?.UnitPrice
            };
        })
        .OrderByDescending(s => s.SafetyStock - s.CurrentStock) // 按缺口數量排序
        .ToList();

        return suggestions;
    }
}

/// <summary>
/// 庫存調撥服務實作
/// </summary>
public class StockTransferService : IStockTransferService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockTransferService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public StockTransferService(ApplicationDbContext context, ILogger<StockTransferService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<StockTransferDto>> GetStockTransfersAsync(PaginationRequest request)
    {
        var query = _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.TransferNo.Contains(request.Search));
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new StockTransferDto
            {
                Id = t.Id,
                TransferNo = t.TransferNo,
                FromWarehouseName = t.FromWarehouse.Name,
                ToWarehouseName = t.ToWarehouse.Name,
                Status = t.Status,
                TransferDate = t.RequestDate,
                TotalQuantity = t.TotalQuantity,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<StockTransferDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<StockTransferDetailDto?> GetStockTransferByIdAsync(int id)
    {
        return await _context.StockTransfers
            .Where(t => t.Id == id)
            .Select(t => new StockTransferDetailDto
            {
                Id = t.Id,
                TransferNo = t.TransferNo,
                FromWarehouseId = t.FromWarehouseId,
                FromWarehouseName = t.FromWarehouse.Name,
                ToWarehouseId = t.ToWarehouseId,
                ToWarehouseName = t.ToWarehouse.Name,
                Status = t.Status,
                TransferDate = t.RequestDate,
                TotalQuantity = t.TotalQuantity,
                Notes = t.Notes,
                Items = t.Items.Select(i => new StockTransferItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    Quantity = i.RequestedQuantity,
                    Notes = i.Notes
                }),
                CreatedByName = t.Requester.RealName ?? t.Requester.UserName,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateStockTransferAsync(CreateStockTransferRequest request, int userId)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
        {
            _logger.LogWarning("建立調撥單失敗：來源與目的倉庫相同");
            return null;
        }

        var transferNo = await GenerateTransferNoAsync();

        var transfer = new StockTransfer
        {
            TransferNo = transferNo,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            RequestDate = request.TransferDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = StockTransferStatus.Draft,
            Notes = request.Notes,
            RequestedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductId = item.ProductId,
                RequestedQuantity = item.Quantity,
                Notes = item.Notes
            });
        }

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立調撥單成功 - {TransferNo}", transferNo);
        return transfer.Id;
    }

    /// <inheritdoc />
    public async Task<bool> ApproveStockTransferAsync(int id, int userId)
    {
        var transfer = await _context.StockTransfers.FindAsync(id);
        if (transfer == null || transfer.Status != StockTransferStatus.Draft)
        {
            return false;
        }

        transfer.Status = StockTransferStatus.Approved;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ShipStockTransferAsync(int id, int userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null || transfer.Status != StockTransferStatus.Approved)
        {
            return false;
        }

        // 扣除來源倉庫庫存
        foreach (var item in transfer.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId &&
                                          i.WarehouseId == transfer.FromWarehouseId);

            if (inventory == null || inventory.Quantity < item.RequestedQuantity)
            {
                _logger.LogWarning("調撥出庫失敗：庫存不足 - ProductId: {ProductId}", item.ProductId);
                return false;
            }

            inventory.Quantity -= item.RequestedQuantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            _context.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = item.ProductId,
                WarehouseId = transfer.FromWarehouseId,
                MovementType = InventoryMovementType.TransferOut,
                Quantity = item.RequestedQuantity,
                BeforeQuantity = inventory.Quantity + item.RequestedQuantity,
                AfterQuantity = inventory.Quantity,
                ReferenceNo = transfer.TransferNo,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        transfer.Status = StockTransferStatus.InTransit;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReceiveStockTransferAsync(int id, int userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null || transfer.Status != StockTransferStatus.InTransit)
        {
            return false;
        }

        // 增加目的倉庫庫存
        foreach (var item in transfer.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId &&
                                          i.WarehouseId == transfer.ToWarehouseId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.ToWarehouseId,
                    Quantity = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }

            var beforeQuantity = inventory.Quantity;
            inventory.Quantity += item.RequestedQuantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            _context.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = item.ProductId,
                WarehouseId = transfer.ToWarehouseId,
                MovementType = InventoryMovementType.TransferIn,
                Quantity = item.RequestedQuantity,
                BeforeQuantity = beforeQuantity,
                AfterQuantity = inventory.Quantity,
                ReferenceNo = transfer.TransferNo,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        transfer.Status = StockTransferStatus.Completed;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CancelStockTransferAsync(int id, int userId)
    {
        var transfer = await _context.StockTransfers.FindAsync(id);
        if (transfer == null ||
            transfer.Status == StockTransferStatus.Completed ||
            transfer.Status == StockTransferStatus.Cancelled)
        {
            return false;
        }

        transfer.Status = StockTransferStatus.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateTransferNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"TRF{today}";

        var lastTransfer = await _context.StockTransfers
            .Where(t => t.TransferNo.StartsWith(prefix))
            .OrderByDescending(t => t.TransferNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTransfer != null && lastTransfer.TransferNo.Length > prefix.Length)
        {
            if (int.TryParse(lastTransfer.TransferNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
