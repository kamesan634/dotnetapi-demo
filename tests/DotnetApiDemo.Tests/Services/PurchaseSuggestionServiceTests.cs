using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Services.Interfaces;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 採購建議服務測試
/// </summary>
public class PurchaseSuggestionServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PurchaseSuggestionService _service;
    private readonly Mock<ILogger<PurchaseSuggestionService>> _loggerMock;
    private readonly Mock<IPurchaseOrderService> _purchaseOrderMock;

    public PurchaseSuggestionServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<PurchaseSuggestionService>>();
        _purchaseOrderMock = new Mock<IPurchaseOrderService>();

        _service = new PurchaseSuggestionService(_context, _purchaseOrderMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetPurchaseSuggestionsAsync_ReturnsSuggestions()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPurchaseSuggestionsAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPurchaseSuggestionsAsync_WithWarehouseFilter_ReturnsFilteredSuggestions()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetPurchaseSuggestionsAsync(request, warehouseId: 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSuggestionSummaryAsync_ReturnsSummary()
    {
        // Act
        var result = await _service.GetSuggestionSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalProductCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GeneratePurchaseOrdersFromSuggestionsAsync_WithEmptyProductIds_ReturnsEmptyList()
    {
        // Arrange
        var request = new GeneratePurchaseOrderRequest
        {
            ProductIds = new List<int>()
        };

        // Act
        var result = await _service.GeneratePurchaseOrdersFromSuggestionsAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
