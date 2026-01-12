using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 分類控制器測試
/// </summary>
public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CategoriesControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCategories_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateCategoryRequest
        {
            Code = $"CAT{DateTime.UtcNow.Ticks}",
            Name = "新測試分類"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var code = $"DUP{DateTime.UtcNow.Ticks}";

        // 先建立一個分類
        var request1 = new CreateCategoryRequest { Code = code, Name = "分類 1" };
        await client.PostAsJsonAsync("/api/v1/categories", request1);

        // 嘗試建立重複代碼的分類
        var request2 = new CreateCategoryRequest { Code = code, Name = "分類 2" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCategory_ExistingId_ReturnsCategory()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 先建立分類
        var createRequest = new CreateCategoryRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "取得測試分類"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/categories", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var categoryId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDetailDto>>();
        result.Should().NotBeNull();
        result!.Data!.Name.Should().Be("取得測試分類");
    }

    [Fact]
    public async Task GetCategory_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/categories/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立分類
        var createRequest = new CreateCategoryRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前分類"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/categories", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var categoryId = createResult!.Data;

        var updateRequest = new UpdateCategoryRequest { Name = "更新後分類" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/categories/{categoryId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCategory_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立分類
        var createRequest = new CreateCategoryRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試分類"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/categories", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var categoryId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryTree_ReturnsHierarchicalData()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/categories/tree");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCategory_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var request = new CreateCategoryRequest
        {
            Code = $"STAFF{DateTime.UtcNow.Ticks}",
            Name = "Staff 嘗試建立"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
