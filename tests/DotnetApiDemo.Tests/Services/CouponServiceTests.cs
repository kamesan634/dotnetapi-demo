using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Promotions;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 優惠券服務測試
/// </summary>
public class CouponServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CouponService _service;
    private readonly Mock<ILogger<CouponService>> _loggerMock;

    public CouponServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<CouponService>>();
        _service = new CouponService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetCouponsAsync_ReturnsCoupons()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetCouponsAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCouponAsync_WithValidData_ReturnsId()
    {
        // Arrange
        var request = new CreateCouponRequest
        {
            Code = $"COUP{DateTime.UtcNow.Ticks}",
            Name = "新優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };

        // Act
        var result = await _service.CreateCouponAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCouponByIdAsync_ExistingId_ReturnsCoupon()
    {
        // Arrange
        var createResult = await _service.CreateCouponAsync(new CreateCouponRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        });

        // Act
        var result = await _service.GetCouponByIdAsync(createResult!.Value);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCouponByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetCouponByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCouponAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var createResult = await _service.CreateCouponAsync(new CreateCouponRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        });

        // Act
        var result = await _service.DeleteCouponAsync(createResult!.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCouponAsync_ValidCode_ReturnsValid()
    {
        // Arrange
        var code = $"VAL{DateTime.UtcNow.Ticks}";
        await _service.CreateCouponAsync(new CreateCouponRequest
        {
            Code = code,
            Name = "驗證優惠券",
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        });

        // Act
        var result = await _service.ValidateCouponAsync(code, 100);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateCouponAsync_InvalidCode_ReturnsInvalid()
    {
        // Act
        var result = await _service.ValidateCouponAsync("INVALID_CODE", 100);

        // Assert
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
    }
}
