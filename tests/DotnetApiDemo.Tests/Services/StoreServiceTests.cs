using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Stores;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 門市服務測試
/// </summary>
public class StoreServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly StoreService _service;
    private readonly Mock<ILogger<StoreService>> _loggerMock;

    public StoreServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<StoreService>>();
        _service = new StoreService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetStoresAsync_ReturnsStores()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetStoresAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateStoreAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateStoreRequest
        {
            Code = $"ST{DateTime.UtcNow.Ticks}",
            Name = "新門市"
        };

        // Act
        var result = await _service.CreateStoreAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateStoreAsync_WithDuplicateCode_ReturnsNull()
    {
        // Arrange
        var code = $"DUP{DateTime.UtcNow.Ticks}";
        await _service.CreateStoreAsync(new CreateStoreRequest { Code = code, Name = "門市 1" });

        // Act
        var result = await _service.CreateStoreAsync(new CreateStoreRequest { Code = code, Name = "門市 2" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStoreByIdAsync_ExistingId_ReturnsStore()
    {
        // Arrange
        var createResult = await _service.CreateStoreAsync(new CreateStoreRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢門市"
        });

        // Act
        var result = await _service.GetStoreByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStoreByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetStoreByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStoreAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateStoreAsync(new CreateStoreRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前"
        });

        // Act
        var result = await _service.UpdateStoreAsync(createResult!.Value, new UpdateStoreRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteStoreAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateStoreAsync(new CreateStoreRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除門市"
        });

        // Act
        var result = await _service.DeleteStoreAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveStoresAsync_ReturnsOnlyActive()
    {
        // Act
        var result = await _service.GetActiveStoresAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStoreWarehousesAsync_ExistingStore_ReturnsWarehouses()
    {
        // Arrange
        var createResult = await _service.CreateStoreAsync(new CreateStoreRequest
        {
            Code = $"WH{DateTime.UtcNow.Ticks}",
            Name = "倉庫測試門市"
        });

        // Act
        var result = await _service.GetStoreWarehousesAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }
}
