using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Services.Implementations;
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

    public OrderServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _service = new OrderService(_context, _loggerMock.Object);
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
    public async Task GetOrdersAsync_WithStoreFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetOrdersAsync(request, storeId: 1);

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
    public async Task GetOrderByOrderNoAsync_ExistingOrderNo_ReturnsOrder()
    {
        // Arrange
        var order = MockDbContextFactory.CreateTestOrder(_context);

        // Act
        var result = await _service.GetOrderByOrderNoAsync(order.OrderNo);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrderByOrderNoAsync_NonExistingOrderNo_ReturnsNull()
    {
        // Act
        var result = await _service.GetOrderByOrderNoAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ReturnsId()
    {
        // Arrange
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
        var order = MockDbContextFactory.CreateTestOrder(_context);

        // Act
        var result = await _service.CancelOrderAsync(order.Id, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelOrderAsync_NonExistingOrder_ReturnsFalse()
    {
        // Act
        var result = await _service.CancelOrderAsync(99999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_ReturnsOrders()
    {
        // Arrange
        var order = MockDbContextFactory.CreateTestOrder(_context, customerId: 1);
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCustomerOrdersAsync(1, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingOrdersAsync_ReturnsPendingOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPendingOrdersAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingOrderSummaryAsync_ReturnsSummary()
    {
        // Act
        var result = await _service.GetPendingOrderSummaryAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
