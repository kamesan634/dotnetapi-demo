using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.NumberRules;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class NumberRulesController : ControllerBase
{
    private readonly INumberRuleService _numberRuleService;

    public NumberRulesController(INumberRuleService numberRuleService)
    {
        _numberRuleService = numberRuleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NumberRuleListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NumberRuleListDto>>>> GetRules()
    {
        var rules = await _numberRuleService.GetRulesAsync();
        return Ok(ApiResponse<IEnumerable<NumberRuleListDto>>.SuccessResponse(rules));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<NumberRuleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NumberRuleDetailDto>>> GetRule(int id)
    {
        var rule = await _numberRuleService.GetRuleByIdAsync(id);
        if (rule == null) return NotFound(ApiResponse.FailResponse("找不到編號規則"));
        return Ok(ApiResponse<NumberRuleDetailDto>.SuccessResponse(rule));
    }

    [HttpGet("type/{ruleType}")]
    [ProducesResponseType(typeof(ApiResponse<NumberRuleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NumberRuleDetailDto>>> GetRuleByType(string ruleType)
    {
        var rule = await _numberRuleService.GetRuleByTypeAsync(ruleType);
        if (rule == null) return NotFound(ApiResponse.FailResponse("找不到編號規則"));
        return Ok(ApiResponse<NumberRuleDetailDto>.SuccessResponse(rule));
    }

    [HttpPost("generate/{ruleType}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GenerateNumber(string ruleType)
    {
        var number = await _numberRuleService.GenerateNumberAsync(ruleType);
        return Ok(ApiResponse<string>.SuccessResponse(number));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateRule([FromBody] CreateNumberRuleRequest request)
    {
        var ruleId = await _numberRuleService.CreateRuleAsync(request);
        if (ruleId == null) return BadRequest(ApiResponse.FailResponse("建立編號規則失敗"));
        return CreatedAtAction(nameof(GetRule), new { id = ruleId }, ApiResponse<int>.SuccessResponse(ruleId.Value, "編號規則建立成功"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateRule(int id, [FromBody] UpdateNumberRuleRequest request)
    {
        var success = await _numberRuleService.UpdateRuleAsync(id, request);
        if (!success) return BadRequest(ApiResponse.FailResponse("更新編號規則失敗"));
        return Ok(ApiResponse.SuccessResponse("編號規則更新成功"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeleteRule(int id)
    {
        var success = await _numberRuleService.DeleteRuleAsync(id);
        if (!success) return BadRequest(ApiResponse.FailResponse("刪除編號規則失敗"));
        return Ok(ApiResponse.SuccessResponse("編號規則刪除成功"));
    }
}
