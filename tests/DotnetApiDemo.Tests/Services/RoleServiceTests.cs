using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Roles;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Services;

/// <summary>
/// 角色服務測試
/// </summary>
public class RoleServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<ILogger<RoleService>> _loggerMock;
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        _context = MockDbContextFactory.CreateWithSeedData();
        _loggerMock = new Mock<ILogger<RoleService>>();

        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null!, null!, null!, null!);

        _service = new RoleService(_context, _roleManagerMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsRoles()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetRolesAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRoleByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetRoleByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }
}
