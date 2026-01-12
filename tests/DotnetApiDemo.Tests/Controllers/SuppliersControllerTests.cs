using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 供應商控制器測試
/// </summary>
public class SuppliersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SuppliersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSuppliers_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/suppliers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuppliers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/suppliers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSupplier_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateSupplierRequest
        {
            Code = $"SUP{DateTime.UtcNow.Ticks}",
            Name = "新測試供應商",
            ContactPerson = "聯絡人",
            Phone = "02-12345678"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/suppliers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetSupplier_ExistingId_ReturnsSupplier()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立供應商
        var createRequest = new CreateSupplierRequest
        {
            Code = $"GSP{DateTime.UtcNow.Ticks}",
            Name = "查詢測試供應商",
            ContactPerson = "聯絡人"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/suppliers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var supplierId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/suppliers/{supplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSupplier_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/suppliers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSupplier_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立供應商
        var createRequest = new CreateSupplierRequest
        {
            Code = $"USP{DateTime.UtcNow.Ticks}",
            Name = "更新前供應商",
            ContactPerson = "聯絡人"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/suppliers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var supplierId = createResult!.Data;

        var updateRequest = new UpdateSupplierRequest { Name = "更新後供應商" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/suppliers/{supplierId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteSupplier_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立供應商
        var createRequest = new CreateSupplierRequest
        {
            Code = $"DSP{DateTime.UtcNow.Ticks}",
            Name = "刪除測試供應商",
            ContactPerson = "聯絡人"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/suppliers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var supplierId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/suppliers/{supplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActiveSuppliers_ReturnsOnlyActiveSuppliers()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/suppliers/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
