using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 商品控制器測試
/// </summary>
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 測試取得商品列表
    /// </summary>
    [Fact]
    public async Task GetProducts_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<ProductListDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    /// <summary>
    /// 測試取得商品列表 - 未授權
    /// </summary>
    [Fact]
    public async Task GetProducts_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// 測試建立商品
    /// </summary>
    [Fact]
    public async Task CreateProduct_WithAdminRole_ReturnsCreated()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync("Admin");

        var request = new CreateProductRequest
        {
            Sku = "TEST-001",
            Name = "測試商品",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100m,
            Cost = 50m,
            TaxType = TaxType.Taxable
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// 測試建立商品 - 一般使用者無權限
    /// </summary>
    [Fact]
    public async Task CreateProduct_WithStaffRole_ReturnsForbidden()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync("Staff");

        var request = new CreateProductRequest
        {
            Sku = "TEST-002",
            Name = "測試商品2",
            CategoryId = 1,
            UnitId = 1,
            SellingPrice = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// 測試取得商品詳細資訊
    /// </summary>
    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();
        var productId = await CreateTestProductAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDetailDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    /// <summary>
    /// 測試取得商品詳細資訊 - 不存在的 ID
    /// </summary>
    [Fact]
    public async Task GetProduct_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/products/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 建立角色
        var roles = new[] { "Admin", "Manager", "Staff" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }

        // 建立測試使用者
        if (await userManager.FindByNameAsync("testadmin") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "testadmin",
                Email = "admin@test.com",
                FullName = "測試管理員",
                IsActive = true
            };
            await userManager.CreateAsync(adminUser, "Test123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (await userManager.FindByNameAsync("teststaff") == null)
        {
            var staffUser = new ApplicationUser
            {
                UserName = "teststaff",
                Email = "staff@test.com",
                FullName = "測試員工",
                IsActive = true
            };
            await userManager.CreateAsync(staffUser, "Test123!");
            await userManager.AddToRoleAsync(staffUser, "Staff");
        }

        // 建立基礎資料
        if (!context.Units.Any())
        {
            context.Units.Add(new Unit { Code = "PCS", Name = "個" });
            await context.SaveChangesAsync();
        }

        if (!context.Categories.Any())
        {
            context.Categories.Add(new Category
            {
                Code = "TEST",
                Name = "測試分類",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task AuthenticateAsync(string role = "Staff")
    {
        var userName = role == "Admin" ? "testadmin" : "teststaff";
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = "Test123!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

        if (result?.Data?.AccessToken != null)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
        }
    }

    private async Task<int> CreateTestProductAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var product = new Product
        {
            Sku = "AUTO-" + Guid.NewGuid().ToString("N").Substring(0, 8),
            Name = "自動測試商品",
            CategoryId = context.Categories.First().Id,
            UnitId = context.Units.First().Id,
            SellingPrice = 100m,
            Cost = 50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.Id;
    }
}
