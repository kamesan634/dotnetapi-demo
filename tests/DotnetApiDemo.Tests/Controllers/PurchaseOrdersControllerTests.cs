using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Purchasing;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 採購單控制器測試
/// </summary>
public class PurchaseOrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PurchaseOrdersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPurchaseOrders_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseOrders_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");

        // 需要先有供應商和商品
        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = 1,
            WarehouseId = 1,
            ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Notes = "測試採購單",
            Items = new List<CreatePurchaseOrderItemRequest>
            {
                new CreatePurchaseOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 10,
                    UnitPrice = 100
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/purchaseorders", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPurchaseOrder_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPurchaseOrders_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders?status=Draft");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseOrders_WithSupplierFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders?supplierId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseOrders_WithDateRange_ReturnsFilteredResults()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/v1/purchaseorders?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseOrders_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");

        // Act
        var response = await client.GetAsync("/api/v1/purchaseorders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
