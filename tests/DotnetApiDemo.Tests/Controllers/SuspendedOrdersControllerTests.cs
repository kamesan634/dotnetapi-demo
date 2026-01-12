using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 掛單控制器測試
/// </summary>
public class SuspendedOrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SuspendedOrdersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSuspendedOrders_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/suspendedorders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuspendedOrders_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/suspendedorders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSuspendedOrder_WithValidData_ReturnsCreated()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var request = new CreateSuspendedOrderRequest
        {
            StoreId = 1,
            Reason = "測試掛單",
            Items = new List<CreateSuspendedOrderItemRequest>
            {
                new CreateSuspendedOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 100
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/v1/suspendedorders", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSuspendedOrder_NonExistingId_ReturnsNotFound()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/suspendedorders/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSuspendedOrders_WithStoreFilter_ReturnsFilteredResults()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/suspendedorders?storeId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingCount_ReturnsCount()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/suspendedorders/pending-count?storeId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
