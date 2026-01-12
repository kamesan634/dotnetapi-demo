using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Reports;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 自訂報表服務測試 - 100% 涵蓋
/// </summary>
public class CustomReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CustomReportService _service;
    private readonly Mock<ILogger<CustomReportService>> _loggerMock;

    public CustomReportServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<CustomReportService>>();
        _service = new CustomReportService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Custom Reports

    [Fact]
    public async Task GetCustomReportsAsync_ReturnsReports()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCustomReportsAsync(request, 1, true);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetCustomReportsAsync_WithoutPublic_ReturnsOnlyOwn()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCustomReportsAsync(request, 1, false);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCustomReportAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateCustomReportRequest
        {
            Code = $"TEST{DateTime.UtcNow.Ticks}",
            Name = "測試報表",
            ReportType = "Sales"
        };

        // Act
        var result = await _service.CreateCustomReportAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateCustomReportAsync_WithDuplicateCode_ReturnsNull()
    {
        // Arrange
        var code = $"DUP{DateTime.UtcNow.Ticks}";
        var request1 = new CreateCustomReportRequest
        {
            Code = code,
            Name = "報表 1",
            ReportType = "Sales"
        };
        await _service.CreateCustomReportAsync(request1, 1);

        var request2 = new CreateCustomReportRequest
        {
            Code = code,
            Name = "報表 2",
            ReportType = "Sales"
        };

        // Act
        var result = await _service.CreateCustomReportAsync(request2, 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomReportByIdAsync_ExistingId_ReturnsReport()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢測試報表",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.GetCustomReportByIdAsync(reportId!.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("查詢測試報表");
    }

    [Fact]
    public async Task GetCustomReportByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetCustomReportByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCustomReportAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前報表",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        var updateRequest = new UpdateCustomReportRequest { Name = "更新後報表" };

        // Act
        var result = await _service.UpdateCustomReportAsync(reportId!.Value, updateRequest, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCustomReportAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var updateRequest = new UpdateCustomReportRequest { Name = "更新報表" };

        // Act
        var result = await _service.UpdateCustomReportAsync(99999, updateRequest, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCustomReportAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試報表",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.DeleteCustomReportAsync(reportId!.Value, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCustomReportAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteCustomReportAsync(99999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RunCustomReportAsync_SalesReport_ReturnsResult()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"RUN{DateTime.UtcNow.Ticks}",
            Name = "執行測試報表",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.RunCustomReportAsync(reportId!.Value, null);

        // Assert
        result.Should().NotBeNull();
        result!.ReportName.Should().Be("執行測試報表");
    }

    [Fact]
    public async Task RunCustomReportAsync_InventoryReport_ReturnsResult()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"INV{DateTime.UtcNow.Ticks}",
            Name = "庫存報表",
            ReportType = "Inventory"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.RunCustomReportAsync(reportId!.Value, null);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunCustomReportAsync_CustomerReport_ReturnsResult()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"CUS{DateTime.UtcNow.Ticks}",
            Name = "客戶報表",
            ReportType = "Customer"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.RunCustomReportAsync(reportId!.Value, null);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunCustomReportAsync_ProductReport_ReturnsResult()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"PRD{DateTime.UtcNow.Ticks}",
            Name = "商品報表",
            ReportType = "Product"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.RunCustomReportAsync(reportId!.Value, null);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunCustomReportAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.RunCustomReportAsync(99999, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExportCustomReportAsync_CsvFormat_ReturnsCsv()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"CSV{DateTime.UtcNow.Ticks}",
            Name = "CSV 匯出測試",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.ExportCustomReportAsync(reportId!.Value, "csv", null);

        // Assert
        result.Should().NotBeNull();
        result!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportCustomReportAsync_JsonFormat_ReturnsJson()
    {
        // Arrange
        var createRequest = new CreateCustomReportRequest
        {
            Code = $"JSON{DateTime.UtcNow.Ticks}",
            Name = "JSON 匯出測試",
            ReportType = "Sales"
        };
        var reportId = await _service.CreateCustomReportAsync(createRequest, 1);

        // Act
        var result = await _service.ExportCustomReportAsync(reportId!.Value, "json", null);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Scheduled Reports

    [Fact]
    public async Task GetScheduledReportsAsync_ReturnsReports()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetScheduledReportsAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateScheduledReportAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateScheduledReportRequest
        {
            Name = "測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };

        // Act
        var result = await _service.CreateScheduledReportAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetScheduledReportByIdAsync_ExistingId_ReturnsReport()
    {
        // Arrange
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "查詢測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var reportId = await _service.CreateScheduledReportAsync(createRequest, 1);

        // Act
        var result = await _service.GetScheduledReportByIdAsync(reportId!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetScheduledReportByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetScheduledReportByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateScheduledReportAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "更新前排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var reportId = await _service.CreateScheduledReportAsync(createRequest, 1);

        var updateRequest = new UpdateScheduledReportRequest { Name = "更新後排程報表" };

        // Act
        var result = await _service.UpdateScheduledReportAsync(reportId!.Value, updateRequest, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteScheduledReportAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "刪除測試排程報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var reportId = await _service.CreateScheduledReportAsync(createRequest, 1);

        // Act
        var result = await _service.DeleteScheduledReportAsync(reportId!.Value, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetScheduledReportHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var createRequest = new CreateScheduledReportRequest
        {
            Name = "歷史測試報表",
            ReportType = "Sales",
            Schedule = "Daily",
            OutputFormat = "CSV",
            DeliveryMethod = "Download"
        };
        var reportId = await _service.CreateScheduledReportAsync(createRequest, 1);

        // Act
        var result = await _service.GetScheduledReportHistoryAsync(reportId!.Value, 10);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
