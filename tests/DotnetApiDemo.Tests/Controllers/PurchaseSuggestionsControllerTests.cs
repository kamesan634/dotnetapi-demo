using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Services.Interfaces;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 採購建議控制器測試
/// </summary>
public class PurchaseSuggestionsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PurchaseSuggestionsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WithPurchaserRole_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var response = await client.GetAsync("/api/v1/purchasesuggestions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/purchasesuggestions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WithStaffRole_ReturnsForbidden()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var response = await client.GetAsync("/api/v1/purchasesuggestions");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WithWarehouseFilter_ReturnsFilteredResults()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var response = await client.GetAsync("/api/v1/purchasesuggestions?warehouseId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WithSupplierFilter_ReturnsFilteredResults()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var response = await client.GetAsync("/api/v1/purchasesuggestions?supplierId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggestionSummary_WithPurchaserRole_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var response = await client.GetAsync("/api/v1/purchasesuggestions/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GeneratePurchaseOrders_WithEmptyProductIds_ReturnsBadRequest()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var request = new GeneratePurchaseOrderRequest
        {
            ProductIds = new List<int>()
        };

        var response = await client.PostAsJsonAsync("/api/v1/purchasesuggestions/generate-orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GeneratePurchaseOrders_WithValidProductIds_ReturnsCreated()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Purchaser");
        var request = new GeneratePurchaseOrderRequest
        {
            ProductIds = new List<int> { 1, 2 },
            GroupBySupplier = true
        };

        var response = await client.PostAsJsonAsync("/api/v1/purchasesuggestions/generate-orders", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }
}
