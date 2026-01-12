using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Customers;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 客戶控制器測試
/// </summary>
public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CustomersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCustomers_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var request = new CreateCustomerRequest
        {
            Name = "測試新客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}",
            Email = $"test{DateTime.UtcNow.Ticks}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomer_ExistingId_ReturnsCustomer()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 先建立客戶
        var createRequest = new CreateCustomerRequest
        {
            Name = "查詢測試客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var customerId = createResult!.Data;

        // Act
        var response = await client.GetAsync($"/api/v1/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDetailDto>>();
        result.Should().NotBeNull();
        result!.Data!.Name.Should().Be("查詢測試客戶");
    }

    [Fact]
    public async Task GetCustomer_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/customers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // 先建立客戶
        var createRequest = new CreateCustomerRequest
        {
            Name = "更新前客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var customerId = createResult!.Data;

        var updateRequest = new UpdateCustomerRequest { Name = "更新後客戶" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/customers/{customerId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCustomer_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");

        // 先建立客戶
        var createRequest = new CreateCustomerRequest
        {
            Name = "刪除測試客戶",
            Phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var customerId = createResult!.Data;

        // Act
        var response = await client.DeleteAsync($"/api/v1/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchCustomers_WithKeyword_ReturnsFilteredResults()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);

        // Act
        var response = await client.GetAsync("/api/v1/customers?search=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerByPhone_ExistingPhone_ReturnsCustomer()
    {
        // Arrange
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var phone = $"09{DateTime.UtcNow.Ticks % 100000000:00000000}";

        // 先建立客戶
        var createRequest = new CreateCustomerRequest
        {
            Name = "電話查詢客戶",
            Phone = phone
        };
        await client.PostAsJsonAsync("/api/v1/customers", createRequest);

        // Act
        var response = await client.GetAsync($"/api/v1/customers/phone/{phone}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
