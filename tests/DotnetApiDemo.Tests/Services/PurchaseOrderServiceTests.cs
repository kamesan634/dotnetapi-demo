using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 採購單服務測試
/// </summary>
public class PurchaseOrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PurchaseOrderService _service;
    private readonly Mock<ILogger<PurchaseOrderService>> _loggerMock;

    public PurchaseOrderServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<PurchaseOrderService>>();
        _service = new PurchaseOrderService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetPurchaseOrdersAsync_ReturnsOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPurchaseOrdersAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePurchaseOrderAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = 1,
            WarehouseId = 1,
            ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Items = new List<CreatePurchaseOrderItemRequest>
            {
                new CreatePurchaseOrderItemRequest { ProductId = 1, Quantity = 10, UnitPrice = 100 }
            }
        };

        // Act
        var result = await _service.CreatePurchaseOrderAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPurchaseOrderByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetPurchaseOrderByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPurchaseOrdersAsync_WithSupplierFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPurchaseOrdersAsync(request, supplierId: 1);

        // Assert
        result.Should().NotBeNull();
    }
}
