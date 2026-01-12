using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 商品分類服務實作
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CategoryService> _logger;

    private const string CategoryTreeCacheKey = "cache:categories:tree";
    private static readonly TimeSpan CategoryTreeCacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 建構函式
    /// </summary>
    public CategoryService(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<CategoryService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<CategoryListDto>> GetCategoriesAsync(PaginationRequest request)
    {
        var query = _context.Categories.AsQueryable();

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Name.Contains(request.Search) || c.Code.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "code" => request.IsDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "name" => request.IsDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "sortorder" => request.IsDescending ? query.OrderByDescending(c => c.SortOrder) : query.OrderBy(c => c.SortOrder),
            _ => query.OrderBy(c => c.SortOrder).ThenBy(c => c.Code)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                ChildCount = c.Children.Count,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        return new PaginatedResponse<CategoryListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        // 嘗試從快取取得
        var cached = await _cacheService.GetAsync<List<CategoryTreeDto>>(CategoryTreeCacheKey);
        if (cached != null)
        {
            _logger.LogDebug("從快取取得分類樹");
            return cached;
        }

        var categories = await _context.Categories
            .Where(c => c.IsActive && c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                SortOrder = c.SortOrder,
                Children = c.Children
                    .Where(child => child.IsActive)
                    .OrderBy(child => child.SortOrder)
                    .Select(child => new CategoryTreeDto
                    {
                        Id = child.Id,
                        Code = child.Code,
                        Name = child.Name,
                        SortOrder = child.SortOrder,
                        Children = Enumerable.Empty<CategoryTreeDto>()
                    })
            })
            .ToListAsync();

        // 寫入快取
        await _cacheService.SetAsync(CategoryTreeCacheKey, categories, CategoryTreeCacheDuration);
        _logger.LogDebug("分類樹已寫入快取，TTL: {Duration} 分鐘", CategoryTreeCacheDuration.TotalMinutes);

        return categories;
    }

    /// <inheritdoc />
    public async Task<CategoryListDto?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                ChildCount = c.Children.Count,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // 檢查代碼是否重複
        if (await _context.Categories.AnyAsync(c => c.Code == request.Code))
        {
            _logger.LogWarning("建立分類失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var category = new Category
        {
            Code = request.Code,
            Name = request.Name,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // 清除分類樹快取
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);

        _logger.LogInformation("建立分類成功 - {Code}: {Name}", category.Code, category.Name);
        return category.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name;

        if (request.ParentId.HasValue)
            category.ParentId = request.ParentId;

        if (request.SortOrder.HasValue)
            category.SortOrder = request.SortOrder.Value;

        if (request.Description != null)
            category.Description = request.Description;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 清除分類樹快取
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);

        _logger.LogInformation("更新分類成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Children)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return false;
        }

        // 檢查是否有子分類或商品
        if (category.Children.Any() || category.Products.Any())
        {
            _logger.LogWarning("刪除分類失敗：存在子分類或商品 - Id: {Id}", id);
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        // 清除分類樹快取
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);

        _logger.LogInformation("刪除分類成功 - Id: {Id}", id);
        return true;
    }
}

/// <summary>
/// 商品服務實作
/// </summary>
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<ProductListDto>> GetProductsAsync(PaginationRequest request, int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Inventories)
            .AsQueryable();

        // 分類篩選
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) ||
                                     p.Sku.Contains(request.Search));
        }

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "sku" => request.IsDescending ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
            "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.IsDescending ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice),
            _ => query.OrderBy(p => p.Sku)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                CategoryName = p.Category.Name,
                UnitName = p.Unit.Name,
                SellingPrice = p.SellingPrice,
                Cost = p.CostPrice,
                IsActive = p.IsActive,
                TotalStock = p.Inventories.Sum(i => i.Quantity)
            })
            .ToListAsync();

        return new PaginatedResponse<ProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
    {
        return await GetProductDetailAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<ProductDetailDto?> GetProductBySkuAsync(string sku)
    {
        return await GetProductDetailAsync(p => p.Sku == sku);
    }

    /// <inheritdoc />
    public async Task<ProductDetailDto?> GetProductByBarcodeAsync(string barcode)
    {
        var productId = await _context.ProductBarcodes
            .Where(pb => pb.Barcode == barcode)
            .Select(pb => pb.ProductId)
            .FirstOrDefaultAsync();

        if (productId == 0)
        {
            return null;
        }

        return await GetProductByIdAsync(productId);
    }

    private async Task<ProductDetailDto?> GetProductDetailAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate)
    {
        return await _context.Products
            .Where(predicate)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                UnitId = p.UnitId,
                UnitName = p.Unit.Name,
                TaxType = p.TaxType,
                SellingPrice = p.SellingPrice,
                Cost = p.CostPrice,
                SafetyStock = p.SafetyStock,
                MinOrderQuantity = p.MinOrderQuantity,
                IsActive = p.IsActive,
                Barcodes = p.Barcodes.Select(b => new ProductBarcodeDto
                {
                    Id = b.Id,
                    Barcode = b.Barcode,
                    IsPrimary = b.IsPrimary
                }),
                Inventories = p.Inventories.Select(i => new ProductInventoryDto
                {
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity
                }),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateProductAsync(CreateProductRequest request)
    {
        // 檢查 SKU 是否重複
        if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
        {
            _logger.LogWarning("建立商品失敗：SKU 已存在 - {Sku}", request.Sku);
            return null;
        }

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            TaxType = request.TaxType,
            SellingPrice = request.SellingPrice,
            CostPrice = request.Cost,
            SafetyStock = request.SafetyStock,
            MinOrderQuantity = request.MinOrderQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // 如果有條碼，建立條碼記錄
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcode = new ProductBarcode
            {
                ProductId = product.Id,
                Barcode = request.Barcode,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProductBarcodes.Add(barcode);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("建立商品成功 - {Sku}: {Name}", product.Sku, product.Name);
        return product.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name;

        if (request.Description != null)
            product.Description = request.Description;

        if (request.CategoryId.HasValue)
            product.CategoryId = request.CategoryId.Value;

        if (request.UnitId.HasValue)
            product.UnitId = request.UnitId.Value;

        if (request.TaxType.HasValue)
            product.TaxType = request.TaxType.Value;

        if (request.SellingPrice.HasValue)
            product.SellingPrice = request.SellingPrice.Value;

        if (request.Cost.HasValue)
            product.CostPrice = request.Cost.Value;

        if (request.SafetyStock.HasValue)
            product.SafetyStock = request.SafetyStock.Value;

        if (request.MinOrderQuantity.HasValue)
            product.MinOrderQuantity = request.MinOrderQuantity.Value;

        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("更新商品成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return false;
        }

        // 檢查是否有庫存
        if (product.Inventories.Any(i => i.Quantity > 0))
        {
            _logger.LogWarning("刪除商品失敗：商品尚有庫存 - Id: {Id}", id);
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除商品成功 - Id: {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UnitDto>> GetUnitsAsync()
    {
        return await _context.Units
            .Select(u => new UnitDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int?> CreateUnitAsync(CreateUnitRequest request)
    {
        if (await _context.Units.AnyAsync(u => u.Code == request.Code))
        {
            _logger.LogWarning("建立計量單位失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        var unit = new Unit
        {
            Code = request.Code,
            Name = request.Name
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立計量單位成功 - {Code}: {Name}", unit.Code, unit.Name);
        return unit.Id;
    }

    /// <inheritdoc />
    public async Task<ProductImportResultDto> ImportProductsAsync(ProductImportRequest request)
    {
        var result = new ProductImportResultDto
        {
            TotalCount = request.Products.Count(),
            Errors = new List<ProductImportErrorDto>()
        };

        var errors = (List<ProductImportErrorDto>)result.Errors;
        var categories = await _context.Categories.ToDictionaryAsync(c => c.Code, c => c.Id);
        var units = await _context.Units.ToDictionaryAsync(u => u.Code, u => u.Id);

        int rowNumber = 0;
        foreach (var item in request.Products)
        {
            rowNumber++;
            try
            {
                if (string.IsNullOrWhiteSpace(item.Sku))
                {
                    errors.Add(new ProductImportErrorDto { RowNumber = rowNumber, Sku = item.Sku, ErrorMessage = "SKU 為必填" });
                    result.FailedCount++;
                    continue;
                }

                if (!categories.TryGetValue(item.CategoryCode, out var categoryId))
                {
                    errors.Add(new ProductImportErrorDto { RowNumber = rowNumber, Sku = item.Sku, ErrorMessage = $"找不到分類代碼: {item.CategoryCode}" });
                    result.FailedCount++;
                    continue;
                }

                if (!units.TryGetValue(item.UnitCode, out var unitId))
                {
                    errors.Add(new ProductImportErrorDto { RowNumber = rowNumber, Sku = item.Sku, ErrorMessage = $"找不到單位代碼: {item.UnitCode}" });
                    result.FailedCount++;
                    continue;
                }

                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Sku == item.Sku);

                if (existingProduct != null)
                {
                    if (!request.UpdateExisting)
                    {
                        errors.Add(new ProductImportErrorDto { RowNumber = rowNumber, Sku = item.Sku, ErrorMessage = "SKU 已存在，且未設定更新" });
                        result.FailedCount++;
                        continue;
                    }

                    existingProduct.Name = item.Name;
                    existingProduct.Description = item.Description;
                    existingProduct.CategoryId = categoryId;
                    existingProduct.UnitId = unitId;
                    existingProduct.TaxType = Enum.TryParse<Models.Enums.TaxType>(item.TaxType, out var taxType) ? taxType : Models.Enums.TaxType.Taxable;
                    existingProduct.SellingPrice = item.SellingPrice;
                    existingProduct.CostPrice = item.Cost;
                    existingProduct.SafetyStock = item.SafetyStock;
                    existingProduct.MinOrderQuantity = item.MinOrderQuantity;
                    existingProduct.UpdatedAt = DateTime.UtcNow;

                    result.UpdatedCount++;
                    result.SuccessCount++;
                }
                else
                {
                    var product = new Product
                    {
                        Sku = item.Sku,
                        Name = item.Name,
                        Description = item.Description,
                        CategoryId = categoryId,
                        UnitId = unitId,
                        TaxType = Enum.TryParse<Models.Enums.TaxType>(item.TaxType, out var taxType) ? taxType : Models.Enums.TaxType.Taxable,
                        SellingPrice = item.SellingPrice,
                        CostPrice = item.Cost,
                        SafetyStock = item.SafetyStock,
                        MinOrderQuantity = item.MinOrderQuantity,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(item.Barcode))
                    {
                        _context.ProductBarcodes.Add(new ProductBarcode
                        {
                            ProductId = product.Id,
                            Barcode = item.Barcode,
                            IsPrimary = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    result.CreatedCount++;
                    result.SuccessCount++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ProductImportErrorDto { RowNumber = rowNumber, Sku = item.Sku, ErrorMessage = ex.Message });
                result.FailedCount++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("商品匯入完成 - 成功: {Success}, 失敗: {Failed}", result.SuccessCount, result.FailedCount);

        return result;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportProductsToCsvAsync(int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Barcodes)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var products = await query.Select(p => new ProductExportDto
        {
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description,
            CategoryCode = p.Category.Code,
            CategoryName = p.Category.Name,
            UnitCode = p.Unit.Code,
            UnitName = p.Unit.Name,
            TaxType = p.TaxType.ToString(),
            SellingPrice = p.SellingPrice,
            Cost = p.CostPrice,
            SafetyStock = p.SafetyStock,
            MinOrderQuantity = p.MinOrderQuantity,
            Barcode = p.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault(),
            IsActive = p.IsActive
        }).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("SKU,名稱,描述,分類代碼,分類名稱,單位代碼,單位名稱,稅別,售價,成本,安全庫存,最低訂購量,條碼,啟用");

        foreach (var p in products)
        {
            sb.AppendLine($"\"{p.Sku}\",\"{p.Name}\",\"{p.Description}\",\"{p.CategoryCode}\",\"{p.CategoryName}\",\"{p.UnitCode}\",\"{p.UnitName}\",\"{p.TaxType}\",{p.SellingPrice},{p.Cost},{p.SafetyStock},{p.MinOrderQuantity},\"{p.Barcode}\",{p.IsActive}");
        }

        return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    /// <inheritdoc />
    public Task<byte[]> GetImportTemplateAsync()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("SKU,名稱,描述,分類代碼,單位代碼,稅別,售價,成本,安全庫存,最低訂購量,條碼");
        sb.AppendLine("SAMPLE001,範例商品,商品描述,CAT001,PCS,Taxable,100,50,10,1,4710000000001");

        return Task.FromResult(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray());
    }

    /// <inheritdoc />
    public async Task<ProductImportResultDto> ImportProductsFromExcelAsync(Stream excelFile, bool updateExisting = false)
    {
        var result = new ProductImportResultDto
        {
            Errors = new List<ProductImportErrorDto>()
        };

        var errors = (List<ProductImportErrorDto>)result.Errors;
        var categories = await _context.Categories.ToDictionaryAsync(c => c.Code, c => c.Id);
        var units = await _context.Units.ToDictionaryAsync(u => u.Code, u => u.Id);
        var items = new List<ProductImportItemDto>();

        using (var workbook = new XLWorkbook(excelFile))
        {
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // 跳過標題列

            int rowNumber = 1;
            foreach (var row in rows)
            {
                rowNumber++;
                try
                {
                    var item = new ProductImportItemDto
                    {
                        Sku = row.Cell(1).GetString().Trim(),
                        Name = row.Cell(2).GetString().Trim(),
                        Description = row.Cell(3).GetString().Trim(),
                        CategoryCode = row.Cell(4).GetString().Trim(),
                        UnitCode = row.Cell(5).GetString().Trim(),
                        TaxType = row.Cell(6).GetString().Trim(),
                        SellingPrice = row.Cell(7).GetValue<decimal>(),
                        Cost = row.Cell(8).GetValue<decimal>(),
                        SafetyStock = row.Cell(9).GetValue<int>(),
                        MinOrderQuantity = row.Cell(10).IsEmpty() ? 1 : row.Cell(10).GetValue<int>(),
                        Barcode = row.Cell(11).GetString().Trim()
                    };
                    items.Add(item);
                }
                catch (Exception ex)
                {
                    errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Sku = row.Cell(1).GetString(),
                        ErrorMessage = $"讀取資料錯誤: {ex.Message}"
                    });
                }
            }
        }

        result.TotalCount = items.Count + errors.Count;

        // 使用原有的匯入邏輯處理資料
        var importRequest = new ProductImportRequest
        {
            Products = items,
            UpdateExisting = updateExisting
        };

        var importResult = await ImportProductsAsync(importRequest);
        result.SuccessCount = importResult.SuccessCount;
        result.FailedCount = importResult.FailedCount + errors.Count;
        result.CreatedCount = importResult.CreatedCount;
        result.UpdatedCount = importResult.UpdatedCount;
        errors.AddRange(importResult.Errors);

        return result;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportProductsToExcelAsync(int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Barcodes)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var products = await query.Select(p => new ProductExportDto
        {
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description,
            CategoryCode = p.Category.Code,
            CategoryName = p.Category.Name,
            UnitCode = p.Unit.Code,
            UnitName = p.Unit.Name,
            TaxType = p.TaxType.ToString(),
            SellingPrice = p.SellingPrice,
            Cost = p.CostPrice,
            SafetyStock = p.SafetyStock,
            MinOrderQuantity = p.MinOrderQuantity,
            Barcode = p.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault(),
            IsActive = p.IsActive
        }).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("商品資料");

        // 標題列
        var headers = new[] { "SKU", "名稱", "描述", "分類代碼", "分類名稱", "單位代碼", "單位名稱", "稅別", "售價", "成本", "安全庫存", "最低訂購量", "條碼", "啟用" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // 設定標題列樣式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // 資料列
        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.Sku;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Description;
            worksheet.Cell(row, 4).Value = product.CategoryCode;
            worksheet.Cell(row, 5).Value = product.CategoryName;
            worksheet.Cell(row, 6).Value = product.UnitCode;
            worksheet.Cell(row, 7).Value = product.UnitName;
            worksheet.Cell(row, 8).Value = product.TaxType;
            worksheet.Cell(row, 9).Value = product.SellingPrice;
            worksheet.Cell(row, 10).Value = product.Cost;
            worksheet.Cell(row, 11).Value = product.SafetyStock;
            worksheet.Cell(row, 12).Value = product.MinOrderQuantity;
            worksheet.Cell(row, 13).Value = product.Barcode ?? "";
            worksheet.Cell(row, 14).Value = product.IsActive ? "是" : "否";
            row++;
        }

        // 自動調整欄寬
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <inheritdoc />
    public Task<byte[]> GetImportTemplateExcelAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("商品匯入範本");

        // 標題列
        var headers = new[] { "SKU*", "名稱*", "描述", "分類代碼*", "單位代碼*", "稅別", "售價*", "成本*", "安全庫存", "最低訂購量", "條碼" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // 設定標題列樣式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // 範例資料
        worksheet.Cell(2, 1).Value = "SAMPLE001";
        worksheet.Cell(2, 2).Value = "範例商品";
        worksheet.Cell(2, 3).Value = "商品描述";
        worksheet.Cell(2, 4).Value = "CAT001";
        worksheet.Cell(2, 5).Value = "PCS";
        worksheet.Cell(2, 6).Value = "Taxable";
        worksheet.Cell(2, 7).Value = 100;
        worksheet.Cell(2, 8).Value = 50;
        worksheet.Cell(2, 9).Value = 10;
        worksheet.Cell(2, 10).Value = 1;
        worksheet.Cell(2, 11).Value = "4710000000001";

        // 新增說明工作表
        var helpSheet = workbook.Worksheets.Add("欄位說明");
        helpSheet.Cell(1, 1).Value = "欄位";
        helpSheet.Cell(1, 2).Value = "說明";
        helpSheet.Cell(1, 3).Value = "必填";
        helpSheet.Row(1).Style.Font.Bold = true;

        var helpData = new[]
        {
            new[] { "SKU", "商品編號，唯一識別碼", "是" },
            new[] { "名稱", "商品名稱", "是" },
            new[] { "描述", "商品描述", "否" },
            new[] { "分類代碼", "商品分類代碼，需先建立分類", "是" },
            new[] { "單位代碼", "計量單位代碼，如 PCS", "是" },
            new[] { "稅別", "Taxable/TaxFree/ZeroRate", "否" },
            new[] { "售價", "銷售價格", "是" },
            new[] { "成本", "商品成本", "是" },
            new[] { "安全庫存", "安全庫存量", "否" },
            new[] { "最低訂購量", "最低訂購數量，預設為1", "否" },
            new[] { "條碼", "商品條碼", "否" }
        };

        for (int i = 0; i < helpData.Length; i++)
        {
            helpSheet.Cell(i + 2, 1).Value = helpData[i][0];
            helpSheet.Cell(i + 2, 2).Value = helpData[i][1];
            helpSheet.Cell(i + 2, 3).Value = helpData[i][2];
        }

        worksheet.Columns().AdjustToContents();
        helpSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
