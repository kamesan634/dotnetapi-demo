using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 系統設定控制器測試
/// </summary>
public class SystemSettingsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SystemSettingsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSystemSettings_WithAdminRole_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/api/v1/systemsettings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSystemSettings_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/systemsettings");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSystemSettings_WithStaffRole_ReturnsForbidden()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var response = await client.GetAsync("/api/v1/systemsettings");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSystemSetting_ByKey_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/api/v1/systemsettings/CompanyName");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
