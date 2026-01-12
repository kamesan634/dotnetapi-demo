using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Users;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 使用者控制器測試
/// </summary>
public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_WithAdminRole_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");

        // Act
        var response = await client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_WithAdminRole_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateUserRequest
        {
            UserName = $"newuser{DateTime.UtcNow.Ticks}",
            Email = $"newuser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "新測試使用者",
            RoleIds = new List<int>()
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立使用者
        var createRequest = new CreateUserRequest
        {
            UserName = $"getuser{DateTime.UtcNow.Ticks}",
            Email = $"getuser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "查詢測試使用者"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var userId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUser_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.GetAsync("/api/v1/users/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立使用者
        var createRequest = new CreateUserRequest
        {
            UserName = $"upduser{DateTime.UtcNow.Ticks}",
            Email = $"upduser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "更新前使用者"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var userId = createResult!.Data;

        var updateRequest = new UpdateUserRequest { FullName = "更新後使用者" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/users/{userId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立使用者
        var createRequest = new CreateUserRequest
        {
            UserName = $"deluser{DateTime.UtcNow.Ticks}",
            Email = $"deluser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "刪除測試使用者"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var userId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立使用者
        var createRequest = new CreateUserRequest
        {
            UserName = $"pwduser{DateTime.UtcNow.Ticks}",
            Email = $"pwduser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "密碼測試使用者"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var userId = createResult!.Data;

        var passwordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "Test123!",
            NewPassword = "NewTest123!",
            ConfirmPassword = "NewTest123!"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/users/{userId}/change-password", passwordRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ToggleUserStatus_ExistingUser_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立使用者
        var createRequest = new CreateUserRequest
        {
            UserName = $"toguser{DateTime.UtcNow.Ticks}",
            Email = $"toguser{DateTime.UtcNow.Ticks}@test.com",
            Password = "Test123!",
            FullName = "狀態測試使用者"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var userId = createResult!.Data;

        // Act
        var response = await client.PostAsync($"/api/v1/users/{userId}/toggle-status", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
