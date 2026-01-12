using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Labels;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class LabelsController : ControllerBase
{
    private readonly ILabelService _labelService;
    private readonly ILogger<LabelsController> _logger;

    public LabelsController(ILabelService labelService, ILogger<LabelsController> logger)
    {
        _labelService = labelService;
        _logger = logger;
    }

    #region Templates

    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LabelTemplateListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<LabelTemplateListDto>>>> GetTemplates(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? type = null)
    {
        var result = await _labelService.GetTemplatesAsync(request, type);
        return Ok(ApiResponse<PaginatedResponse<LabelTemplateListDto>>.SuccessResponse(result));
    }

    [HttpGet("templates/active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabelTemplateListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LabelTemplateListDto>>>> GetActiveTemplates(
        [FromQuery] string? type = null)
    {
        var templates = await _labelService.GetActiveTemplatesAsync(type);
        return Ok(ApiResponse<IEnumerable<LabelTemplateListDto>>.SuccessResponse(templates));
    }

    [HttpGet("templates/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LabelTemplateDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LabelTemplateDetailDto>>> GetTemplate(int id)
    {
        var template = await _labelService.GetTemplateByIdAsync(id);
        if (template == null)
            return NotFound(ApiResponse.FailResponse("找不到標籤模板"));

        return Ok(ApiResponse<LabelTemplateDetailDto>.SuccessResponse(template));
    }

    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateTemplate([FromBody] CreateLabelTemplateRequest request)
    {
        var templateId = await _labelService.CreateTemplateAsync(request);
        if (templateId == null)
            return BadRequest(ApiResponse.FailResponse("建立標籤模板失敗，代碼可能已存在"));

        return CreatedAtAction(
            nameof(GetTemplate),
            new { id = templateId },
            ApiResponse<int>.SuccessResponse(templateId.Value, "標籤模板建立成功"));
    }

    [HttpPut("templates/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateTemplate(int id, [FromBody] UpdateLabelTemplateRequest request)
    {
        var success = await _labelService.UpdateTemplateAsync(id, request);
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到標籤模板"));

        return Ok(ApiResponse.SuccessResponse("標籤模板更新成功"));
    }

    [HttpDelete("templates/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteTemplate(int id)
    {
        var success = await _labelService.DeleteTemplateAsync(id);
        if (!success)
            return NotFound(ApiResponse.FailResponse("找不到標籤模板"));

        return Ok(ApiResponse.SuccessResponse("標籤模板刪除成功"));
    }

    #endregion

    #region Print Jobs

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PrintJobListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PrintJobListDto>>>> GetPrintJobs(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? status = null)
    {
        var result = await _labelService.GetPrintJobsAsync(request, status);
        return Ok(ApiResponse<PaginatedResponse<PrintJobListDto>>.SuccessResponse(result));
    }

    [HttpGet("jobs/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PrintJobDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PrintJobDetailDto>>> GetPrintJob(int id)
    {
        var job = await _labelService.GetPrintJobByIdAsync(id);
        if (job == null)
            return NotFound(ApiResponse.FailResponse("找不到列印任務"));

        return Ok(ApiResponse<PrintJobDetailDto>.SuccessResponse(job));
    }

    [HttpPost("jobs")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreatePrintJob([FromBody] CreatePrintJobRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var jobId = await _labelService.CreatePrintJobAsync(request, userId);
        if (jobId == null)
            return BadRequest(ApiResponse.FailResponse("建立列印任務失敗"));

        return CreatedAtAction(
            nameof(GetPrintJob),
            new { id = jobId },
            ApiResponse<int>.SuccessResponse(jobId.Value, "列印任務建立成功"));
    }

    [HttpPost("jobs/batch")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateBatchPrintJob([FromBody] BatchPrintRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var jobId = await _labelService.CreateBatchPrintJobAsync(request, userId);
        if (jobId == null)
            return BadRequest(ApiResponse.FailResponse("建立批次列印任務失敗"));

        return CreatedAtAction(
            nameof(GetPrintJob),
            new { id = jobId },
            ApiResponse<int>.SuccessResponse(jobId.Value, "批次列印任務建立成功"));
    }

    [HttpPost("jobs/{id:int}/start")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> StartPrintJob(int id)
    {
        var success = await _labelService.StartPrintJobAsync(id);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("開始列印任務失敗"));

        return Ok(ApiResponse.SuccessResponse("列印任務已開始"));
    }

    [HttpPost("jobs/{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CompletePrintJob(int id, [FromQuery] int printedCount)
    {
        var success = await _labelService.CompletePrintJobAsync(id, printedCount);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("完成列印任務失敗"));

        return Ok(ApiResponse.SuccessResponse("列印任務已完成"));
    }

    [HttpPost("jobs/{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CancelPrintJob(int id)
    {
        var success = await _labelService.CancelPrintJobAsync(id);
        if (!success)
            return BadRequest(ApiResponse.FailResponse("取消列印任務失敗"));

        return Ok(ApiResponse.SuccessResponse("列印任務已取消"));
    }

    #endregion

    #region Preview

    [HttpGet("preview/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<LabelPreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LabelPreviewDto>>> GetLabelPreview(int productId)
    {
        var preview = await _labelService.GetLabelPreviewAsync(productId);
        if (preview == null)
            return NotFound(ApiResponse.FailResponse("找不到商品"));

        return Ok(ApiResponse<LabelPreviewDto>.SuccessResponse(preview));
    }

    [HttpPost("preview/batch")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabelPreviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LabelPreviewDto>>>> GetBatchPreview([FromBody] IEnumerable<int> productIds)
    {
        var previews = await _labelService.GetBatchPreviewAsync(productIds);
        return Ok(ApiResponse<IEnumerable<LabelPreviewDto>>.SuccessResponse(previews));
    }

    #endregion
}
