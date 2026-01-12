using System.ComponentModel.DataAnnotations;

namespace DotnetApiDemo.Models.DTOs.NumberRules;

public class NumberRuleListDto
{
    public int Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public int SequenceLength { get; set; }
    public bool ResetDaily { get; set; }
    public bool IsActive { get; set; }
}

public class NumberRuleDetailDto
{
    public int Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public int SequenceLength { get; set; }
    public int CurrentSequence { get; set; }
    public string? LastDate { get; set; }
    public bool ResetDaily { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string NextNumber { get; set; } = string.Empty;
}

public class CreateNumberRuleRequest
{
    [Required] public string RuleType { get; set; } = string.Empty;
    [Required] public string Prefix { get; set; } = string.Empty;
    public string DateFormat { get; set; } = "yyyyMMdd";
    [Range(1, 10)] public int SequenceLength { get; set; } = 4;
    public bool ResetDaily { get; set; } = true;
}

public class UpdateNumberRuleRequest
{
    public string? Prefix { get; set; }
    public string? DateFormat { get; set; }
    public int? SequenceLength { get; set; }
    public bool? ResetDaily { get; set; }
    public bool? IsActive { get; set; }
    public int? ResetSequence { get; set; }
}
