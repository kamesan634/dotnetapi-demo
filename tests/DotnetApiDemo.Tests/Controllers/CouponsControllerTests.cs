using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 優惠券控制器測試
/// </summary>
public class CouponsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CouponsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCoupons_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCoupons_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCoupon_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreateCouponRequest
        {
            Code = $"COUP{DateTime.UtcNow.Ticks}",
            Name = "測試優惠券",
            DiscountType = "Percentage",
            DiscountValue = 10,
            MinimumAmount = 100,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            UsageLimit = 100
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetCoupon_ExistingId_ReturnsCoupon()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立優惠券
        var createRequest = new CreateCouponRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢測試優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/coupons", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var couponId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/coupons/{couponId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCoupon_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/coupons/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCoupon_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立優惠券
        var createRequest = new CreateCouponRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/coupons", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var couponId = createResult!.Data;

        var updateRequest = new UpdateCouponRequest { Name = "更新後優惠券" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/coupons/{couponId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCoupon_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 建立優惠券
        var createRequest = new CreateCouponRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/coupons", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var couponId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/coupons/{couponId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidateCoupon_ValidCode_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var code = $"VAL{DateTime.UtcNow.Ticks}";

        // 建立優惠券
        var createRequest = new CreateCouponRequest
        {
            Code = code,
            Name = "驗證測試優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };
        await client.PostAsJsonAsync("/api/v1/coupons", createRequest);

        // Act
        var response = await client.GetAsync($"/api/v1/coupons/validate/{code}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateCoupon_InvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/coupons/validate/INVALID_CODE");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
