# Aufgabe 26: DTOs (InvoiceDto, ExtractionResult, etc.)

## Ziel

Data Transfer Objects für saubere Trennung zwischen Domain und Application Layer.

## 1. Invoice DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/InvoiceDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    string IssuerName,
    string IssuerStreet,
    string IssuerPostalCode,
    string IssuerCity,
    string? IssuerCountry,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    string SourceFilePath,
    DateTime ImportedAt,
    float ExtractionConfidence,
    string ModelVersion
);

public record InvoiceCreateDto(
    string InvoiceNumber,
    DateOnly InvoiceDate,
    string IssuerName,
    string IssuerStreet,
    string IssuerPostalCode,
    string IssuerCity,
    string? IssuerCountry,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    string SourceFilePath,
    float ExtractionConfidence = 0.0f,
    string ModelVersion = ""
);

public record InvoiceUpdateDto(
    Guid Id,
    string? InvoiceNumber,
    DateOnly? InvoiceDate,
    string? IssuerName,
    string? IssuerStreet,
    string? IssuerPostalCode,
    string? IssuerCity,
    string? IssuerCountry,
    decimal? NetTotal,
    decimal? VatTotal,
    decimal? GrossTotal,
    float? ExtractionConfidence,
    string? ModelVersion
);

public record InvoiceSearchDto(
    string? InvoiceNumber,
    string? IssuerName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? MinAmount,
    decimal? MaxAmount,
    float? MinConfidence,
    string? ModelVersion,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "ImportedAt",
    bool SortDescending = true
);

public record InvoiceSummaryDto(
    int TotalCount,
    decimal TotalAmount,
    decimal AverageAmount,
    DateOnly? EarliestDate,
    DateOnly? LatestDate,
    Dictionary<string, int> IssuerCounts,
    Dictionary<string, int> ModelVersionCounts,
    Dictionary<string, int> ConfidenceLevelCounts
);
```

## 2. Extraction DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/ExtractionDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record ExtractionResult(
    bool Success,
    string Message,
    List<ExtractedField> Fields,
    List<ExtractionWarning> Warnings,
    List<ExtractionError> Errors,
    float OverallConfidence,
    string ModelVersion,
    DateTime ExtractedAt,
    TimeSpan ExtractionTime
);

public record ExtractedField(
    string FieldType,
    string Value,
    float Confidence,
    string OriginalText,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    List<AlternativeValue> Alternatives
);

public record AlternativeValue(
    string Value,
    float Confidence,
    string Source
);

public record ExtractionWarning(
    string Code,
    string Message,
    string FieldType,
    string Value,
    int LineIndex
);

public record ExtractionError(
    string Code,
    string Message,
    string FieldType,
    string Value,
    int LineIndex,
    Exception? Exception
);

public record FieldExtractionRequest(
    string Text,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string Context
);

public record FieldExtractionResponse(
    string FieldType,
    string Value,
    float Confidence,
    List<AlternativeValue> Alternatives,
    bool IsHighConfidence,
    bool IsLowConfidence,
    string Recommendation
);

public record BatchExtractionRequest(
    List<FieldExtractionRequest> Fields,
    string DocumentId,
    string ModelVersion
);

public record BatchExtractionResponse(
    List<FieldExtractionResponse> Fields,
    float OverallConfidence,
    int HighConfidenceCount,
    int LowConfidenceCount,
    List<ExtractionWarning> Warnings,
    List<ExtractionError> Errors,
    TimeSpan ProcessingTime
);
```

## 3. Training DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/TrainingDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record TrainingRequest(
    string ModelName,
    string Version,
    List<TrainingSample> Samples,
    TrainingOptions Options
);

public record TrainingSample(
    string Text,
    string Label,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string DocumentId,
    Dictionary<string, object> Features
);

public record TrainingOptions(
    string TrainerType,
    Dictionary<string, object> TrainerParameters,
    bool UseCrossValidation,
    int CrossValidationFolds,
    bool UseFeatureSelection,
    bool UseFeatureNormalization,
    int MaxIterations,
    float LearningRate,
    float L1Regularization,
    float L2Regularization,
    string Description,
    Dictionary<string, string> Tags
);

public record TrainingResponse(
    bool Success,
    string ModelVersion,
    string ModelPath,
    TrainingMetrics Metrics,
    ModelEvaluation Evaluation,
    TimeSpan TrainingTime,
    DateTime TrainedAt,
    List<string> Errors,
    List<string> Warnings
);

public record TrainingMetrics(
    float Accuracy,
    float MicroF1Score,
    float MacroF1Score,
    float WeightedF1Score,
    float LogLoss,
    float PerClassLogLoss,
    Dictionary<string, float> PerClassMetrics,
    ConfusionMatrix ConfusionMatrix,
    int TotalSamples,
    int CorrectPredictions,
    int IncorrectPredictions
);

public record ModelEvaluation(
    float Accuracy,
    float MicroF1Score,
    float MacroF1Score,
    float WeightedF1Score,
    Dictionary<string, float> PerClassF1Score,
    Dictionary<string, float> PerClassPrecision,
    Dictionary<string, float> PerClassRecall,
    ConfusionMatrix ConfusionMatrix,
    List<ModelError> Errors,
    DateTime EvaluatedAt
);

public record ConfusionMatrix(
    Dictionary<string, Dictionary<string, int>> Matrix,
    List<string> Classes,
    int TotalSamples,
    int CorrectPredictions,
    int IncorrectPredictions
);

public record ModelError(
    string ActualLabel,
    string PredictedLabel,
    float Confidence,
    string Text,
    int LineIndex,
    string DocumentId
);

public record ModelInfo(
    string Version,
    string Name,
    string Description,
    string TrainerType,
    Dictionary<string, object> Parameters,
    ModelEvaluation Evaluation,
    DateTime CreatedAt,
    DateTime LastUsed,
    bool IsActive,
    bool IsLoaded,
    long FileSize,
    string FilePath,
    Dictionary<string, string> Tags
);

public record ModelPerformance(
    string ModelVersion,
    int TotalPredictions,
    int CorrectPredictions,
    float Accuracy,
    float AverageConfidence,
    float AveragePredictionTime,
    Dictionary<string, int> PredictionsByClass,
    Dictionary<string, float> ConfidenceByClass,
    DateTime LastUsed,
    DateTime PerformanceCalculatedAt,
    List<PerformanceMetric> Metrics
);

public record PerformanceMetric(
    string Name,
    float Value,
    string Unit,
    DateTime CalculatedAt
);
```

## 4. File Storage DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/FileStorageDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record FileStorageRequest(
    string SourcePath,
    Guid InvoiceId,
    string FileName,
    long FileSize,
    string FileHash
);

public record FileStorageResponse(
    bool Success,
    string StoredPath,
    string RelativePath,
    string FileName,
    long FileSize,
    string FileHash,
    DateTime StoredAt,
    string Message
);

public record FileInfo(
    string Name,
    string FullPath,
    long Size,
    DateTime CreationTime,
    DateTime LastWriteTime,
    bool Exists
);

public record StorageStatistics(
    long TotalSize,
    int FileCount,
    int DirectoryCount,
    DateTime LastUpdated,
    Dictionary<string, long> SizeByYear,
    Dictionary<string, int> FileCountByYear
);

public record FileSearchRequest(
    string? FileName,
    string? Extension,
    DateTime? StartDate,
    DateTime? EndDate,
    long? MinSize,
    long? MaxSize,
    string? Directory
);

public record FileSearchResponse(
    List<FileInfo> Files,
    int TotalCount,
    long TotalSize,
    Dictionary<string, int> CountByExtension,
    Dictionary<string, long> SizeByExtension
);

public record FileCleanupRequest(
    int RetentionDays,
    bool DeleteEmptyDirectories,
    bool DryRun
);

public record FileCleanupResponse(
    bool Success,
    int FilesDeleted,
    int DirectoriesDeleted,
    long SpaceFreed,
    List<string> DeletedFiles,
    List<string> Errors
);
```

## 5. Validation DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/ValidationDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

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
```

## 6. Search and Filter DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/SearchDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record SearchRequest(
    string Query,
    List<string> Fields,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = true,
    Dictionary<string, object> Filters = null
);

public record SearchResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    Dictionary<string, object> Facets,
    TimeSpan SearchTime
);

public record FilterRequest(
    Dictionary<string, object> Filters,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20
);

public record FilterResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    Dictionary<string, List<FilterOption>> AvailableFilters
);

public record FilterOption(
    string Value,
    string Label,
    int Count,
    bool IsSelected
);

public record FacetRequest(
    string Field,
    int MaxValues = 10,
    bool IncludeCounts = true
);

public record FacetResponse(
    string Field,
    List<FacetValue> Values,
    int TotalValues
);

public record FacetValue(
    string Value,
    string Label,
    int Count,
    bool IsSelected
);
```

## 7. Common DTOs

**Datei:** `src/InvoiceReader.Application/DTOs/CommonDto.cs`

```csharp
namespace InvoiceReader.Application.DTOs;

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string Message,
    List<string> Errors,
    DateTime Timestamp
);

public record ApiResponse(
    bool Success,
    string Message,
    List<string> Errors,
    DateTime Timestamp
);

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

public record ErrorResponse(
    string Code,
    string Message,
    string? Details,
    string? StackTrace,
    DateTime Timestamp
);

public record SuccessResponse(
    string Message,
    object? Data,
    DateTime Timestamp
);

public record StatusResponse(
    string Status,
    string Message,
    DateTime Timestamp,
    Dictionary<string, object> Details
);

public record HealthCheckResponse(
    string Status,
    DateTime Timestamp,
    Dictionary<string, HealthCheck> Checks
);

public record HealthCheck(
    string Name,
    string Status,
    string Message,
    TimeSpan Duration,
    Dictionary<string, object> Details
);

public record ConfigurationResponse(
    Dictionary<string, object> Settings,
    DateTime LastUpdated,
    string Version
);

public record LogEntry(
    DateTime Timestamp,
    string Level,
    string Message,
    string? Exception,
    Dictionary<string, object> Properties
);

public record AuditEntry(
    DateTime Timestamp,
    string User,
    string Action,
    string Entity,
    string EntityId,
    Dictionary<string, object> Changes
);
```

## 8. DTO Extensions

**Datei:** `src/InvoiceReader.Application/Extensions/DtoExtensions.cs`

```csharp
using InvoiceReader.Application.DTOs;
using InvoiceReader.Domain.Entities;

namespace InvoiceReader.Application.Extensions;

public static class DtoExtensions
{
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.IssuerName,
            invoice.IssuerStreet,
            invoice.IssuerPostalCode,
            invoice.IssuerCity,
            invoice.IssuerCountry,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.SourceFilePath,
            invoice.ImportedAt,
            invoice.ExtractionConfidence,
            invoice.ModelVersion
        );
    }

    public static Invoice ToEntity(this InvoiceCreateDto dto)
    {
        return Invoice.Create(
            dto.InvoiceNumber,
            dto.InvoiceDate,
            dto.IssuerName,
            dto.NetTotal,
            dto.VatTotal,
            dto.GrossTotal,
            dto.SourceFilePath,
            dto.ExtractionConfidence,
            dto.ModelVersion
        );
    }

    public static List<InvoiceDto> ToDtoList(this List<Invoice> invoices)
    {
        return invoices.Select(i => i.ToDto()).ToList();
    }

    public static ApiResponse<T> ToApiResponse<T>(this T data, string message = "Success")
    {
        return new ApiResponse<T>(
            true,
            data,
            message,
            new List<string>(),
            DateTime.UtcNow
        );
    }

    public static ApiResponse<T> ToErrorResponse<T>(this string error, List<string> errors = null)
    {
        return new ApiResponse<T>(
            false,
            default(T),
            error,
            errors ?? new List<string> { error },
            DateTime.UtcNow
        );
    }

    public static PagedResponse<T> ToPagedResponse<T>(this List<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<T>(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            page < totalPages,
            page > 1
        );
    }
}
```

## Wichtige Hinweise

- Vollständige DTO-Struktur für alle Application Layer-Operationen
- Record-basierte DTOs für Immutability
- Separate DTOs für Create, Update, Search-Operationen
- Validation DTOs für Input-Validierung
- Search und Filter DTOs für komplexe Abfragen
- Common DTOs für API-Responses
- Extension Methods für Entity-DTO-Konvertierung
- Error Handling DTOs für strukturierte Fehlerbehandlung
- Health Check DTOs für System-Monitoring
- Audit DTOs für Compliance und Tracking
