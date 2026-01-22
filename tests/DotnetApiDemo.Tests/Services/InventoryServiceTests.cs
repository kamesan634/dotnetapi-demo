using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 庫存服務測試
/// </summary>
public class InventoryServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly InventoryService _service;
    private readonly Mock<ILogger<InventoryService>> _loggerMock;

    public InventoryServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<InventoryService>>();
        _service = new InventoryService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetInventoriesAsync_ReturnsInventory()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetInventoriesAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoriesAsync_WithWarehouseFilter_ReturnsFilteredInventory()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetInventoriesAsync(request, warehouseId: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoriesAsync_WithProductFilter_ReturnsFilteredInventory()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetInventoriesAsync(request, productId: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryByIdAsync_ExistingId_ReturnsInventory()
    {
        // Arrange - get first inventory from seed data
        var inventory = _context.Inventories.First();

        // Act
        var result = await _service.GetInventoryByIdAsync(inventory.Id);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetInventoryByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AdjustInventoryAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var request = new AdjustInventoryRequest
        {
            ProductId = 1,
            WarehouseId = 1,
            Quantity = 10,
            Reason = "測試調整"
        };

        // Act
        var result = await _service.AdjustInventoryAsync(request, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AdjustInventoryAsync_WithInvalidProduct_ReturnsFalse()
    {
        // Arrange
        var request = new AdjustInventoryRequest
        {
            ProductId = 99999,
            WarehouseId = 1,
            Quantity = 10,
            Reason = "測試"
        };

        // Act
        var result = await _service.AdjustInventoryAsync(request, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TransferInventoryAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var request = new TransferInventoryRequest
        {
            ProductId = 1,
            FromWarehouseId = 1,
            ToWarehouseId = 1,
            Quantity = 5,
            Reason = "測試轉移"
        };

        // Act
        var result = await _service.TransferInventoryAsync(request, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetLowStockAlertsAsync_ReturnsAlerts()
    {
        // Act
        var result = await _service.GetLowStockAlertsAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
