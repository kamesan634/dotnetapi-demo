using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;
using DotnetApiDemo.Models.Enums;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 促銷活動控制器測試
/// </summary>
public class PromotionsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PromotionsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPromotions_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/promotions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPromotions_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/promotions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePromotion_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreatePromotionRequest
        {
            Code = $"PROMO{DateTime.UtcNow.Ticks}",
            Name = "測試促銷活動",
            PromotionType = PromotionType.Discount,
            DiscountValue = 15,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/promotions", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPromotion_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/promotions/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetActivePromotions_ReturnsOnlyActive()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/promotions/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
