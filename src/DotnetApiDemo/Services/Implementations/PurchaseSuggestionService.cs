using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 採購建議服務實作
/// </summary>
public class PurchaseSuggestionService : IPurchaseSuggestionService
{
    private readonly ApplicationDbContext _context;
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ILogger<PurchaseSuggestionService> _logger;

    public PurchaseSuggestionService(
        ApplicationDbContext context,
        IPurchaseOrderService purchaseOrderService,
        ILogger<PurchaseSuggestionService> logger)
    {
        _context = context;
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PurchaseSuggestionDto>> GetPurchaseSuggestionsAsync(
        PaginationRequest request,
        int? warehouseId = null,
        int? supplierId = null)
    {
        // 取得低於安全庫存的商品
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity < i.Product.SafetyStock && i.Product.IsActive);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 取得供應商報價資訊
        var lowStockItems = await query.ToListAsync();
        var productIds = lowStockItems.Select(i => i.ProductId).Distinct().ToList();

        var supplierPrices = await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Where(sp => productIds.Contains(sp.ProductId) &&
                         sp.EffectiveDate <= today &&
                         (sp.ExpiryDate == null || sp.ExpiryDate >= today) &&
                         sp.Supplier.IsActive)
            .ToListAsync();

        if (supplierId.HasValue)
        {
            // 只保留有指定供應商報價的商品
            var supplierProductIds = supplierPrices
                .Where(sp => sp.SupplierId == supplierId.Value)
                .Select(sp => sp.ProductId)
                .ToHashSet();
            lowStockItems = lowStockItems.Where(i => supplierProductIds.Contains(i.ProductId)).ToList();
        }

        // 取得最後採購日期
        var lastPurchaseDates = await _context.PurchaseOrderItems
            .Include(poi => poi.PurchaseOrder)
            .Where(poi => productIds.Contains(poi.ProductId))
            .GroupBy(poi => poi.ProductId)
            .Select(g => new { ProductId = g.Key, LastDate = g.Max(x => x.PurchaseOrder.OrderDate) })
            .ToDictionaryAsync(x => x.ProductId, x => x.LastDate);

        // 建立建議清單
        var suggestions = lowStockItems.Select(inventory =>
        {
            var shortage = inventory.Product.SafetyStock - inventory.Quantity;
            var suggestedQty = Math.Max(shortage, inventory.Product.MinOrderQuantity);

            // 取得優先供應商
            var preferredSupplier = supplierPrices
                .Where(sp => sp.ProductId == inventory.ProductId)
                .OrderByDescending(sp => sp.IsPrimary)
                .ThenBy(sp => sp.UnitPrice)
                .FirstOrDefault();

            // 計算緊急程度
            var urgencyLevel = "Normal";
            var stockRatio = (double)inventory.Quantity / inventory.Product.SafetyStock;
            if (inventory.Quantity <= 0)
                urgencyLevel = "Critical";
            else if (stockRatio <= 0.3)
                urgencyLevel = "Critical";
            else if (stockRatio <= 0.7)
                urgencyLevel = "Warning";

            return new PurchaseSuggestionDto
            {
                ProductId = inventory.ProductId,
                ProductSku = inventory.Product.Sku,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                CurrentStock = inventory.Quantity,
                SafetyStock = inventory.Product.SafetyStock,
                ShortageQuantity = shortage,
                SuggestedQuantity = suggestedQty,
                PreferredSupplierId = preferredSupplier?.SupplierId,
                PreferredSupplierName = preferredSupplier?.Supplier.Name,
                ReferencePrice = preferredSupplier?.UnitPrice,
                EstimatedAmount = preferredSupplier != null ? preferredSupplier.UnitPrice * suggestedQty : null,
                LastPurchaseDate = lastPurchaseDates.TryGetValue(inventory.ProductId, out var date)
                    ? date.ToDateTime(TimeOnly.MinValue)
                    : null,
                UrgencyLevel = urgencyLevel
            };
        }).ToList();

        // 搜尋過濾
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            suggestions = suggestions.Where(s =>
                s.ProductSku.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                s.ProductName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 排序
        suggestions = request.SortBy?.ToLower() switch
        {
            "sku" => request.IsDescending ? suggestions.OrderByDescending(s => s.ProductSku).ToList() : suggestions.OrderBy(s => s.ProductSku).ToList(),
            "name" => request.IsDescending ? suggestions.OrderByDescending(s => s.ProductName).ToList() : suggestions.OrderBy(s => s.ProductName).ToList(),
            "shortage" => request.IsDescending ? suggestions.OrderByDescending(s => s.ShortageQuantity).ToList() : suggestions.OrderBy(s => s.ShortageQuantity).ToList(),
            "urgency" => suggestions.OrderByDescending(s => s.UrgencyLevel == "Critical" ? 3 : s.UrgencyLevel == "Warning" ? 2 : 1).ToList(),
            _ => suggestions.OrderByDescending(s => s.UrgencyLevel == "Critical" ? 3 : s.UrgencyLevel == "Warning" ? 2 : 1).ToList()
        };

        var totalCount = suggestions.Count;
        var pagedSuggestions = suggestions
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResponse<PurchaseSuggestionDto>
        {
            Items = pagedSuggestions,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<int>> GeneratePurchaseOrdersFromSuggestionsAsync(
        GeneratePurchaseOrderRequest request,
        int userId)
    {
        var createdOrderIds = new List<int>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 取得建議資料
        var suggestionsQuery = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => request.ProductIds.Contains(i.ProductId) && i.Quantity < i.Product.SafetyStock);

        if (request.WarehouseId.HasValue)
        {
            suggestionsQuery = suggestionsQuery.Where(i => i.WarehouseId == request.WarehouseId.Value);
        }

        var inventories = await suggestionsQuery.ToListAsync();

        if (!inventories.Any())
        {
            _logger.LogWarning("產生採購單失敗：沒有符合條件的商品");
            return createdOrderIds;
        }

        // 取得供應商報價
        var productIds = inventories.Select(i => i.ProductId).Distinct().ToList();
        var supplierPrices = await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Where(sp => productIds.Contains(sp.ProductId) &&
                         sp.EffectiveDate <= today &&
                         (sp.ExpiryDate == null || sp.ExpiryDate >= today) &&
                         sp.Supplier.IsActive)
            .ToListAsync();

        // 取得預設倉庫
        var defaultWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault);
        var defaultWarehouseId = defaultWarehouse?.Id ?? inventories.First().WarehouseId;

        if (request.GroupBySupplier)
        {
            // 依供應商分組產生採購單
            var supplierGroups = new Dictionary<int, List<(int ProductId, int Quantity, decimal UnitPrice)>>();

            foreach (var inventory in inventories)
            {
                var shortage = inventory.Product.SafetyStock - inventory.Quantity;
                var suggestedQty = Math.Max(shortage, inventory.Product.MinOrderQuantity);

                var preferredSupplier = supplierPrices
                    .Where(sp => sp.ProductId == inventory.ProductId)
                    .OrderByDescending(sp => sp.IsPrimary)
                    .ThenBy(sp => sp.UnitPrice)
                    .FirstOrDefault();

                if (preferredSupplier == null)
                {
                    _logger.LogWarning("商品 {ProductId} 無供應商報價，跳過", inventory.ProductId);
                    continue;
                }

                if (!supplierGroups.ContainsKey(preferredSupplier.SupplierId))
                {
                    supplierGroups[preferredSupplier.SupplierId] = new List<(int, int, decimal)>();
                }

                supplierGroups[preferredSupplier.SupplierId].Add(
                    (inventory.ProductId, suggestedQty, preferredSupplier.UnitPrice));
            }

            // 為每個供應商建立採購單
            foreach (var group in supplierGroups)
            {
                var createRequest = new CreatePurchaseOrderRequest
                {
                    SupplierId = group.Key,
                    WarehouseId = defaultWarehouseId,
                    ExpectedDeliveryDate = request.ExpectedDeliveryDate ?? today.AddDays(7),
                    Notes = request.Notes ?? "由採購建議自動產生",
                    Items = group.Value.Select(item => new CreatePurchaseOrderItemRequest
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                var orderId = await _purchaseOrderService.CreatePurchaseOrderAsync(createRequest, userId);
                if (orderId.HasValue)
                {
                    createdOrderIds.Add(orderId.Value);
                    _logger.LogInformation("產生採購單成功 - OrderId: {OrderId}, SupplierId: {SupplierId}",
                        orderId.Value, group.Key);
                }
            }
        }
        else
        {
            // 產生單一採購單（取第一個供應商）
            var items = new List<CreatePurchaseOrderItemRequest>();
            int? supplierId = null;

            foreach (var inventory in inventories)
            {
                var shortage = inventory.Product.SafetyStock - inventory.Quantity;
                var suggestedQty = Math.Max(shortage, inventory.Product.MinOrderQuantity);

                var preferredSupplier = supplierPrices
                    .Where(sp => sp.ProductId == inventory.ProductId)
                    .OrderByDescending(sp => sp.IsPrimary)
                    .ThenBy(sp => sp.UnitPrice)
                    .FirstOrDefault();

                if (preferredSupplier == null) continue;

                supplierId ??= preferredSupplier.SupplierId;

                items.Add(new CreatePurchaseOrderItemRequest
                {
                    ProductId = inventory.ProductId,
                    Quantity = suggestedQty,
                    UnitPrice = preferredSupplier.UnitPrice
                });
            }

            if (supplierId.HasValue && items.Any())
            {
                var createRequest = new CreatePurchaseOrderRequest
                {
                    SupplierId = supplierId.Value,
                    WarehouseId = defaultWarehouseId,
                    ExpectedDeliveryDate = request.ExpectedDeliveryDate ?? today.AddDays(7),
                    Notes = request.Notes ?? "由採購建議自動產生",
                    Items = items
                };

                var orderId = await _purchaseOrderService.CreatePurchaseOrderAsync(createRequest, userId);
                if (orderId.HasValue)
                {
                    createdOrderIds.Add(orderId.Value);
                }
            }
        }

        _logger.LogInformation("從採購建議產生 {Count} 張採購單", createdOrderIds.Count);
        return createdOrderIds;
    }

    /// <inheritdoc />
    public async Task<PurchaseSuggestionSummaryDto> GetSuggestionSummaryAsync()
    {
        var lowStockItems = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity < i.Product.SafetyStock && i.Product.IsActive)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var productIds = lowStockItems.Select(i => i.ProductId).Distinct().ToList();

        var supplierPrices = await _context.SupplierPrices
            .Include(sp => sp.Supplier)
            .Where(sp => productIds.Contains(sp.ProductId) &&
                         sp.EffectiveDate <= today &&
                         (sp.ExpiryDate == null || sp.ExpiryDate >= today) &&
                         sp.Supplier.IsActive)
            .ToListAsync();

        int criticalCount = 0;
        int warningCount = 0;
        int normalCount = 0;
        decimal estimatedTotal = 0;
        var supplierIds = new HashSet<int>();

        foreach (var inventory in lowStockItems)
        {
            var stockRatio = inventory.Product.SafetyStock > 0
                ? (double)inventory.Quantity / inventory.Product.SafetyStock
                : 0;

            if (inventory.Quantity <= 0 || stockRatio <= 0.3)
                criticalCount++;
            else if (stockRatio <= 0.7)
                warningCount++;
            else
                normalCount++;

            var shortage = inventory.Product.SafetyStock - inventory.Quantity;
            var suggestedQty = Math.Max(shortage, inventory.Product.MinOrderQuantity);

            var preferredSupplier = supplierPrices
                .Where(sp => sp.ProductId == inventory.ProductId)
                .OrderByDescending(sp => sp.IsPrimary)
                .ThenBy(sp => sp.UnitPrice)
                .FirstOrDefault();

            if (preferredSupplier != null)
            {
                estimatedTotal += preferredSupplier.UnitPrice * suggestedQty;
                supplierIds.Add(preferredSupplier.SupplierId);
            }
        }

        return new PurchaseSuggestionSummaryDto
        {
            TotalProductCount = lowStockItems.Count,
            CriticalCount = criticalCount,
            WarningCount = warningCount,
            NormalCount = normalCount,
            EstimatedTotalAmount = estimatedTotal,
            SupplierCount = supplierIds.Count
        };
    }
}
