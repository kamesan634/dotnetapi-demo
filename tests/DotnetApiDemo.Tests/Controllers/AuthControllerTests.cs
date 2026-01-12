using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.Entities;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 驗證控制器測試
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 測試使用者註冊成功
    /// </summary>
    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FullName = "測試使用者"
        };

        // 先建立必要的角色
        await SeedRolesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    /// <summary>
    /// 測試使用者註冊 - 密碼太短
    /// </summary>
    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "testuser2",
            Email = "test2@example.com",
            Password = "123",
            ConfirmPassword = "123",
            FullName = "測試使用者"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 測試使用者登入成功
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        await SeedRolesAsync();
        await SeedTestUserAsync();

        var request = new LoginRequest
        {
            UserName = "logintest",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 測試使用者登入 - 錯誤密碼
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await SeedRolesAsync();
        await SeedTestUserAsync();

        var request = new LoginRequest
        {
            UserName = "logintest",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// 測試取得目前使用者 - 未授權
    /// </summary>
    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task SeedRolesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roles = new[] { "Admin", "Manager", "Staff", "Warehouse", "Purchaser" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} 角色"
                });
            }
        }
    }

    private async Task SeedTestUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByNameAsync("logintest") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "logintest",
                Email = "logintest@example.com",
                FullName = "登入測試使用者",
                IsActive = true
            };

            await userManager.CreateAsync(user, "Test123!");
            await userManager.AddToRoleAsync(user, "Staff");
        }
    }
}
