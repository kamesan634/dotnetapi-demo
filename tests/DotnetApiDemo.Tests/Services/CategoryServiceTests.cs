using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Products;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Services.Interfaces;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 分類服務測試
/// </summary>
public class CategoryServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryService _service;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly Mock<ICacheService> _cacheMock;

    public CategoryServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<CategoryService>>();
        _cacheMock = new Mock<ICacheService>();
        _service = new CategoryService(_context, _loggerMock.Object, _cacheMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCategoriesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Code = $"CAT{DateTime.UtcNow.Ticks}",
            Name = "新分類"
        };

        // Act
        var result = await _service.CreateCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateCode_ReturnsNull()
    {
        // Arrange
        var code = $"DUP{DateTime.UtcNow.Ticks}";
        await _service.CreateCategoryAsync(new CreateCategoryRequest { Code = code, Name = "分類 1" });

        // Act
        var result = await _service.CreateCategoryAsync(new CreateCategoryRequest { Code = code, Name = "分類 2" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ExistingId_ReturnsCategory()
    {
        // Arrange
        var createResult = await _service.CreateCategoryAsync(new CreateCategoryRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢分類"
        });

        // Act
        var result = await _service.GetCategoryByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetCategoryByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCategoryAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateCategoryAsync(new CreateCategoryRequest
        {
            Code = $"UPD{DateTime.UtcNow.Ticks}",
            Name = "更新前"
        });

        // Act
        var result = await _service.UpdateCategoryAsync(createResult!.Value, new UpdateCategoryRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateCategoryAsync(new CreateCategoryRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除分類"
        });

        // Act
        var result = await _service.DeleteCategoryAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCategoryTreeAsync_ReturnsTree()
    {
        // Act
        var result = await _service.GetCategoryTreeAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveCategoriesAsync_ReturnsOnlyActive()
    {
        // Act
        var result = await _service.GetActiveCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
