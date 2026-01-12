using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.NumberRules;

namespace DotnetApiDemo.Services.Interfaces;

public interface INumberRuleService
{
    Task<IEnumerable<NumberRuleListDto>> GetRulesAsync();
    Task<NumberRuleDetailDto?> GetRuleByIdAsync(int id);
    Task<NumberRuleDetailDto?> GetRuleByTypeAsync(string ruleType);
    Task<string> GenerateNumberAsync(string ruleType);
    Task<int?> CreateRuleAsync(CreateNumberRuleRequest request);
    Task<bool> UpdateRuleAsync(int id, UpdateNumberRuleRequest request);
    Task<bool> DeleteRuleAsync(int id);
}
