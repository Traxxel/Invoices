namespace Invoice.Application.DTOs;

public record ValidationResult(
    bool IsValid,
    List<ValidationError> Errors,
    List<ValidationWarning> Warnings,
    Dictionary<string, object> Data
);

public record ValidationError(
    string Field,
    string Code,
    string Message,
    object? Value,
    string? Suggestion
);

public record ValidationWarning(
    string Field,
    string Code,
    string Message,
    object? Value,
    string? Suggestion
);

public record ValidationRule(
    string Field,
    string Rule,
    string Message,
    object? Parameters
);

public record ValidationRequest(
    object Data,
    List<ValidationRule> Rules,
    bool StopOnFirstError
);

public record ValidationResponse(
    ValidationResult Result,
    TimeSpan ValidationTime,
    DateTime ValidatedAt
);

public record BusinessRuleValidation(
    string RuleName,
    string Description,
    bool IsValid,
    string Message,
    object? Data
);

public record BusinessRuleValidationResult(
    bool IsValid,
    List<BusinessRuleValidation> Rules,
    List<string> Violations,
    List<string> Recommendations
);

