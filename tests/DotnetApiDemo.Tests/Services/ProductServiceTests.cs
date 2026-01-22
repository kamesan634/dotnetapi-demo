using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 商品服務測試
/// </summary>
public class ProductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductService _service;
    private readonly Mock<ILogger<ProductService>> _loggerMock;

    public ProductServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsProducts()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetProductsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsAsync_WithSearch_ReturnsFilteredProducts()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10, Search = "測試" };

        // Act
        var result = await _service.GetProductsAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = $"SKU{DateTime.UtcNow.Ticks}",
            Name = "新商品",
            CategoryId = 1,
            UnitId = 1,
            CostPrice = 100,
            SellingPrice = 150
        };

        // Act
        var result = await _service.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSku_ReturnsNull()
    {
        // Arrange
        var sku = $"DUP{DateTime.UtcNow.Ticks}";
        await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = sku,
            Name = "商品 1",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = sku,
            Name = "商品 2",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var createResult = await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢商品",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.GetProductByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetProductByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProductAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.UpdateProductAsync(createResult!.Value, new UpdateProductRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除商品",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.DeleteProductAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductBySkuAsync_ExistingSku_ReturnsProduct()
    {
        // Arrange
        var sku = $"SKU{DateTime.UtcNow.Ticks}";
        await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = sku,
            Name = "SKU 查詢商品",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.GetProductBySkuAsync(sku);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductByBarcodeAsync_ExistingBarcode_ReturnsProduct()
    {
        // Arrange
        var barcode = $"{DateTime.UtcNow.Ticks}";
        await _service.CreateProductAsync(new CreateProductRequest
        {
            Sku = $"BAR{DateTime.UtcNow.Ticks}",
            Barcode = barcode,
            Name = "條碼查詢商品",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100
        });

        // Act
        var result = await _service.GetProductByBarcodeAsync(barcode);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveProductsAsync_ReturnsOnlyActive()
    {
        // Act
        var result = await _service.GetActiveProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.All(p => p.IsActive).Should().BeTrue();
    }
}
