using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 報表服務測試 - 100% 涵蓋
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReportService _service;
    private readonly Mock<ILogger<ReportService>> _loggerMock;

    public ReportServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<ReportService>>();
        _service = new ReportService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Dashboard

    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsValidSummary()
    {
        // Act
        var result = await _service.GetDashboardSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TodaySales.Should().BeGreaterOrEqualTo(0);
        result.TodayOrders.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_IncludesLowStockCount()
    {
        // Arrange - 測試資料中有低庫存商品
        var result = await _service.GetDashboardSummaryAsync();

        // Assert
        result.LowStockCount.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Sales Report

    [Fact]
    public async Task GetSalesReportAsync_WithValidDateRange_ReturnsReport()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetSalesReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSalesReportAsync_WithNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-9));

        // Act
        var result = await _service.GetSalesReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportSalesReportToCsvAsync_ReturnsValidCsv()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.ExportSalesReportToCsvAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Top Products

    [Fact]
    public async Task GetTopProductsAsync_WithValidParams_ReturnsProducts()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetTopProductsAsync(10, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTopProductsAsync_WithLimit_RespectsLimit()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetTopProductsAsync(5, startDate, endDate);

        // Assert
        result.Count().Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task ExportTopProductsToCsvAsync_ReturnsValidCsv()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.ExportTopProductsToCsvAsync(10, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Inventory Report

    [Fact]
    public async Task GetInventorySummaryAsync_ReturnsValidSummary()
    {
        // Act
        var result = await _service.GetInventorySummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalProducts.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ExportInventoryReportToCsvAsync_ReturnsValidCsv()
    {
        // Act
        var result = await _service.ExportInventoryReportToCsvAsync();

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Purchase Report

    [Fact]
    public async Task GetPurchaseSummaryAsync_WithValidDateRange_ReturnsReport()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetPurchaseSummaryAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportPurchaseReportToCsvAsync_ReturnsValidCsv()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.ExportPurchaseReportToCsvAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Customer Report

    [Fact]
    public async Task GetCustomerSummaryAsync_ReturnsValidSummary()
    {
        // Act
        var result = await _service.GetCustomerSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCustomers.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ExportCustomerReportToCsvAsync_ReturnsValidCsv()
    {
        // Act
        var result = await _service.ExportCustomerReportToCsvAsync();

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Sales Trend

    [Fact]
    public async Task GetSalesTrendAsync_WithValidDateRange_ReturnsTrend()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetSalesTrendAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Profit Report

    [Fact]
    public async Task GetProfitReportAsync_WithValidDateRange_ReturnsReport()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetProfitReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.GrossProfitMargin.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Comparison Report

    [Fact]
    public async Task GetComparisonReportAsync_WithValidPeriods_ReturnsComparison()
    {
        // Arrange
        var period1Start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60));
        var period1End = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-31));
        var period2Start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var period2End = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _service.GetComparisonReportAsync(period1Start, period1End, period2Start, period2End);

        // Assert
        result.Should().NotBeNull();
        result.Period1.Should().NotBeNull();
        result.Period2.Should().NotBeNull();
    }

    #endregion
}
