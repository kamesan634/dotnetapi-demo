using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Inventory;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Services.Interfaces;
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
    private readonly Mock<IDistributedLockService> _lockMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IAuditQueueService> _auditMock;

    public InventoryServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<InventoryService>>();
        _lockMock = new Mock<IDistributedLockService>();
        _notificationMock = new Mock<INotificationService>();
        _auditMock = new Mock<IAuditQueueService>();

        // Setup lock mock to always acquire successfully
        _lockMock.Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

        _service = new InventoryService(_context, _loggerMock.Object, _lockMock.Object, _notificationMock.Object, _auditMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetInventoryAsync_ReturnsInventory()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetInventoryAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLowStockAlertsAsync_ReturnsAlerts()
    {
        // Act
        var result = await _service.GetLowStockAlertsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryByProductAsync_ExistingProduct_ReturnsInventory()
    {
        // Act
        var result = await _service.GetInventoryByProductAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventoryByWarehouseAsync_ExistingWarehouse_ReturnsInventory()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetInventoryByWarehouseAsync(1, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInventorySummaryAsync_ReturnsSummary()
    {
        // Act
        var result = await _service.GetInventorySummaryAsync();

        // Assert
        result.Should().NotBeNull();
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
}
