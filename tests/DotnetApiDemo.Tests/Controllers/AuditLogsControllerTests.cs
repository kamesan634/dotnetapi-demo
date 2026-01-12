using System.Net;
using FluentAssertions;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 審計日誌控制器測試
/// </summary>
public class AuditLogsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuditLogsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAuditLogs_WithAdminRole_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/api/v1/auditlogs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditLogs_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/auditlogs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_WithStaffRole_ReturnsForbidden()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Staff");
        var response = await client.GetAsync("/api/v1/auditlogs");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_WithDateFilter_ReturnsFilteredResults()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await client.GetAsync($"/api/v1/auditlogs?startDate={startDate}&endDate={endDate}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditLogs_WithEntityTypeFilter_ReturnsFilteredResults()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/api/v1/auditlogs?entityType=Order");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditLog_NonExistingId_ReturnsNotFound()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/api/v1/auditlogs/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
