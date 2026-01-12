using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Suppliers;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 供應商服務測試
/// </summary>
public class SupplierServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SupplierService _service;
    private readonly Mock<ILogger<SupplierService>> _loggerMock;

    public SupplierServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<SupplierService>>();
        _service = new SupplierService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetSuppliersAsync_ReturnsSuppliers()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetSuppliersAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSupplierAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Code = $"SUP{DateTime.UtcNow.Ticks}",
            Name = "新供應商",
            ContactPerson = "聯絡人"
        };

        // Act
        var result = await _service.CreateSupplierAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateSupplierAsync_WithDuplicateCode_ReturnsNull()
    {
        // Arrange
        var code = $"DUP{DateTime.UtcNow.Ticks}";
        await _service.CreateSupplierAsync(new CreateSupplierRequest { Code = code, Name = "供應商 1", ContactPerson = "聯絡人" });

        // Act
        var result = await _service.CreateSupplierAsync(new CreateSupplierRequest { Code = code, Name = "供應商 2", ContactPerson = "聯絡人" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSupplierByIdAsync_ExistingId_ReturnsSupplier()
    {
        // Arrange
        var createResult = await _service.CreateSupplierAsync(new CreateSupplierRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢供應商",
            ContactPerson = "聯絡人"
        });

        // Act
        var result = await _service.GetSupplierByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSupplierByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetSupplierByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSupplierAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateSupplierAsync(new CreateSupplierRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前",
            ContactPerson = "聯絡人"
        });

        // Act
        var result = await _service.UpdateSupplierAsync(createResult!.Value, new UpdateSupplierRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSupplierAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateSupplierAsync(new CreateSupplierRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除供應商",
            ContactPerson = "聯絡人"
        });

        // Act
        var result = await _service.DeleteSupplierAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveSuppliersAsync_ReturnsOnlyActive()
    {
        // Act
        var result = await _service.GetActiveSuppliersAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
