namespace Invoice.Domain.Validators;

public class ValidationResult
{
    private readonly List<ValidationError> _errors = new();
    private readonly List<ValidationWarning> _warnings = new();

    public bool IsValid => _errors.Count == 0;
    public bool HasWarnings => _warnings.Count > 0;
    public bool HasErrors => _errors.Count > 0;

    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();
    public IReadOnlyList<ValidationWarning> Warnings => _warnings.AsReadOnly();

    public void AddError(string field, string message)
    {
        _errors.Add(new ValidationError(field, message));
    }

    public void AddWarning(string field, string message)
    {
        _warnings.Add(new ValidationWarning(field, message));
    }

    public void AddErrors(IEnumerable<ValidationError> errors)
    {
        _errors.AddRange(errors);
    }

    public void AddWarnings(IEnumerable<ValidationWarning> warnings)
    {
        _warnings.AddRange(warnings);
    }

    public string GetErrorSummary()
    {
        if (!HasErrors) return string.Empty;

        var errorMessages = _errors.Select(e => $"{e.Field}: {e.Message}");
        return string.Join("; ", errorMessages);
    }

    public string GetWarningSummary()
    {
        if (!HasWarnings) return string.Empty;

        var warningMessages = _warnings.Select(w => $"{w.Field}: {w.Message}");
        return string.Join("; ", warningMessages);
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (HasErrors)
            parts.Add($"Errors: {GetErrorSummary()}");

        if (HasWarnings)
            parts.Add($"Warnings: {GetWarningSummary()}");

        return string.Join(" | ", parts);
    }
}

public record ValidationError(string Field, string Message);
public record ValidationWarning(string Field, string Message);

