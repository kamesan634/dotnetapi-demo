using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 客戶服務測試
/// </summary>
public class CustomerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CustomerService _service;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;

    public CustomerServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _service = new CustomerService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetCustomersAsync_ReturnsCustomers()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCustomersAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "新客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        };

        // Act
        var result = await _service.CreateCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ExistingId_ReturnsCustomer()
    {
        // Arrange
        var createResult = await _service.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = "查詢客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        });

        // Act
        var result = await _service.GetCustomerByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomerByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetCustomerByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCustomerAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = "更新前",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        });

        // Act
        var result = await _service.UpdateCustomerAsync(createResult!.Value, new UpdateCustomerRequest { Name = "更新後" });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCustomerAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = "刪除客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        });

        // Act
        var result = await _service.DeleteCustomerAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerByPhoneAsync_ExistingPhone_ReturnsCustomer()
    {
        // Arrange
        var phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}";
        await _service.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = "電話查詢客戶",
            Phone = phone
        });

        // Act
        var result = await _service.GetCustomerByPhoneAsync(phone);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomerByPhoneAsync_NonExistingPhone_ReturnsNull()
    {
        // Act
        var result = await _service.GetCustomerByPhoneAsync("0900000000");

        // Assert
        result.Should().BeNull();
    }
}
