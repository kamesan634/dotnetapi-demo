using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 點數控制器測試
/// </summary>
public class PointsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PointsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPointHistory_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/points/customer/1/history");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPointHistory_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/points/customer/1/history");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPointBalance_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/points/customer/1/balance");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
