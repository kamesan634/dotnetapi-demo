using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Roles;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 角色控制器測試
/// </summary>
public class RolesControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public RolesControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoles_WithAdminRole_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoles_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");

        // Act
        var response = await client.GetAsync("/api/v1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateRoleRequest
        {
            Name = $"TestRole{DateTime.UtcNow.Ticks}",
            Description = "測試角色"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetRole_ExistingId_ReturnsRole()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立角色
        var createRequest = new CreateRoleRequest
        {
            Name = $"GetRole{DateTime.UtcNow.Ticks}",
            Description = "查詢測試角色"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var roleId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRole_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/roles/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立角色
        var createRequest = new CreateRoleRequest
        {
            Name = $"UpdRole{DateTime.UtcNow.Ticks}",
            Description = "更新前角色"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var roleId = createResult!.Data;

        var updateRequest = new UpdateRoleRequest { Description = "更新後角色" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/roles/{roleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteRole_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立角色
        var createRequest = new CreateRoleRequest
        {
            Name = $"DelRole{DateTime.UtcNow.Ticks}",
            Description = "刪除測試角色"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var roleId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
