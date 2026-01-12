using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 客戶等級控制器測試
/// </summary>
public class CustomerLevelsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CustomerLevelsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCustomerLevels_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/customerlevels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerLevels_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/customerlevels");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomerLevel_WithValidData_ReturnsCreated()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateCustomerLevelRequest
        {
            Code = $"LVL{DateTime.UtcNow.Ticks}",
            Name = "測試等級",
            DiscountRate = 95,
            PointRate = 2
        };

        var response = await client.PostAsJsonAsync("/api/v1/customerlevels", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetCustomerLevel_NonExistingId_ReturnsNotFound()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/customerlevels/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomerLevel_ExistingId_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreateCustomerLevelRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試等級",
            DiscountRate = 90
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/customerlevels", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var response = await client.DeleteAsync($"/api/v1/customerlevels/{createResult!.Data}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
