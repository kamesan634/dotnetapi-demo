using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.PaymentMethods;
using DotnetApiDemo.Tests.TestHelpers;
using Xunit;

namespace DotnetApiDemo.Tests.Controllers;

/// <summary>
/// 付款方式控制器測試
/// </summary>
public class PaymentMethodsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PaymentMethodsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPaymentMethods_WithAuthentication_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/paymentmethods");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaymentMethods_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/paymentmethods");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePaymentMethod_WithValidData_ReturnsCreated()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var request = new CreatePaymentMethodRequest
        {
            Code = $"PAY{DateTime.UtcNow.Ticks}",
            Name = "測試付款方式"
        };

        var response = await client.PostAsJsonAsync("/api/v1/paymentmethods", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetPaymentMethod_ExistingId_ReturnsPaymentMethod()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreatePaymentMethodRequest
        {
            Code = $"GET{DateTime.UtcNow.Ticks}",
            Name = "查詢測試付款方式"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/paymentmethods", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var response = await client.GetAsync($"/api/v1/paymentmethods/{createResult!.Data}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaymentMethod_NonExistingId_ReturnsNotFound()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory);
        var response = await client.GetAsync("/api/v1/paymentmethods/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePaymentMethod_ExistingId_ReturnsSuccess()
    {
        var client = await TestHelper.CreateAuthorizedClientAsync(_factory, "Admin");
        var createRequest = new CreatePaymentMethodRequest
        {
            Code = $"DEL{DateTime.UtcNow.Ticks}",
            Name = "刪除測試付款方式"
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/paymentmethods", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();

        var response = await client.DeleteAsync($"/api/v1/paymentmethods/{createResult!.Data}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
