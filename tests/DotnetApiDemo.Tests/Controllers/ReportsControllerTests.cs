using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Services.Interfaces;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 報表控制器測試 - 100% 涵蓋
/// </summary>
public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ReportsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region Dashboard

    [Fact]
    public async Task GetDashboardSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/reports/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Sales Report

    [Fact]
    public async Task GetSalesReport_WithDefaultDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSalesReport_WithCustomDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/reports/sales?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSalesReport_WithInvalidDates_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/reports/sales?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSalesReport_WithAdminRole_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/sales/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportSalesReport_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");

        // Act
        var response = await client.GetAsync("/api/v1/reports/sales/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Top Products

    [Fact]
    public async Task GetTopProducts_WithDefaultParams_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/top-products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopProducts_WithCustomLimit_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/top-products?limit=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopProducts_WithInvalidLimit_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/top-products?limit=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTopProducts_WithLimitExceedingMax_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/top-products?limit=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportTopProducts_WithAdminRole_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/top-products/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    #endregion

    #region Inventory Report

    [Fact]
    public async Task GetInventorySummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/inventory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportInventoryReport_WithAdminRole_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/inventory/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    #endregion

    #region Purchasing Report

    [Fact]
    public async Task GetPurchasingSummary_WithDefaultDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/purchasing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchasingSummary_WithCustomDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/reports/purchasing?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportPurchasingReport_WithAdminRole_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/purchasing/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Customer Report

    [Fact]
    public async Task GetCustomerSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportCustomerReport_WithAdminRole_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/customers/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    #endregion

    #region Sales Trend

    [Fact]
    public async Task GetSalesTrend_WithDefaultDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/sales/trend");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSalesTrend_WithCustomDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var startDate = DateTime.UtcNow.AddDays(-14).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/reports/sales/trend?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Profit Report

    [Fact]
    public async Task GetProfitReport_WithAdminRole_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/reports/profit");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProfitReport_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");

        // Act
        var response = await client.GetAsync("/api/v1/reports/profit");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProfitReport_WithCustomDates_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/reports/profit?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Comparison Report

    [Fact]
    public async Task GetComparisonReport_WithValidPeriods_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var period1Start = DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd");
        var period1End = DateTime.UtcNow.AddDays(-31).ToString("yyyy-MM-dd");
        var period2Start = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var period2End = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync(
            $"/api/v1/reports/comparison?period1Start={period1Start}&period1End={period1End}&period2Start={period2Start}&period2End={period2End}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetComparisonReport_WithInvalidPeriods_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var period1Start = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var period1End = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"); // End before start
        var period2Start = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var period2End = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync(
            $"/api/v1/reports/comparison?period1Start={period1Start}&period1End={period1End}&period2Start={period2Start}&period2End={period2End}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetComparisonReport_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var period1Start = DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd");
        var period1End = DateTime.UtcNow.AddDays(-31).ToString("yyyy-MM-dd");
        var period2Start = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var period2End = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync(
            $"/api/v1/reports/comparison?period1Start={period1Start}&period1End={period1End}&period2Start={period2Start}&period2End={period2End}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
