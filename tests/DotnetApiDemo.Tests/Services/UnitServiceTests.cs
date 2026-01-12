using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 單位服務測試
/// </summary>
public class UnitServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UnitService _service;
    private readonly Mock<ILogger<UnitService>> _loggerMock;

    public UnitServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<UnitService>>();
        _service = new UnitService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetUnitsAsync_ReturnsUnits()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetUnitsAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUnitAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateUnitRequest
        {
            Code = $"UNIT{DateTime.UtcNow.Ticks}",
            Name = "新單位"
        };

        // Act
        var result = await _service.CreateUnitAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUnitByIdAsync_ExistingId_ReturnsUnit()
    {
        // Arrange
        var createResult = await _service.CreateUnitAsync(new CreateUnitRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢單位"
        });

        // Act
        var result = await _service.GetUnitByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUnitByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetUnitByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUnitAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateUnitAsync(new CreateUnitRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前"
        });

        // Act
        var result = await _service.UpdateUnitAsync(createResult!.Value, new UpdateUnitRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUnitAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateUnitAsync(new CreateUnitRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除單位"
        });

        // Act
        var result = await _service.DeleteUnitAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveUnitsAsync_ReturnsOnlyActive()
    {
        // Act
        var result = await _service.GetActiveUnitsAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
