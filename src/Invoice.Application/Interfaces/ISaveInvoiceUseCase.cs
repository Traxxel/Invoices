using Invoice.Application.DTOs;

namespace Invoice.Application.Interfaces;

public interface ISaveInvoiceUseCase
{
    Task<SaveInvoiceResult> ExecuteAsync(SaveInvoiceRequest request);
    Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice);
    Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice, SaveInvoiceOptions options);

    // Validation
    Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice);

    // Duplicate handling
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice);
    Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice);

    // File operations
    Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath);

    // Database operations
    Task<DatabaseOperationResult> SaveInvoiceToDatabaseAsync(InvoiceDto invoice);
}

public record SaveInvoiceRequest(
    InvoiceDto Invoice,
    SaveInvoiceOptions Options,
    string? UserId = null,
    string? SessionId = null
);

public record SaveInvoiceOptions(
    bool CheckForDuplicates,
    bool RequireValidation,
    bool ApplyBusinessRules,
    bool CreateBackup,
    bool StoreFile,
    bool LogOperation,
    DuplicateHandlingStrategy DuplicateStrategy,
    ValidationLevel ValidationLevel,
    BusinessRulesLevel BusinessRulesLevel,
    FileStorageStrategy FileStorageStrategy,
    Dictionary<string, object> CustomSettings
);

public record SaveInvoiceResult(
    bool Success,
    string Message,
    InvoiceDto? Invoice,
    List<SaveWarning> Warnings,
    List<SaveError> Errors,
    SaveStatistics Statistics,
    DateTime SavedAt,
    TimeSpan SaveTime
);

public record SaveWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record SaveError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record SaveStatistics(
    int TotalInvoices,
    int SuccessfulSaves,
    int FailedSaves,
    int DuplicatesFound,
    int ValidationsPassed,
    int ValidationsFailed,
    int BusinessRulesApplied,
    TimeSpan TotalSaveTime,
    float AverageConfidence,
    Dictionary<string, int> SavesByField,
    Dictionary<string, int> ErrorsByType
);

public record FileOperationResult(
    bool Success,
    string Message,
    string? FilePath,
    long? FileSize,
    string? FileHash,
    List<FileOperationWarning> Warnings,
    List<FileOperationError> Errors
);

public record FileOperationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record FileOperationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public enum FileStorageStrategy
{
    Local,
    Network,
    Cloud,
    Hybrid
}

public record DatabaseOperationResult(
    bool Success,
    string Message,
    InvoiceDto? Invoice,
    List<DatabaseOperationWarning> Warnings,
    List<DatabaseOperationError> Errors
);

public record DatabaseOperationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DatabaseOperationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public enum DuplicateHandlingStrategy
{
    Skip,
    Replace,
    Merge,
    CreateNew,
    AskUser
}

public enum ValidationLevel
{
    None,
    Basic,
    Standard,
    Strict
}

public enum BusinessRulesLevel
{
    None,
    Basic,
    Standard,
    Strict
}

