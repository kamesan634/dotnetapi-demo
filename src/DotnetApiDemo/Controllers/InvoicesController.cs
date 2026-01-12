using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Invoices;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InvoiceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<InvoiceListDto>>>> GetInvoices(
        [FromQuery] PaginationRequest request)
    {
        var result = await _invoiceService.GetInvoicesAsync(request);
        return Ok(ApiResponse<PaginatedResponse<InvoiceListDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetInvoice(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound(ApiResponse.FailResponse("找不到發票"));
        return Ok(ApiResponse<InvoiceDetailDto>.SuccessResponse(invoice));
    }

    [HttpGet("number/{invoiceNumber}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetInvoiceByNumber(string invoiceNumber)
    {
        var invoice = await _invoiceService.GetInvoiceByNumberAsync(invoiceNumber);
        if (invoice == null) return NotFound(ApiResponse.FailResponse("找不到發票"));
        return Ok(ApiResponse<InvoiceDetailDto>.SuccessResponse(invoice));
    }

    [HttpGet("order/{orderId:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto?>>> GetInvoiceByOrder(int orderId)
    {
        var invoice = await _invoiceService.GetInvoiceByOrderAsync(orderId);
        return Ok(ApiResponse<InvoiceDetailDto?>.SuccessResponse(invoice));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var invoiceId = await _invoiceService.CreateInvoiceAsync(request);
        if (invoiceId == null) return BadRequest(ApiResponse.FailResponse("建立發票失敗"));
        return CreatedAtAction(nameof(GetInvoice), new { id = invoiceId }, ApiResponse<int>.SuccessResponse(invoiceId.Value, "發票建立成功"));
    }

    [HttpPost("{id:int}/void")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> VoidInvoice(int id, [FromBody] VoidInvoiceRequest request)
    {
        var success = await _invoiceService.VoidInvoiceAsync(id, request);
        if (!success) return BadRequest(ApiResponse.FailResponse("作廢發票失敗"));
        return Ok(ApiResponse.SuccessResponse("發票已作廢"));
    }
}
