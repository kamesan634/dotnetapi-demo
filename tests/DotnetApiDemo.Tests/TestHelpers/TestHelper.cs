using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.Entities;

namespace DotnetApiDemo.Tests.TestHelpers;

/// <summary>
/// 測試輔助類別
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// 建立所有角色
    /// </summary>
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
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

    /// <summary>
    /// 建立測試使用者並取得 Token
    /// </summary>
    public static async Task<string> GetAuthTokenAsync(
        HttpClient client,
        IServiceProvider services,
        string role = "Admin",
        string userName = "testadmin")
    {
        await SeedRolesAsync(services);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = $"{userName}@test.com",
                FullName = $"Test {role}",
                IsActive = true
            };
            await userManager.CreateAsync(user, "Test123!");
            await userManager.AddToRoleAsync(user, role);
        }

        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = "Test123!"
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

        return result?.Data?.AccessToken ?? throw new Exception("無法取得 Token");
    }

    /// <summary>
    /// 設定授權標頭
    /// </summary>
    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// 建立已授權的 HttpClient
    /// </summary>
    public static async Task<HttpClient> CreateAuthorizedClientAsync(
        CustomWebApplicationFactory<Program> factory,
        string role = "Admin")
    {
        var client = factory.CreateClient();
        var token = await GetAuthTokenAsync(client, factory.Services, role);
        SetAuthToken(client, token);
        return client;
    }
}
