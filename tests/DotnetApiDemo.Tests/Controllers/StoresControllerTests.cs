using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Stores;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 門市控制器測試
/// </summary>
public class StoresControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public StoresControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStores_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/stores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStores_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/stores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStore_WithAdminRole_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateStoreRequest
        {
            Code = $"ST{DateTime.UtcNow.Ticks}",
            Name = "新測試門市",
            Address = "測試地址"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/stores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateStore_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var request = new CreateStoreRequest
        {
            Code = $"ST{DateTime.UtcNow.Ticks}",
            Name = "Staff 嘗試建立"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/stores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetStore_ExistingId_ReturnsStore()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立門市
        var createRequest = new CreateStoreRequest
        {
            Code = $"GST{DateTime.UtcNow.Ticks}",
            Name = "查詢測試門市"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var storeId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/stores/{storeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStore_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/stores/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStore_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立門市
        var createRequest = new CreateStoreRequest
        {
            Code = $"UST{DateTime.UtcNow.Ticks}",
            Name = "更新前門市"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var storeId = createResult!.Data;

        var updateRequest = new UpdateStoreRequest { Name = "更新後門市" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/stores/{storeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteStore_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立門市
        var createRequest = new CreateStoreRequest
        {
            Code = $"DST{DateTime.UtcNow.Ticks}",
            Name = "刪除測試門市"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var storeId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/stores/{storeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStoreWarehouses_ExistingStore_ReturnsWarehouses()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立門市
        var createRequest = new CreateStoreRequest
        {
            Code = $"WST{DateTime.UtcNow.Ticks}",
            Name = "倉庫測試門市"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var storeId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/stores/{storeId}/warehouses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
