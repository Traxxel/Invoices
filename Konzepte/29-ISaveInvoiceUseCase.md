# Aufgabe 29: ISaveInvoiceUseCase und Duplikatbehandlung

## Ziel

Save Invoice Use Case für das Speichern von Rechnungen mit Duplikatbehandlung und Validierung.

## 1. Save Invoice Use Case Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/ISaveInvoiceUseCase.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

public interface ISaveInvoiceUseCase
{
    Task<SaveInvoiceResult> ExecuteAsync(SaveInvoiceRequest request);
    Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice);
    Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice, SaveInvoiceOptions options);

    // Validation
    Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice);
    Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice, ValidationOptions options);
    Task<List<ValidationError>> ValidateFieldAsync(string fieldType, string value);
    Task<List<ValidationError>> ValidateFieldAsync(string fieldType, string value, ValidationOptions options);

    // Duplicate handling
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice);
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice, DuplicateCheckOptions options);
    Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice);
    Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice, SimilaritySearchOptions options);
    Task<DuplicateResolutionResult> ResolveDuplicatesAsync(InvoiceDto invoice, List<InvoiceDto> duplicates);
    Task<DuplicateResolutionResult> ResolveDuplicatesAsync(InvoiceDto invoice, List<InvoiceDto> duplicates, DuplicateResolutionOptions options);

    // Business rules
    Task<BusinessRulesResult> ApplyBusinessRulesAsync(InvoiceDto invoice);
    Task<BusinessRulesResult> ApplyBusinessRulesAsync(InvoiceDto invoice, BusinessRulesOptions options);
    Task<List<BusinessRule>> GetBusinessRulesAsync();
    Task<List<BusinessRule>> GetBusinessRulesAsync(string category);
    Task<bool> UpdateBusinessRulesAsync(List<BusinessRule> rules);

    // File operations
    Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath);
    Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, Stream fileStream, string fileName);
    Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, byte[] fileBytes, string fileName);
    Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath);
    Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, Stream fileStream, string fileName);
    Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, byte[] fileBytes, string fileName);

    // Database operations
    Task<DatabaseOperationResult> SaveInvoiceToDatabaseAsync(InvoiceDto invoice);
    Task<DatabaseOperationResult> UpdateInvoiceInDatabaseAsync(InvoiceDto invoice);
    Task<DatabaseOperationResult> DeleteInvoiceFromDatabaseAsync(Guid invoiceId);
    Task<DatabaseOperationResult> GetInvoiceFromDatabaseAsync(Guid invoiceId);
    Task<DatabaseOperationResult> SearchInvoicesInDatabaseAsync(InvoiceSearchCriteria criteria);

    // Audit and logging
    Task<AuditResult> LogInvoiceOperationAsync(InvoiceDto invoice, string operation, string userId);
    Task<AuditResult> LogInvoiceOperationAsync(InvoiceDto invoice, string operation, string userId, Dictionary<string, object> metadata);
    Task<List<AuditLogEntry>> GetInvoiceAuditLogAsync(Guid invoiceId);
    Task<List<AuditLogEntry>> GetInvoiceAuditLogAsync(Guid invoiceId, DateTime fromDate, DateTime toDate);

    // Statistics and reporting
    Task<SaveStatistics> GetSaveStatisticsAsync();
    Task<SaveStatistics> GetSaveStatisticsAsync(DateTime fromDate, DateTime toDate);
    Task<DuplicateStatistics> GetDuplicateStatisticsAsync();
    Task<DuplicateStatistics> GetDuplicateStatisticsAsync(DateTime fromDate, DateTime toDate);
    Task<ValidationStatistics> GetValidationStatisticsAsync();
    Task<ValidationStatistics> GetValidationStatisticsAsync(DateTime fromDate, DateTime toDate);
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

public record DuplicateCheckResult(
    bool HasDuplicates,
    List<InvoiceDto> Duplicates,
    List<SimilarInvoice> SimilarInvoices,
    DuplicateMatchType MatchType,
    float SimilarityScore,
    List<DuplicateField> DuplicateFields
);

public record SimilarInvoice(
    InvoiceDto Invoice,
    float SimilarityScore,
    List<string> MatchingFields,
    List<string> Differences,
    List<SimilarityMetric> Metrics
);

public record SimilarityMetric(
    string Field,
    float Similarity,
    string Source,
    object? Value1,
    object? Value2
);

public record DuplicateField(
    string FieldName,
    string Value,
    float Similarity,
    string Source
);

public enum DuplicateMatchType
{
    None,
    Exact,
    Similar,
    Potential
}

public record DuplicateCheckOptions(
    float SimilarityThreshold,
    List<string> FieldsToCheck,
    bool CheckExactMatches,
    bool CheckSimilarMatches,
    bool CheckPotentialMatches,
    Dictionary<string, object> CustomSettings
);

public record SimilaritySearchOptions(
    float SimilarityThreshold,
    List<string> FieldsToSearch,
    int MaxResults,
    bool IncludeExactMatches,
    bool IncludeSimilarMatches,
    Dictionary<string, object> CustomSettings
);

public record DuplicateResolutionResult(
    bool Success,
    string Message,
    InvoiceDto? ResolvedInvoice,
    List<DuplicateResolutionAction> Actions,
    List<DuplicateResolutionWarning> Warnings,
    List<DuplicateResolutionError> Errors
);

public record DuplicateResolutionAction(
    string Action,
    string Field,
    object? OldValue,
    object? NewValue,
    string Reason
);

public record DuplicateResolutionWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DuplicateResolutionError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record DuplicateResolutionOptions(
    DuplicateResolutionStrategy Strategy,
    bool MergeFields,
    bool KeepOriginal,
    bool UpdateOriginal,
    bool CreateNew,
    Dictionary<string, object> CustomSettings
);

public enum DuplicateResolutionStrategy
{
    KeepOriginal,
    UpdateOriginal,
    CreateNew,
    Merge,
    Skip
}

public record BusinessRulesResult(
    bool Success,
    string Message,
    List<BusinessRule> AppliedRules,
    List<BusinessRuleWarning> Warnings,
    List<BusinessRuleError> Errors,
    List<BusinessRuleSuggestion> Suggestions
);

public record BusinessRule(
    string Id,
    string Name,
    string Description,
    string Category,
    string Field,
    string Condition,
    string Action,
    int Priority,
    bool IsEnabled,
    Dictionary<string, object> Parameters
);

public record BusinessRuleWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record BusinessRuleError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record BusinessRuleSuggestion(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record BusinessRulesOptions(
    List<string> Categories,
    List<string> Fields,
    int MaxRules,
    bool IncludeDisabled,
    Dictionary<string, object> CustomSettings
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

public record InvoiceSearchCriteria(
    string? InvoiceNumber,
    string? IssuerName,
    DateOnly? FromDate,
    DateOnly? ToDate,
    decimal? MinAmount,
    decimal? MaxAmount,
    List<string> Fields,
    int MaxResults,
    int Skip,
    string? SortBy,
    bool SortDescending
);

public record AuditResult(
    bool Success,
    string Message,
    string AuditId,
    DateTime AuditedAt,
    List<AuditWarning> Warnings,
    List<AuditError> Errors
);

public record AuditWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record AuditError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record AuditLogEntry(
    string Id,
    Guid InvoiceId,
    string Operation,
    string UserId,
    DateTime Timestamp,
    Dictionary<string, object> Metadata,
    string? PreviousValue,
    string? NewValue
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

public record DuplicateStatistics(
    int TotalDuplicates,
    int ExactDuplicates,
    int SimilarDuplicates,
    int PotentialDuplicates,
    float AverageSimilarity,
    Dictionary<string, int> DuplicatesByField,
    Dictionary<string, int> DuplicatesByType
);

public record ValidationStatistics(
    int TotalValidations,
    int PassedValidations,
    int FailedValidations,
    int FieldValidations,
    int BusinessRuleValidations,
    float AverageConfidence,
    Dictionary<string, int> ValidationsByField,
    Dictionary<string, int> ValidationsByType
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
```

## 2. Save Invoice Use Case Implementation

**Datei:** `src/InvoiceReader.Application/UseCases/SaveInvoiceUseCase.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.DTOs;
using InvoiceReader.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.Application.UseCases;

public class SaveInvoiceUseCase : ISaveInvoiceUseCase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileHashService _fileHashService;
    private readonly IBusinessRulesService _businessRulesService;
    private readonly IValidationService _validationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<SaveInvoiceUseCase> _logger;

    public SaveInvoiceUseCase(
        IInvoiceRepository invoiceRepository,
        IFileStorageService fileStorageService,
        IFileHashService fileHashService,
        IBusinessRulesService businessRulesService,
        IValidationService validationService,
        IAuditService auditService,
        ILogger<SaveInvoiceUseCase> logger)
    {
        _invoiceRepository = invoiceRepository;
        _fileStorageService = fileStorageService;
        _fileHashService = fileHashService;
        _businessRulesService = businessRulesService;
        _validationService = validationService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<SaveInvoiceResult> ExecuteAsync(SaveInvoiceRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<SaveWarning>();
        var errors = new List<SaveError>();

        try
        {
            _logger.LogInformation("Starting invoice save: {InvoiceNumber}", request.Invoice.InvoiceNumber);

            // Validate invoice if required
            if (request.Options.RequireValidation)
            {
                var validation = await ValidateInvoiceAsync(request.Invoice);
                if (!validation.IsValid)
                {
                    return new SaveInvoiceResult(
                        false,
                        "Invoice validation failed",
                        null,
                        warnings,
                        validation.Errors.Select(e => new SaveError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                        new SaveStatistics(),
                        DateTime.UtcNow,
                        DateTime.UtcNow - startTime
                    );
                }
            }

            // Check for duplicates if required
            if (request.Options.CheckForDuplicates)
            {
                var duplicateCheck = await CheckForDuplicatesAsync(request.Invoice);
                if (duplicateCheck.HasDuplicates)
                {
                    warnings.Add(new SaveWarning(
                        "DUPLICATE_FOUND",
                        "Potential duplicate invoice found",
                        "Invoice",
                        request.Invoice.InvoiceNumber,
                        "Review duplicate invoices before saving"
                    ));

                    // Handle duplicates based on strategy
                    var resolution = await ResolveDuplicatesAsync(request.Invoice, duplicateCheck.Duplicates);
                    if (!resolution.Success)
                    {
                        return new SaveInvoiceResult(
                            false,
                            "Duplicate resolution failed",
                            null,
                            warnings,
                            resolution.Errors.Select(e => new SaveError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                            new SaveStatistics(),
                            DateTime.UtcNow,
                            DateTime.UtcNow - startTime
                        );
                    }
                }
            }

            // Apply business rules if required
            if (request.Options.ApplyBusinessRules)
            {
                var businessRules = await ApplyBusinessRulesAsync(request.Invoice);
                if (!businessRules.Success)
                {
                    warnings.Add(new SaveWarning(
                        "BUSINESS_RULES_FAILED",
                        "Business rules validation failed",
                        "Invoice",
                        request.Invoice.InvoiceNumber,
                        "Review business rules before saving"
                    ));
                }
            }

            // Store file if required
            if (request.Options.StoreFile && !string.IsNullOrEmpty(request.Invoice.SourceFilePath))
            {
                var fileOperation = await StoreInvoiceFileAsync(request.Invoice, request.Invoice.SourceFilePath);
                if (!fileOperation.Success)
                {
                    warnings.Add(new SaveWarning(
                        "FILE_STORAGE_FAILED",
                        "Failed to store invoice file",
                        "SourceFilePath",
                        request.Invoice.SourceFilePath,
                        "Check file storage configuration"
                    ));
                }
            }

            // Create backup if required
            if (request.Options.CreateBackup && !string.IsNullOrEmpty(request.Invoice.SourceFilePath))
            {
                var backupOperation = await BackupInvoiceFileAsync(request.Invoice, request.Invoice.SourceFilePath);
                if (!backupOperation.Success)
                {
                    warnings.Add(new SaveWarning(
                        "BACKUP_FAILED",
                        "Failed to create backup",
                        "SourceFilePath",
                        request.Invoice.SourceFilePath,
                        "Check backup configuration"
                    ));
                }
            }

            // Save to database
            var databaseOperation = await SaveInvoiceToDatabaseAsync(request.Invoice);
            if (!databaseOperation.Success)
            {
                return new SaveInvoiceResult(
                    false,
                    "Database save failed",
                    null,
                    warnings,
                    databaseOperation.Errors.Select(e => new SaveError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                    new SaveStatistics(),
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Log operation if required
            if (request.Options.LogOperation)
            {
                await LogInvoiceOperationAsync(request.Invoice, "Save", request.UserId ?? "System");
            }

            var statistics = new SaveStatistics(
                1, // TotalInvoices
                1, // SuccessfulSaves
                0, // FailedSaves
                duplicateCheck?.HasDuplicates == true ? 1 : 0, // DuplicatesFound
                1, // ValidationsPassed
                0, // ValidationsFailed
                businessRules?.AppliedRules?.Count ?? 0, // BusinessRulesApplied
                DateTime.UtcNow - startTime, // TotalSaveTime
                request.Invoice.ExtractionConfidence, // AverageConfidence
                new Dictionary<string, int>(), // SavesByField
                new Dictionary<string, int>() // ErrorsByType
            );

            _logger.LogInformation("Invoice save completed successfully: {InvoiceNumber}", request.Invoice.InvoiceNumber);

            return new SaveInvoiceResult(
                true,
                "Invoice saved successfully",
                request.Invoice,
                warnings,
                errors,
                statistics,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice save failed: {InvoiceNumber}", request.Invoice.InvoiceNumber);

            return new SaveInvoiceResult(
                false,
                "Invoice save failed",
                null,
                warnings,
                new List<SaveError> { new SaveError("SAVE_FAILED", ex.Message, "Invoice", request.Invoice.InvoiceNumber, ex) },
                new SaveStatistics(),
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice)
    {
        var options = new SaveInvoiceOptions(
            CheckForDuplicates: true,
            RequireValidation: true,
            ApplyBusinessRules: true,
            CreateBackup: true,
            StoreFile: true,
            LogOperation: true,
            DuplicateHandlingStrategy.DuplicateHandlingStrategy.AskUser,
            ValidationLevel.Standard,
            BusinessRulesLevel.Standard,
            FileStorageStrategy.Local,
            new Dictionary<string, object>()
        );

        var request = new SaveInvoiceRequest(invoice, options);
        return await ExecuteAsync(request);
    }

    public async Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice, SaveInvoiceOptions options)
    {
        var request = new SaveInvoiceRequest(invoice, options);
        return await ExecuteAsync(request);
    }

    public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Validating invoice: {InvoiceNumber}", invoice.InvoiceNumber);

            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            {
                errors.Add(new ValidationError("InvoiceNumber", "REQUIRED_FIELD", "Invoice number is required", null, null));
            }

            if (invoice.InvoiceDate == default)
            {
                errors.Add(new ValidationError("InvoiceDate", "REQUIRED_FIELD", "Invoice date is required", null, null));
            }

            if (invoice.GrossTotal <= 0)
            {
                errors.Add(new ValidationError("GrossTotal", "INVALID_AMOUNT", "Gross total must be greater than zero", invoice.GrossTotal, null));
            }

            // Validate field formats
            var fieldValidations = await _validationService.ValidateFieldsAsync(invoice);
            errors.AddRange(fieldValidations.Errors);
            warnings.AddRange(fieldValidations.Warnings);

            // Validate business rules
            var businessRules = await _businessRulesService.GetBusinessRulesAsync();
            foreach (var rule in businessRules)
            {
                var ruleResult = await _businessRulesService.ApplyRuleAsync(rule, invoice);
                if (!ruleResult.IsValid)
                {
                    if (ruleResult.IsError)
                    {
                        errors.Add(new ValidationError(rule.Field, ruleResult.Code, ruleResult.Message, ruleResult.Value, null));
                    }
                    else
                    {
                        warnings.Add(new ValidationWarning(rule.Field, ruleResult.Code, ruleResult.Message, ruleResult.Value, ruleResult.Suggestion));
                    }
                }
            }

            _logger.LogInformation("Invoice validation completed: {IsValid}, {ErrorCount} errors, {WarningCount} warnings",
                errors.Count == 0, errors.Count, warnings.Count);

            return new ValidationResult(errors.Count == 0, errors, warnings, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice validation failed: {InvoiceNumber}", invoice.InvoiceNumber);
            return new ValidationResult(false, new List<ValidationError> { new ValidationError("Validation", "VALIDATION_ERROR", "Validation process failed", null, ex.Message) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }
    }

    public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice, ValidationOptions options)
    {
        try
        {
            _logger.LogInformation("Validating invoice with options: {InvoiceNumber}", invoice.InvoiceNumber);

            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            // Apply validation level
            switch (options.ValidationLevel)
            {
                case ValidationLevel.Basic:
                    // Basic validation - only required fields
                    if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                        errors.Add(new ValidationError("InvoiceNumber", "REQUIRED_FIELD", "Invoice number is required", null, null));
                    break;

                case ValidationLevel.Standard:
                    // Standard validation - required fields + format validation
                    await ValidateInvoiceAsync(invoice);
                    break;

                case ValidationLevel.Strict:
                    // Strict validation - all validations
                    await ValidateInvoiceAsync(invoice);
                    // Additional strict validations
                    if (invoice.ExtractionConfidence < 0.8f)
                    {
                        warnings.Add(new ValidationWarning("ExtractionConfidence", "LOW_CONFIDENCE", "Extraction confidence is low", invoice.ExtractionConfidence, "Consider manual review"));
                    }
                    break;
            }

            return new ValidationResult(errors.Count == 0, errors, warnings, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice validation with options failed: {InvoiceNumber}", invoice.InvoiceNumber);
            return new ValidationResult(false, new List<ValidationError> { new ValidationError("Validation", "VALIDATION_ERROR", "Validation process failed", null, ex.Message) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }
    }

    public async Task<List<ValidationError>> ValidateFieldAsync(string fieldType, string value)
    {
        try
        {
            return await _validationService.ValidateFieldAsync(fieldType, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Field validation failed: {FieldType}", fieldType);
            return new List<ValidationError> { new ValidationError(fieldType, "VALIDATION_ERROR", "Field validation failed", value, ex.Message) };
        }
    }

    public async Task<List<ValidationError>> ValidateFieldAsync(string fieldType, string value, ValidationOptions options)
    {
        try
        {
            return await _validationService.ValidateFieldAsync(fieldType, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Field validation with options failed: {FieldType}", fieldType);
            return new List<ValidationError> { new ValidationError(fieldType, "VALIDATION_ERROR", "Field validation failed", value, ex.Message) };
        }
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Checking for duplicates: {InvoiceNumber}", invoice.InvoiceNumber);

            var duplicates = new List<InvoiceDto>();
            var similarInvoices = new List<SimilarInvoice>();
            var duplicateFields = new List<DuplicateField>();

            // Check for exact duplicates
            var exactDuplicates = await _invoiceRepository.SearchByInvoiceNumberAsync(invoice.InvoiceNumber);
            duplicates.AddRange(exactDuplicates.Where(d => d.GrossTotal == invoice.GrossTotal));

            // Check for similar invoices
            var similar = await _invoiceRepository.SearchByIssuerAsync(invoice.IssuerName);
            foreach (var similarInvoice in similar)
            {
                var similarity = CalculateSimilarity(invoice, similarInvoice);
                if (similarity > 0.8f)
                {
                    var matchingFields = GetMatchingFields(invoice, similarInvoice);
                    var differences = GetDifferences(invoice, similarInvoice);
                    var metrics = CalculateSimilarityMetrics(invoice, similarInvoice);

                    similarInvoices.Add(new SimilarInvoice(
                        similarInvoice,
                        similarity,
                        matchingFields,
                        differences,
                        metrics
                    ));
                }
            }

            // Identify duplicate fields
            foreach (var duplicate in duplicates)
            {
                if (duplicate.InvoiceNumber == invoice.InvoiceNumber)
                {
                    duplicateFields.Add(new DuplicateField("InvoiceNumber", duplicate.InvoiceNumber, 1.0f, "Exact"));
                }
                if (duplicate.GrossTotal == invoice.GrossTotal)
                {
                    duplicateFields.Add(new DuplicateField("GrossTotal", duplicate.GrossTotal.ToString(), 1.0f, "Exact"));
                }
            }

            var hasDuplicates = duplicates.Any() || similarInvoices.Any();
            var matchType = duplicates.Any() ? DuplicateMatchType.Exact :
                           similarInvoices.Any() ? DuplicateMatchType.Similar :
                           DuplicateMatchType.None;

            _logger.LogInformation("Duplicate check completed: {HasDuplicates}, {DuplicateCount} duplicates, {SimilarCount} similar",
                hasDuplicates, duplicates.Count, similarInvoices.Count);

            return new DuplicateCheckResult(
                hasDuplicates,
                duplicates,
                similarInvoices,
                matchType,
                similarInvoices.Any() ? similarInvoices.Max(s => s.SimilarityScore) : 0f,
                duplicateFields
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for duplicates: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DuplicateCheckResult(false, new List<InvoiceDto>(), new List<SimilarInvoice>(), DuplicateMatchType.None, 0f, new List<DuplicateField>());
        }
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice, DuplicateCheckOptions options)
    {
        try
        {
            _logger.LogInformation("Checking for duplicates with options: {InvoiceNumber}", invoice.InvoiceNumber);

            var duplicates = new List<InvoiceDto>();
            var similarInvoices = new List<SimilarInvoice>();
            var duplicateFields = new List<DuplicateField>();

            // Check exact matches if enabled
            if (options.CheckExactMatches)
            {
                var exactDuplicates = await _invoiceRepository.SearchByInvoiceNumberAsync(invoice.InvoiceNumber);
                duplicates.AddRange(exactDuplicates.Where(d => d.GrossTotal == invoice.GrossTotal));
            }

            // Check similar matches if enabled
            if (options.CheckSimilarMatches)
            {
                var similar = await _invoiceRepository.SearchByIssuerAsync(invoice.IssuerName);
                foreach (var similarInvoice in similar)
                {
                    var similarity = CalculateSimilarity(invoice, similarInvoice);
                    if (similarity >= options.SimilarityThreshold)
                    {
                        var matchingFields = GetMatchingFields(invoice, similarInvoice);
                        var differences = GetDifferences(invoice, similarInvoice);
                        var metrics = CalculateSimilarityMetrics(invoice, similarInvoice);

                        similarInvoices.Add(new SimilarInvoice(
                            similarInvoice,
                            similarity,
                            matchingFields,
                            differences,
                            metrics
                        ));
                    }
                }
            }

            // Check potential matches if enabled
            if (options.CheckPotentialMatches)
            {
                var potential = await _invoiceRepository.SearchByDateRangeAsync(invoice.InvoiceDate, invoice.InvoiceDate.AddDays(7));
                foreach (var potentialInvoice in potential)
                {
                    var similarity = CalculateSimilarity(invoice, potentialInvoice);
                    if (similarity >= options.SimilarityThreshold * 0.8f)
                    {
                        var matchingFields = GetMatchingFields(invoice, potentialInvoice);
                        var differences = GetDifferences(invoice, potentialInvoice);
                        var metrics = CalculateSimilarityMetrics(invoice, potentialInvoice);

                        similarInvoices.Add(new SimilarInvoice(
                            potentialInvoice,
                            similarity,
                            matchingFields,
                            differences,
                            metrics
                        ));
                    }
                }
            }

            var hasDuplicates = duplicates.Any() || similarInvoices.Any();
            var matchType = duplicates.Any() ? DuplicateMatchType.Exact :
                           similarInvoices.Any() ? DuplicateMatchType.Similar :
                           DuplicateMatchType.None;

            return new DuplicateCheckResult(
                hasDuplicates,
                duplicates,
                similarInvoices,
                matchType,
                similarInvoices.Any() ? similarInvoices.Max(s => s.SimilarityScore) : 0f,
                duplicateFields
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for duplicates with options: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DuplicateCheckResult(false, new List<InvoiceDto>(), new List<SimilarInvoice>(), DuplicateMatchType.None, 0f, new List<DuplicateField>());
        }
    }

    public async Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice)
    {
        try
        {
            var similar = await _invoiceRepository.SearchByIssuerAsync(invoice.IssuerName);
            return similar.Where(s => CalculateSimilarity(invoice, s) > 0.7f).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find similar invoices: {InvoiceNumber}", invoice.InvoiceNumber);
            return new List<InvoiceDto>();
        }
    }

    public async Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice, SimilaritySearchOptions options)
    {
        try
        {
            var similar = new List<InvoiceDto>();

            // Search by issuer if enabled
            if (options.FieldsToSearch.Contains("IssuerName"))
            {
                var byIssuer = await _invoiceRepository.SearchByIssuerAsync(invoice.IssuerName);
                similar.AddRange(byIssuer);
            }

            // Search by date range if enabled
            if (options.FieldsToSearch.Contains("InvoiceDate"))
            {
                var byDate = await _invoiceRepository.SearchByDateRangeAsync(invoice.InvoiceDate, invoice.InvoiceDate.AddDays(7));
                similar.AddRange(byDate);
            }

            // Search by amount if enabled
            if (options.FieldsToSearch.Contains("GrossTotal"))
            {
                var byAmount = await _invoiceRepository.SearchByAmountRangeAsync(invoice.GrossTotal * 0.9m, invoice.GrossTotal * 1.1m);
                similar.AddRange(byAmount);
            }

            // Filter by similarity threshold and max results
            var filtered = similar
                .Where(s => CalculateSimilarity(invoice, s) >= options.SimilarityThreshold)
                .OrderByDescending(s => CalculateSimilarity(invoice, s))
                .Take(options.MaxResults)
                .ToList();

            return filtered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find similar invoices with options: {InvoiceNumber}", invoice.InvoiceNumber);
            return new List<InvoiceDto>();
        }
    }

    public async Task<DuplicateResolutionResult> ResolveDuplicatesAsync(InvoiceDto invoice, List<InvoiceDto> duplicates)
    {
        try
        {
            _logger.LogInformation("Resolving duplicates: {InvoiceNumber}, {DuplicateCount} duplicates", invoice.InvoiceNumber, duplicates.Count);

            var actions = new List<DuplicateResolutionAction>();
            var warnings = new List<DuplicateResolutionWarning>();
            var errors = new List<DuplicateResolutionError>();

            // Default resolution strategy: keep original, update with new data
            foreach (var duplicate in duplicates)
            {
                // Merge fields that are different
                if (duplicate.InvoiceNumber != invoice.InvoiceNumber)
                {
                    actions.Add(new DuplicateResolutionAction(
                        "Update",
                        "InvoiceNumber",
                        duplicate.InvoiceNumber,
                        invoice.InvoiceNumber,
                        "Invoice number updated"
                    ));
                }

                if (duplicate.GrossTotal != invoice.GrossTotal)
                {
                    actions.Add(new DuplicateResolutionAction(
                        "Update",
                        "GrossTotal",
                        duplicate.GrossTotal,
                        invoice.GrossTotal,
                        "Gross total updated"
                    ));
                }
            }

            _logger.LogInformation("Duplicate resolution completed: {ActionCount} actions", actions.Count);

            return new DuplicateResolutionResult(
                true,
                "Duplicates resolved successfully",
                invoice,
                actions,
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve duplicates: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DuplicateResolutionResult(
                false,
                "Duplicate resolution failed",
                null,
                new List<DuplicateResolutionAction>(),
                new List<DuplicateResolutionWarning>(),
                new List<DuplicateResolutionError> { new DuplicateResolutionError("RESOLUTION_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) }
            );
        }
    }

    public async Task<DuplicateResolutionResult> ResolveDuplicatesAsync(InvoiceDto invoice, List<InvoiceDto> duplicates, DuplicateResolutionOptions options)
    {
        try
        {
            _logger.LogInformation("Resolving duplicates with options: {InvoiceNumber}, {DuplicateCount} duplicates", invoice.InvoiceNumber, duplicates.Count);

            var actions = new List<DuplicateResolutionAction>();
            var warnings = new List<DuplicateResolutionWarning>();
            var errors = new List<DuplicateResolutionError>();

            // Apply resolution strategy
            switch (options.Strategy)
            {
                case DuplicateResolutionStrategy.KeepOriginal:
                    // Keep original, skip new
                    actions.Add(new DuplicateResolutionAction(
                        "Skip",
                        "Invoice",
                        invoice.InvoiceNumber,
                        null,
                        "Keep original invoice"
                    ));
                    break;

                case DuplicateResolutionStrategy.UpdateOriginal:
                    // Update original with new data
                    foreach (var duplicate in duplicates)
                    {
                        if (duplicate.InvoiceNumber != invoice.InvoiceNumber)
                        {
                            actions.Add(new DuplicateResolutionAction(
                                "Update",
                                "InvoiceNumber",
                                duplicate.InvoiceNumber,
                                invoice.InvoiceNumber,
                                "Invoice number updated"
                            ));
                        }
                    }
                    break;

                case DuplicateResolutionStrategy.CreateNew:
                    // Create new invoice
                    actions.Add(new DuplicateResolutionAction(
                        "Create",
                        "Invoice",
                        null,
                        invoice.InvoiceNumber,
                        "Create new invoice"
                    ));
                    break;

                case DuplicateResolutionStrategy.Merge:
                    // Merge fields from duplicates
                    if (options.MergeFields)
                    {
                        foreach (var duplicate in duplicates)
                        {
                            // Merge non-empty fields
                            if (!string.IsNullOrWhiteSpace(duplicate.IssuerName) && string.IsNullOrWhiteSpace(invoice.IssuerName))
                            {
                                actions.Add(new DuplicateResolutionAction(
                                    "Merge",
                                    "IssuerName",
                                    invoice.IssuerName,
                                    duplicate.IssuerName,
                                    "Issuer name merged"
                                ));
                            }
                        }
                    }
                    break;
            }

            return new DuplicateResolutionResult(
                true,
                "Duplicates resolved successfully",
                invoice,
                actions,
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve duplicates with options: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DuplicateResolutionResult(
                false,
                "Duplicate resolution failed",
                null,
                new List<DuplicateResolutionAction>(),
                new List<DuplicateResolutionWarning>(),
                new List<DuplicateResolutionError> { new DuplicateResolutionError("RESOLUTION_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) }
            );
        }
    }

    public async Task<BusinessRulesResult> ApplyBusinessRulesAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Applying business rules: {InvoiceNumber}", invoice.InvoiceNumber);

            var rules = await _businessRulesService.GetBusinessRulesAsync();
            var appliedRules = new List<BusinessRule>();
            var warnings = new List<BusinessRuleWarning>();
            var errors = new List<BusinessRuleError>();
            var suggestions = new List<BusinessRuleSuggestion>();

            foreach (var rule in rules.Where(r => r.IsEnabled))
            {
                var ruleResult = await _businessRulesService.ApplyRuleAsync(rule, invoice);
                if (ruleResult.IsValid)
                {
                    appliedRules.Add(rule);
                }
                else
                {
                    if (ruleResult.IsError)
                    {
                        errors.Add(new BusinessRuleError(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.Field,
                            ruleResult.Value,
                            null
                        ));
                    }
                    else
                    {
                        warnings.Add(new BusinessRuleWarning(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.Field,
                            ruleResult.Value,
                            ruleResult.Suggestion
                        ));
                    }
                }
            }

            _logger.LogInformation("Business rules applied: {AppliedCount} applied, {WarningCount} warnings, {ErrorCount} errors",
                appliedRules.Count, warnings.Count, errors.Count);

            return new BusinessRulesResult(
                errors.Count == 0,
                "Business rules applied successfully",
                appliedRules,
                warnings,
                errors,
                suggestions
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply business rules: {InvoiceNumber}", invoice.InvoiceNumber);
            return new BusinessRulesResult(
                false,
                "Business rules application failed",
                new List<BusinessRule>(),
                new List<BusinessRuleWarning>(),
                new List<BusinessRuleError> { new BusinessRuleError("BUSINESS_RULES_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) },
                new List<BusinessRuleSuggestion>()
            );
        }
    }

    public async Task<BusinessRulesResult> ApplyBusinessRulesAsync(InvoiceDto invoice, BusinessRulesOptions options)
    {
        try
        {
            _logger.LogInformation("Applying business rules with options: {InvoiceNumber}", invoice.InvoiceNumber);

            var rules = await _businessRulesService.GetBusinessRulesAsync();
            var filteredRules = rules.Where(r => r.IsEnabled);

            // Filter by categories if specified
            if (options.Categories.Any())
            {
                filteredRules = filteredRules.Where(r => options.Categories.Contains(r.Category));
            }

            // Filter by fields if specified
            if (options.Fields.Any())
            {
                filteredRules = filteredRules.Where(r => options.Fields.Contains(r.Field));
            }

            // Apply max rules limit
            if (options.MaxRules > 0)
            {
                filteredRules = filteredRules.Take(options.MaxRules);
            }

            var appliedRules = new List<BusinessRule>();
            var warnings = new List<BusinessRuleWarning>();
            var errors = new List<BusinessRuleError>();
            var suggestions = new List<BusinessRuleSuggestion>();

            foreach (var rule in filteredRules)
            {
                var ruleResult = await _businessRulesService.ApplyRuleAsync(rule, invoice);
                if (ruleResult.IsValid)
                {
                    appliedRules.Add(rule);
                }
                else
                {
                    if (ruleResult.IsError)
                    {
                        errors.Add(new BusinessRuleError(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.Field,
                            ruleResult.Value,
                            null
                        ));
                    }
                    else
                    {
                        warnings.Add(new BusinessRuleWarning(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.Field,
                            ruleResult.Value,
                            ruleResult.Suggestion
                        ));
                    }
                }
            }

            return new BusinessRulesResult(
                errors.Count == 0,
                "Business rules applied successfully",
                appliedRules,
                warnings,
                errors,
                suggestions
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply business rules with options: {InvoiceNumber}", invoice.InvoiceNumber);
            return new BusinessRulesResult(
                false,
                "Business rules application failed",
                new List<BusinessRule>(),
                new List<BusinessRuleWarning>(),
                new List<BusinessRuleError> { new BusinessRuleError("BUSINESS_RULES_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) },
                new List<BusinessRuleSuggestion>()
            );
        }
    }

    public async Task<List<BusinessRule>> GetBusinessRulesAsync()
    {
        try
        {
            return await _businessRulesService.GetBusinessRulesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get business rules");
            return new List<BusinessRule>();
        }
    }

    public async Task<List<BusinessRule>> GetBusinessRulesAsync(string category)
    {
        try
        {
            var rules = await _businessRulesService.GetBusinessRulesAsync();
            return rules.Where(r => r.Category == category).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get business rules for category: {Category}", category);
            return new List<BusinessRule>();
        }
    }

    public async Task<bool> UpdateBusinessRulesAsync(List<BusinessRule> rules)
    {
        try
        {
            return await _businessRulesService.UpdateBusinessRulesAsync(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update business rules");
            return false;
        }
    }

    public async Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath)
    {
        try
        {
            _logger.LogInformation("Storing invoice file: {InvoiceNumber}, {SourceFilePath}", invoice.InvoiceNumber, sourceFilePath);

            var storedFilePath = await _fileStorageService.StoreFileAsync(sourceFilePath, invoice.Id);
            var fileInfo = new FileInfo(storedFilePath);
            var fileHash = await _fileHashService.CalculateHashAsync(storedFilePath);

            _logger.LogInformation("Invoice file stored successfully: {StoredFilePath}", storedFilePath);

            return new FileOperationResult(
                true,
                "File stored successfully",
                storedFilePath,
                fileInfo.Length,
                fileHash,
                new List<FileOperationWarning>(),
                new List<FileOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store invoice file: {InvoiceNumber}, {SourceFilePath}", invoice.InvoiceNumber, sourceFilePath);
            return new FileOperationResult(
                false,
                "File storage failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("FILE_STORAGE_FAILED", ex.Message, "SourceFilePath", sourceFilePath, ex) }
            );
        }
    }

    public async Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, Stream fileStream, string fileName)
    {
        try
        {
            _logger.LogInformation("Storing invoice file from stream: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);

            var storedFilePath = await _fileStorageService.StoreFileAsync(fileStream, fileName, invoice.Id);
            var fileInfo = new FileInfo(storedFilePath);
            var fileHash = await _fileHashService.CalculateHashAsync(storedFilePath);

            _logger.LogInformation("Invoice file stored successfully from stream: {StoredFilePath}", storedFilePath);

            return new FileOperationResult(
                true,
                "File stored successfully",
                storedFilePath,
                fileInfo.Length,
                fileHash,
                new List<FileOperationWarning>(),
                new List<FileOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store invoice file from stream: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);
            return new FileOperationResult(
                false,
                "File storage failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("FILE_STORAGE_FAILED", ex.Message, "FileName", fileName, ex) }
            );
        }
    }

    public async Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, byte[] fileBytes, string fileName)
    {
        try
        {
            using var stream = new MemoryStream(fileBytes);
            return await StoreInvoiceFileAsync(invoice, stream, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store invoice file from bytes: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);
            return new FileOperationResult(
                false,
                "File storage failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("FILE_STORAGE_FAILED", ex.Message, "FileName", fileName, ex) }
            );
        }
    }

    public async Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath)
    {
        try
        {
            _logger.LogInformation("Creating backup for invoice file: {InvoiceNumber}, {SourceFilePath}", invoice.InvoiceNumber, sourceFilePath);

            var backupFilePath = await _fileStorageService.CreateBackupAsync(sourceFilePath, invoice.Id);
            var fileInfo = new FileInfo(backupFilePath);
            var fileHash = await _fileHashService.CalculateHashAsync(backupFilePath);

            _logger.LogInformation("Invoice file backup created successfully: {BackupFilePath}", backupFilePath);

            return new FileOperationResult(
                true,
                "Backup created successfully",
                backupFilePath,
                fileInfo.Length,
                fileHash,
                new List<FileOperationWarning>(),
                new List<FileOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for invoice file: {InvoiceNumber}, {SourceFilePath}", invoice.InvoiceNumber, sourceFilePath);
            return new FileOperationResult(
                false,
                "Backup creation failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("BACKUP_FAILED", ex.Message, "SourceFilePath", sourceFilePath, ex) }
            );
        }
    }

    public async Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, Stream fileStream, string fileName)
    {
        try
        {
            _logger.LogInformation("Creating backup for invoice file from stream: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);

            var backupFilePath = await _fileStorageService.CreateBackupAsync(fileStream, fileName, invoice.Id);
            var fileInfo = new FileInfo(backupFilePath);
            var fileHash = await _fileHashService.CalculateHashAsync(backupFilePath);

            _logger.LogInformation("Invoice file backup created successfully from stream: {BackupFilePath}", backupFilePath);

            return new FileOperationResult(
                true,
                "Backup created successfully",
                backupFilePath,
                fileInfo.Length,
                fileHash,
                new List<FileOperationWarning>(),
                new List<FileOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for invoice file from stream: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);
            return new FileOperationResult(
                false,
                "Backup creation failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("BACKUP_FAILED", ex.Message, "FileName", fileName, ex) }
            );
        }
    }

    public async Task<FileOperationResult> BackupInvoiceFileAsync(InvoiceDto invoice, byte[] fileBytes, string fileName)
    {
        try
        {
            using var stream = new MemoryStream(fileBytes);
            return await BackupInvoiceFileAsync(invoice, stream, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for invoice file from bytes: {InvoiceNumber}, {FileName}", invoice.InvoiceNumber, fileName);
            return new FileOperationResult(
                false,
                "Backup creation failed",
                null,
                null,
                null,
                new List<FileOperationWarning>(),
                new List<FileOperationError> { new FileOperationError("BACKUP_FAILED", ex.Message, "FileName", fileName, ex) }
            );
        }
    }

    public async Task<DatabaseOperationResult> SaveInvoiceToDatabaseAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Saving invoice to database: {InvoiceNumber}", invoice.InvoiceNumber);

            var entity = invoice.ToEntity();
            await _invoiceRepository.AddAsync(entity);
            await _invoiceRepository.SaveChangesAsync();

            _logger.LogInformation("Invoice saved to database successfully: {InvoiceNumber}", invoice.InvoiceNumber);

            return new DatabaseOperationResult(
                true,
                "Invoice saved to database successfully",
                invoice,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save invoice to database: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DatabaseOperationResult(
                false,
                "Database save failed",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError> { new DatabaseOperationError("DATABASE_SAVE_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) }
            );
        }
    }

    public async Task<DatabaseOperationResult> UpdateInvoiceInDatabaseAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Updating invoice in database: {InvoiceNumber}", invoice.InvoiceNumber);

            var entity = invoice.ToEntity();
            _invoiceRepository.Update(entity);
            await _invoiceRepository.SaveChangesAsync();

            _logger.LogInformation("Invoice updated in database successfully: {InvoiceNumber}", invoice.InvoiceNumber);

            return new DatabaseOperationResult(
                true,
                "Invoice updated in database successfully",
                invoice,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update invoice in database: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DatabaseOperationResult(
                false,
                "Database update failed",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError> { new DatabaseOperationError("DATABASE_UPDATE_FAILED", ex.Message, "Invoice", invoice.InvoiceNumber, ex) }
            );
        }
    }

    public async Task<DatabaseOperationResult> DeleteInvoiceFromDatabaseAsync(Guid invoiceId)
    {
        try
        {
            _logger.LogInformation("Deleting invoice from database: {InvoiceId}", invoiceId);

            await _invoiceRepository.DeleteAsync(invoiceId);
            await _invoiceRepository.SaveChangesAsync();

            _logger.LogInformation("Invoice deleted from database successfully: {InvoiceId}", invoiceId);

            return new DatabaseOperationResult(
                true,
                "Invoice deleted from database successfully",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete invoice from database: {InvoiceId}", invoiceId);
            return new DatabaseOperationResult(
                false,
                "Database delete failed",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError> { new DatabaseOperationError("DATABASE_DELETE_FAILED", ex.Message, "InvoiceId", invoiceId.ToString(), ex) }
            );
        }
    }

    public async Task<DatabaseOperationResult> GetInvoiceFromDatabaseAsync(Guid invoiceId)
    {
        try
        {
            _logger.LogInformation("Getting invoice from database: {InvoiceId}", invoiceId);

            var entity = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (entity == null)
            {
                return new DatabaseOperationResult(
                    false,
                    "Invoice not found",
                    null,
                    new List<DatabaseOperationWarning>(),
                    new List<DatabaseOperationError> { new DatabaseOperationError("INVOICE_NOT_FOUND", "Invoice not found", "InvoiceId", invoiceId.ToString(), null) }
                );
            }

            var dto = entity.ToDto();

            _logger.LogInformation("Invoice retrieved from database successfully: {InvoiceNumber}", dto.InvoiceNumber);

            return new DatabaseOperationResult(
                true,
                "Invoice retrieved from database successfully",
                dto,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get invoice from database: {InvoiceId}", invoiceId);
            return new DatabaseOperationResult(
                false,
                "Database retrieval failed",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError> { new DatabaseOperationError("DATABASE_RETRIEVAL_FAILED", ex.Message, "InvoiceId", invoiceId.ToString(), ex) }
            );
        }
    }

    public async Task<DatabaseOperationResult> SearchInvoicesInDatabaseAsync(InvoiceSearchCriteria criteria)
    {
        try
        {
            _logger.LogInformation("Searching invoices in database with criteria");

            var entities = await _invoiceRepository.SearchAsync(criteria);
            var dtos = entities.Select(e => e.ToDto()).ToList();

            _logger.LogInformation("Invoice search completed: {Count} results", dtos.Count);

            return new DatabaseOperationResult(
                true,
                "Invoice search completed successfully",
                null, // Return first result or null for search
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search invoices in database");
            return new DatabaseOperationResult(
                false,
                "Database search failed",
                null,
                new List<DatabaseOperationWarning>(),
                new List<DatabaseOperationError> { new DatabaseOperationError("DATABASE_SEARCH_FAILED", ex.Message, "Criteria", criteria.ToString(), ex) }
            );
        }
    }

    public async Task<AuditResult> LogInvoiceOperationAsync(InvoiceDto invoice, string operation, string userId)
    {
        try
        {
            _logger.LogInformation("Logging invoice operation: {Operation}, {InvoiceNumber}, {UserId}", operation, invoice.InvoiceNumber, userId);

            var auditId = await _auditService.LogOperationAsync(invoice.Id, operation, userId, new Dictionary<string, object>());

            _logger.LogInformation("Invoice operation logged successfully: {AuditId}", auditId);

            return new AuditResult(
                true,
                "Operation logged successfully",
                auditId,
                DateTime.UtcNow,
                new List<AuditWarning>(),
                new List<AuditError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log invoice operation: {Operation}, {InvoiceNumber}, {UserId}", operation, invoice.InvoiceNumber, userId);
            return new AuditResult(
                false,
                "Operation logging failed",
                string.Empty,
                DateTime.UtcNow,
                new List<AuditWarning>(),
                new List<AuditError> { new AuditError("AUDIT_LOGGING_FAILED", ex.Message, "Operation", operation, ex) }
            );
        }
    }

    public async Task<AuditResult> LogInvoiceOperationAsync(InvoiceDto invoice, string operation, string userId, Dictionary<string, object> metadata)
    {
        try
        {
            _logger.LogInformation("Logging invoice operation with metadata: {Operation}, {InvoiceNumber}, {UserId}", operation, invoice.InvoiceNumber, userId);

            var auditId = await _auditService.LogOperationAsync(invoice.Id, operation, userId, metadata);

            _logger.LogInformation("Invoice operation logged successfully with metadata: {AuditId}", auditId);

            return new AuditResult(
                true,
                "Operation logged successfully",
                auditId,
                DateTime.UtcNow,
                new List<AuditWarning>(),
                new List<AuditError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log invoice operation with metadata: {Operation}, {InvoiceNumber}, {UserId}", operation, invoice.InvoiceNumber, userId);
            return new AuditResult(
                false,
                "Operation logging failed",
                string.Empty,
                DateTime.UtcNow,
                new List<AuditWarning>(),
                new List<AuditError> { new AuditError("AUDIT_LOGGING_FAILED", ex.Message, "Operation", operation, ex) }
            );
        }
    }

    public async Task<List<AuditLogEntry>> GetInvoiceAuditLogAsync(Guid invoiceId)
    {
        try
        {
            _logger.LogInformation("Getting audit log for invoice: {InvoiceId}", invoiceId);

            var entries = await _auditService.GetAuditLogAsync(invoiceId);

            _logger.LogInformation("Audit log retrieved successfully: {Count} entries", entries.Count);

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log for invoice: {InvoiceId}", invoiceId);
            return new List<AuditLogEntry>();
        }
    }

    public async Task<List<AuditLogEntry>> GetInvoiceAuditLogAsync(Guid invoiceId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting audit log for invoice with date range: {InvoiceId}, {FromDate}, {ToDate}", invoiceId, fromDate, toDate);

            var entries = await _auditService.GetAuditLogAsync(invoiceId, fromDate, toDate);

            _logger.LogInformation("Audit log retrieved successfully with date range: {Count} entries", entries.Count);

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log for invoice with date range: {InvoiceId}", invoiceId);
            return new List<AuditLogEntry>();
        }
    }

    public async Task<SaveStatistics> GetSaveStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting save statistics");

            var statistics = await _auditService.GetSaveStatisticsAsync();

            _logger.LogInformation("Save statistics retrieved successfully");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get save statistics");
            return new SaveStatistics();
        }
    }

    public async Task<SaveStatistics> GetSaveStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting save statistics for date range: {FromDate}, {ToDate}", fromDate, toDate);

            var statistics = await _auditService.GetSaveStatisticsAsync(fromDate, toDate);

            _logger.LogInformation("Save statistics retrieved successfully for date range");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get save statistics for date range");
            return new SaveStatistics();
        }
    }

    public async Task<DuplicateStatistics> GetDuplicateStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting duplicate statistics");

            var statistics = await _auditService.GetDuplicateStatisticsAsync();

            _logger.LogInformation("Duplicate statistics retrieved successfully");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get duplicate statistics");
            return new DuplicateStatistics();
        }
    }

    public async Task<DuplicateStatistics> GetDuplicateStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting duplicate statistics for date range: {FromDate}, {ToDate}", fromDate, toDate);

            var statistics = await _auditService.GetDuplicateStatisticsAsync(fromDate, toDate);

            _logger.LogInformation("Duplicate statistics retrieved successfully for date range");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get duplicate statistics for date range");
            return new DuplicateStatistics();
        }
    }

    public async Task<ValidationStatistics> GetValidationStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting validation statistics");

            var statistics = await _auditService.GetValidationStatisticsAsync();

            _logger.LogInformation("Validation statistics retrieved successfully");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get validation statistics");
            return new ValidationStatistics();
        }
    }

    public async Task<ValidationStatistics> GetValidationStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting validation statistics for date range: {FromDate}, {ToDate}", fromDate, toDate);

            var statistics = await _auditService.GetValidationStatisticsAsync(fromDate, toDate);

            _logger.LogInformation("Validation statistics retrieved successfully for date range");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get validation statistics for date range");
            return new ValidationStatistics();
        }
    }

    private float CalculateSimilarity(InvoiceDto invoice1, InvoiceDto invoice2)
    {
        var similarity = 0f;
        var factors = 0;

        if (invoice1.InvoiceNumber == invoice2.InvoiceNumber)
        {
            similarity += 0.4f;
            factors++;
        }

        if (invoice1.IssuerName == invoice2.IssuerName)
        {
            similarity += 0.3f;
            factors++;
        }

        if (Math.Abs(invoice1.GrossTotal - invoice2.GrossTotal) < 0.01m)
        {
            similarity += 0.3f;
            factors++;
        }

        return factors > 0 ? similarity / factors : 0f;
    }

    private List<string> GetMatchingFields(InvoiceDto invoice1, InvoiceDto invoice2)
    {
        var matches = new List<string>();

        if (invoice1.InvoiceNumber == invoice2.InvoiceNumber)
            matches.Add("InvoiceNumber");

        if (invoice1.IssuerName == invoice2.IssuerName)
            matches.Add("IssuerName");

        if (Math.Abs(invoice1.GrossTotal - invoice2.GrossTotal) < 0.01m)
            matches.Add("GrossTotal");

        return matches;
    }

    private List<string> GetDifferences(InvoiceDto invoice1, InvoiceDto invoice2)
    {
        var differences = new List<string>();

        if (invoice1.InvoiceNumber != invoice2.InvoiceNumber)
            differences.Add("InvoiceNumber");

        if (invoice1.IssuerName != invoice2.IssuerName)
            differences.Add("IssuerName");

        if (Math.Abs(invoice1.GrossTotal - invoice2.GrossTotal) >= 0.01m)
            differences.Add("GrossTotal");

        return differences;
    }

    private List<SimilarityMetric> CalculateSimilarityMetrics(InvoiceDto invoice1, InvoiceDto invoice2)
    {
        var metrics = new List<SimilarityMetric>();

        // Invoice number similarity
        var invoiceNumberSimilarity = invoice1.InvoiceNumber == invoice2.InvoiceNumber ? 1.0f : 0.0f;
        metrics.Add(new SimilarityMetric("InvoiceNumber", invoiceNumberSimilarity, "Exact", invoice1.InvoiceNumber, invoice2.InvoiceNumber));

        // Issuer name similarity
        var issuerNameSimilarity = CalculateStringSimilarity(invoice1.IssuerName, invoice2.IssuerName);
        metrics.Add(new SimilarityMetric("IssuerName", issuerNameSimilarity, "String", invoice1.IssuerName, invoice2.IssuerName));

        // Gross total similarity
        var grossTotalSimilarity = Math.Abs(invoice1.GrossTotal - invoice2.GrossTotal) < 0.01m ? 1.0f : 0.0f;
        metrics.Add(new SimilarityMetric("GrossTotal", grossTotalSimilarity, "Exact", invoice1.GrossTotal, invoice2.GrossTotal));

        return metrics;
    }

    private float CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
            return 1.0f;

        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0f;

        if (str1 == str2)
            return 1.0f;

        // Simple Levenshtein distance-based similarity
        var distance = LevenshteinDistance(str1, str2);
        var maxLength = Math.Max(str1.Length, str2.Length);

        return 1.0f - (float)distance / maxLength;
    }

    private int LevenshteinDistance(string str1, string str2)
    {
        var matrix = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[str1.Length, str2.Length];
    }
}
```

## 3. Save Invoice Use Case Extensions

**Datei:** `src/InvoiceReader.Application/Extensions/SaveInvoiceExtensions.cs`

```csharp
using InvoiceReader.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Application.Extensions;

public static class SaveInvoiceExtensions
{
    public static IServiceCollection AddSaveInvoiceServices(this IServiceCollection services)
    {
        services.AddScoped<ISaveInvoiceUseCase, SaveInvoiceUseCase>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Save Invoice Use Case für das Speichern von Rechnungen
- Duplikatbehandlung mit verschiedenen Strategien
- Business Rules für fachliche Validierung
- File Operations für Dateispeicherung und Backup
- Database Operations für Datenbankoperationen
- Audit und Logging für alle Operationen
- Statistics und Reporting für Monitoring
- Error Handling für alle Save-Operationen
- Validation für alle Rechnungsfelder
- Similarity Calculation für Duplikaterkennung
