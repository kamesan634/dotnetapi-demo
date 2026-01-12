using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Services.Interfaces;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 訂單服務測試
/// </summary>
public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderService _service;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IDistributedLockService> _lockMock;
    private readonly Mock<INumberRuleService> _numberRuleMock;
    private readonly Mock<IInventoryService> _inventoryMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IAuditQueueService> _auditMock;
    private readonly Mock<IPointService> _pointMock;

    public OrderServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _lockMock = new Mock<IDistributedLockService>();
        _numberRuleMock = new Mock<INumberRuleService>();
        _inventoryMock = new Mock<IInventoryService>();
        _notificationMock = new Mock<INotificationService>();
        _auditMock = new Mock<IAuditQueueService>();
        _pointMock = new Mock<IPointService>();

        _lockMock.Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        _numberRuleMock.Setup(x => x.GenerateNumberAsync(It.IsAny<string>()))
            .ReturnsAsync($"ORD{DateTime.UtcNow:yyyyMMddHHmmss}");

        _service = new OrderService(
            _context, _loggerMock.Object, _lockMock.Object, _numberRuleMock.Object,
            _inventoryMock.Object, _notificationMock.Object, _auditMock.Object, _pointMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetOrdersAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrdersAsync_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetOrdersAsync(request, status: OrderStatus.Pending);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingId_ReturnsOrder()
    {
        // Arrange
        var order = MockDbContextFactory.CreateTestOrder(_context);

        // Act
        var result = await _service.GetOrderByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetOrderByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ReturnsId()
    {
        // Arrange
        _inventoryMock.Setup(x => x.ReserveStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var request = new CreateOrderRequest
        {
            StoreId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest { ProductId = 1, Quantity = 1 }
            }
        };

        // Act
        var result = await _service.CreateOrderAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelOrderAsync_PendingOrder_ReturnsTrue()
    {
        // Arrange
        _inventoryMock.Setup(x => x.ReserveStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _inventoryMock.Setup(x => x.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var createRequest = new CreateOrderRequest
        {
            StoreId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest { ProductId = 1, Quantity = 1 }
            }
        };
        var orderId = await _service.CreateOrderAsync(createRequest, 1);

        // Act
        var result = await _service.CancelOrderAsync(orderId!.Value, "測試取消", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrderByOrderNoAsync_ExistingOrderNo_ReturnsOrder()
    {
        // Arrange
        var order = MockDbContextFactory.CreateTestOrder(_context);

        // Act
        var result = await _service.GetOrderByOrderNoAsync(order.OrderNo);

        // Assert
        result.Should().NotBeNull();
    }
}
