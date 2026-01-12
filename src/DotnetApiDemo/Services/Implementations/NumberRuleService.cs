using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.NumberRules;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class NumberRuleService : INumberRuleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NumberRuleService> _logger;

    public NumberRuleService(ApplicationDbContext context, ILogger<NumberRuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<NumberRuleListDto>> GetRulesAsync()
    {
        return await _context.NumberRules
            .OrderBy(r => r.RuleType)
            .Select(r => new NumberRuleListDto
            {
                Id = r.Id,
                RuleType = r.RuleType,
                Prefix = r.Prefix,
                DateFormat = r.DateFormat,
                SequenceLength = r.SequenceLength,
                ResetDaily = r.ResetDaily,
                IsActive = r.IsActive
            })
            .ToListAsync();
    }

    public async Task<NumberRuleDetailDto?> GetRuleByIdAsync(int id)
    {
        var rule = await _context.NumberRules.FindAsync(id);
        if (rule == null) return null;
        return MapToDetail(rule);
    }

    public async Task<NumberRuleDetailDto?> GetRuleByTypeAsync(string ruleType)
    {
        var rule = await _context.NumberRules.FirstOrDefaultAsync(r => r.RuleType == ruleType);
        if (rule == null) return null;
        return MapToDetail(rule);
    }

    public async Task<string> GenerateNumberAsync(string ruleType)
    {
        var rule = await _context.NumberRules.FirstOrDefaultAsync(r => r.RuleType == ruleType && r.IsActive);
        if (rule == null)
        {
            return $"{ruleType}{DateTime.UtcNow:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }

        var today = DateTime.UtcNow.ToString(rule.DateFormat);

        if (rule.ResetDaily && rule.LastDate != today)
        {
            rule.CurrentSequence = 0;
            rule.LastDate = today;
        }

        rule.CurrentSequence++;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var sequence = rule.CurrentSequence.ToString().PadLeft(rule.SequenceLength, '0');
        return $"{rule.Prefix}{today}{sequence}";
    }

    public async Task<int?> CreateRuleAsync(CreateNumberRuleRequest request)
    {
        if (await _context.NumberRules.AnyAsync(r => r.RuleType == request.RuleType))
        {
            _logger.LogWarning("建立編號規則失敗：規則類型已存在 - {RuleType}", request.RuleType);
            return null;
        }

        var rule = new NumberRule
        {
            RuleType = request.RuleType,
            Prefix = request.Prefix,
            DateFormat = request.DateFormat,
            SequenceLength = request.SequenceLength,
            ResetDaily = request.ResetDaily,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.NumberRules.Add(rule);
        await _context.SaveChangesAsync();

        return rule.Id;
    }

    public async Task<bool> UpdateRuleAsync(int id, UpdateNumberRuleRequest request)
    {
        var rule = await _context.NumberRules.FindAsync(id);
        if (rule == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Prefix)) rule.Prefix = request.Prefix;
        if (!string.IsNullOrWhiteSpace(request.DateFormat)) rule.DateFormat = request.DateFormat;
        if (request.SequenceLength.HasValue) rule.SequenceLength = request.SequenceLength.Value;
        if (request.ResetDaily.HasValue) rule.ResetDaily = request.ResetDaily.Value;
        if (request.IsActive.HasValue) rule.IsActive = request.IsActive.Value;
        if (request.ResetSequence.HasValue) rule.CurrentSequence = request.ResetSequence.Value;

        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteRuleAsync(int id)
    {
        var rule = await _context.NumberRules.FindAsync(id);
        if (rule == null) return false;

        _context.NumberRules.Remove(rule);
        await _context.SaveChangesAsync();

        return true;
    }

    private NumberRuleDetailDto MapToDetail(NumberRule rule)
    {
        var today = DateTime.UtcNow.ToString(rule.DateFormat);
        var nextSeq = rule.ResetDaily && rule.LastDate != today ? 1 : rule.CurrentSequence + 1;
        var nextNumber = $"{rule.Prefix}{today}{nextSeq.ToString().PadLeft(rule.SequenceLength, '0')}";

        return new NumberRuleDetailDto
        {
            Id = rule.Id,
            RuleType = rule.RuleType,
            Prefix = rule.Prefix,
            DateFormat = rule.DateFormat,
            SequenceLength = rule.SequenceLength,
            CurrentSequence = rule.CurrentSequence,
            LastDate = rule.LastDate,
            ResetDaily = rule.ResetDaily,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            NextNumber = nextNumber
        };
    }
}
