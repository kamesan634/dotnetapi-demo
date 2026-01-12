using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Orders;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 訂單控制器測試
/// </summary>
public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public OrdersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 測試取得訂單列表
    /// </summary>
    [Fact]
    public async Task GetOrders_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<OrderListDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    /// <summary>
    /// 測試建立訂單
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreated()
    {
        // Arrange
        var (storeId, productId) = await SeedOrderPrerequisitesAsync();
        await AuthenticateAsync();

        var request = new CreateOrderRequest
        {
            StoreId = storeId,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest
                {
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// 測試取得訂單詳細資訊
    /// </summary>
    [Fact]
    public async Task GetOrder_ExistingId_ReturnsOrder()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();
        var orderId = await CreateTestOrderAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDetailDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    /// <summary>
    /// 測試新增付款
    /// </summary>
    [Fact]
    public async Task AddPayment_ToExistingOrder_ReturnsSuccess()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();
        var orderId = await CreateTestOrderAsync();

        var paymentRequest = new AddPaymentRequest
        {
            PaymentMethod = PaymentMethod.Cash,
            Amount = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{orderId}/payments", paymentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    /// <summary>
    /// 測試完成訂單
    /// </summary>
    [Fact]
    public async Task CompleteOrder_ExistingOrder_ReturnsSuccess()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();
        var orderId = await CreateTestOrderAsync();

        // Act
        var response = await _client.PostAsync($"/api/v1/orders/{orderId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    /// <summary>
    /// 測試取消訂單
    /// </summary>
    [Fact]
    public async Task CancelOrder_PendingOrder_ReturnsSuccess()
    {
        // Arrange
        await SeedDataAsync();
        await AuthenticateAsync();
        var orderId = await CreateTestOrderAsync();

        // Act
        var response = await _client.PostAsync($"/api/v1/orders/{orderId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    private async Task SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 建立角色
        if (!await roleManager.RoleExistsAsync("Staff"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Staff" });
        }

        // 建立測試使用者
        if (await userManager.FindByNameAsync("ordertest") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "ordertest",
                Email = "ordertest@test.com",
                FullName = "訂單測試使用者",
                IsActive = true
            };
            await userManager.CreateAsync(user, "Test123!");
            await userManager.AddToRoleAsync(user, "Staff");
        }
    }

    private async Task<(int StoreId, int ProductId)> SeedOrderPrerequisitesAsync()
    {
        await SeedDataAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 建立門市
        var store = new Store
        {
            Code = "TEST-S",
            Name = "測試門市",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Stores.Add(store);

        // 建立計量單位
        var unit = new Unit { Code = "PCS", Name = "個" };
        context.Units.Add(unit);

        // 建立分類
        var category = new Category
        {
            Code = "CAT-T",
            Name = "測試分類",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Categories.Add(category);

        await context.SaveChangesAsync();

        // 建立商品
        var product = new Product
        {
            Sku = "SKU-" + Guid.NewGuid().ToString("N").Substring(0, 8),
            Name = "測試商品",
            CategoryId = category.Id,
            UnitId = unit.Id,
            SellingPrice = 100m,
            Cost = 50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return (store.Id, product.Id);
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new LoginRequest
        {
            UserName = "ordertest",
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

    private async Task<int> CreateTestOrderAsync()
    {
        var (storeId, productId) = await SeedOrderPrerequisitesAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync("ordertest");

        var order = new Order
        {
            OrderNo = "TEST-" + Guid.NewGuid().ToString("N").Substring(0, 8),
            StoreId = storeId,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            SubTotal = 200m,
            TaxAmount = 10m,
            TotalAmount = 210m,
            DiscountAmount = 0,
            FinalAmount = 210m,
            PaidAmount = 0,
            CreatedBy = user!.Id,
            CreatedAt = DateTime.UtcNow
        };

        order.Items.Add(new OrderItem
        {
            ProductId = productId,
            Quantity = 2,
            UnitPrice = 100m,
            Amount = 200m
        });

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order.Id;
    }
}
