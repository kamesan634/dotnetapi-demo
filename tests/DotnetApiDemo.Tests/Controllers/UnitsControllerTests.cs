using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 單位控制器測試
/// </summary>
public class UnitsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UnitsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUnits_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/units");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUnits_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/units");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUnit_WithValidData_ReturnsCreated()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateUnitRequest
        {
            Code = $"UNIT{DateTime.UtcNow.Ticks}",
            Name = "測試單位"
        };

        var response = await client.PostAsJsonAsync("/api/v1/units", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetUnit_ExistingId_ReturnsUnit()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreateUnitRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢測試單位"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/units", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var response = await client.GetAsync($"/api/v1/units/{createResult!.Data}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUnit_NonExistingId_ReturnsNotFound()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/units/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUnit_ExistingId_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreateUnitRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前單位"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/units", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var updateRequest = new UpdateUnitRequest { Name = "更新後單位" };
        var response = await client.PutAsJsonAsync($"/api/v1/units/{createResult!.Data}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUnit_ExistingId_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreateUnitRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試單位"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/units", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var response = await client.DeleteAsync($"/api/v1/units/{createResult!.Data}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActiveUnits_ReturnsOnlyActive()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/units/active");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
