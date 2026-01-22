using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Coupons;
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
            PromotionId = 1,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/coupons", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
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
    public async Task DeleteCoupon_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // Act
        var response = await client.DeleteAsync("/api/v1/coupons/99999");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
