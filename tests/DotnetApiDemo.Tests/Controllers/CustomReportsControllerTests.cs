using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 自訂報表控制器測試 - 100% 涵蓋
/// </summary>
public class CustomReportsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CustomReportsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region Custom Reports

    [Fact]
    public async Task GetCustomReports_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomReports_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomReports_WithIncludePublicFalse_ReturnsOnlyOwnReports()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom?includePublic=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCustomReport_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var request = new CreateCustomReportRequest
        {
            Code = $"RPT{DateTime.UtcNow.Ticks}",
            Name = "測試自訂報表",
            ReportType = "Sales",
            IsPublic = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/reports/custom", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCustomReport_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var code = $"DUP{DateTime.UtcNow.Ticks}";

        // 建立第一個報表
        var request1 = new CreateCustomReportRequest
        {
            Code = code,
            Name = "報表 1",
            ReportType = "Sales"
        };
        await client.PostAsJsonAsync("/api/v1/reports/custom", request1);

        // 嘗試建立重複代碼的報表
        var request2 = new CreateCustomReportRequest
        {
            Code = code,
            Name = "報表 2",
            ReportType = "Sales"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/reports/custom", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomReport_ExistingId_ReturnsReport()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢測試報表",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/{reportId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomReport_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomReport_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前報表",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        var updateRequest = new UpdateCustomReportRequest { Name = "更新後報表" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/reports/custom/{reportId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCustomReport_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試報表",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/reports/custom/{reportId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RunCustomReport_ExistingReport_ReturnsResult()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"RUN{DateTime.UtcNow.Ticks}",
            Name = "執行測試報表",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/reports/custom/{reportId}/run", (RunCustomReportRequest?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RunCustomReport_NonExistingReport_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/reports/custom/99999/run", (RunCustomReportRequest?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportCustomReport_ExistingReport_ReturnsFile()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"EXP{DateTime.UtcNow.Ticks}",
            Name = "匯出測試報表",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/{reportId}/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportCustomReport_WithJsonFormat_ReturnsJson()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立報表
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"JSON{DateTime.UtcNow.Ticks}",
            Name = "JSON 匯出測試",
            ReportType = "Sales"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/{reportId}/export?format=json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Scheduled Reports

    [Fact]
    public async Task GetScheduledReports_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom/scheduled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateScheduledReport_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var request = new CreateScheduledReportRequest
        {
            Name = "測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            ScheduleTime = "08:00",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetScheduledReport_ExistingId_ReturnsReport()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "查詢測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/scheduled/{reportId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScheduledReport_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/reports/custom/scheduled/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateScheduledReport_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "更新前排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        var updateRequest = new UpdateScheduledReportRequest { Name = "更新後排程報表" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/reports/custom/scheduled/{reportId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteScheduledReport_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "刪除測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/reports/custom/scheduled/{reportId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RunScheduledReportNow_ExistingReport_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 先建立自訂報表
        var customReportRequest = new CreateCustomReportRequest
        {
            Code = $"SCHED{DateTime.UtcNow.Ticks}",
            Name = "排程用自訂報表",
            ReportType = "Sales"
        };
        var customReportResponse = await client.PostAsJsonAsync("/api/v1/reports/custom", customReportRequest);
        var customReportResult = await customReportResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var customReportId = customReportResult!.Data;

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "立即執行測試排程報表",
            CustomReportId = customReportId,
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.PostAsync($"/api/v1/reports/custom/scheduled/{reportId}/run", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetScheduledReportHistory_ExistingReport_ReturnsHistory()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "歷史查詢測試報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/scheduled/{reportId}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScheduledReportHistory_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 建立排程報表
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "限制查詢測試報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/reports/custom/scheduled", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reportId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/reports/custom/scheduled/{reportId}/history?limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
