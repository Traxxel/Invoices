using Invoice.Application.DTOs;

namespace Invoice.Application.Interfaces;

public interface IImportInvoiceUseCase
{
    Task<ImportResult> ExecuteAsync(ImportInvoiceRequest request);
    Task<ImportResult> ExecuteAsync(string filePath);
    Task<ImportResult> ExecuteAsync(Stream fileStream, string fileName);
    Task<ImportResult> ExecuteAsync(byte[] fileBytes, string fileName);

    // Validation
    Task<ValidationResult> ValidateImportRequestAsync(ImportInvoiceRequest request);
    Task<bool> IsValidPdfFileAsync(string filePath);
    Task<bool> IsValidPdfFileAsync(Stream fileStream);
    Task<bool> IsValidPdfFileAsync(byte[] fileBytes);

    // Pre-processing
    Task<ImportPreprocessingResult> PreprocessFileAsync(string filePath);
    Task<ImportPreprocessingResult> PreprocessFileAsync(Stream fileStream, string fileName);

    // Duplicate detection
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice);
    Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice);

    // Import options
    Task<ImportOptions> GetDefaultImportOptionsAsync();
    Task<ImportOptions> GetImportOptionsAsync(string filePath);
    Task<bool> UpdateImportOptionsAsync(ImportOptions options);
}

public record ImportInvoiceRequest(
    string FilePath,
    ImportOptions Options,
    string? UserId = null,
    string? SessionId = null
);

public record ImportResult(
    bool Success,
    string Message,
    InvoiceDto? Invoice,
    ExtractionResult? Extraction,
    List<ImportWarning> Warnings,
    List<ImportError> Errors,
    ImportStatistics Statistics,
    DateTime ImportedAt,
    TimeSpan ImportTime
);

public record ImportOptions(
    bool UseMLExtraction,
    bool RequireManualReview,
    float ConfidenceThreshold,
    bool CheckForDuplicates,
    bool AutoSave,
    bool CreateBackup,
    string ModelVersion,
    Dictionary<string, object> CustomSettings
);

public record ImportPreprocessingResult(
    bool Success,
    string Message,
    string ProcessedFilePath,
    long FileSize,
    string FileHash,
    int PageCount,
    List<PageInfo> Pages,
    List<ImportWarning> Warnings,
    List<ImportError> Errors
);

public record PageInfo(
    int PageNumber,
    float Width,
    float Height,
    int TextLineCount,
    int WordCount,
    List<Models.TextBlock> TextBlocks
);

public record DuplicateCheckResult(
    bool HasDuplicates,
    List<InvoiceDto> Duplicates,
    List<SimilarInvoice> SimilarInvoices,
    DuplicateMatchType MatchType,
    float SimilarityScore
);

public record SimilarInvoice(
    InvoiceDto Invoice,
    float SimilarityScore,
    List<string> MatchingFields,
    List<string> Differences
);

public enum DuplicateMatchType
{
    None,
    Exact,
    Similar,
    Potential
}

public record ImportWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record ImportError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record ImportStatistics(
    int TotalFiles,
    int SuccessfulImports,
    int FailedImports,
    int DuplicatesFound,
    int ManualReviewsRequired,
    TimeSpan TotalProcessingTime,
    float AverageConfidence,
    Dictionary<string, int> ExtractionsByField,
    Dictionary<string, int> ErrorsByType
);

