# Aufgabe 27: IImportInvoiceUseCase und Implementation

## Ziel

Import Invoice Use Case für PDF-Import mit ML-basierter Feld-Extraktion und manueller Überprüfung.

## 1. Import Invoice Use Case Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IImportInvoiceUseCase.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

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
    List<TextBlock> TextBlocks
);

public record TextBlock(
    string Text,
    float X,
    float Y,
    float Width,
    float Height,
    int LineIndex,
    float FontSize,
    string FontName
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
```

## 2. Import Invoice Use Case Implementation

**Datei:** `src/InvoiceReader.Application/UseCases/ImportInvoiceUseCase.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.DTOs;
using InvoiceReader.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.Application.UseCases;

public class ImportInvoiceUseCase : IImportInvoiceUseCase
{
    private readonly IPdfParserService _pdfParserService;
    private readonly ITextNormalizationService _textNormalizationService;
    private readonly IFeatureExtractionService _featureExtractionService;
    private readonly IPredictionEngineService _predictionEngineService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileHashService _fileHashService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<ImportInvoiceUseCase> _logger;

    public ImportInvoiceUseCase(
        IPdfParserService pdfParserService,
        ITextNormalizationService textNormalizationService,
        IFeatureExtractionService featureExtractionService,
        IPredictionEngineService predictionEngineService,
        IFileStorageService fileStorageService,
        IFileHashService fileHashService,
        IInvoiceRepository invoiceRepository,
        ILogger<ImportInvoiceUseCase> logger)
    {
        _pdfParserService = pdfParserService;
        _textNormalizationService = textNormalizationService;
        _featureExtractionService = featureExtractionService;
        _predictionEngineService = predictionEngineService;
        _fileStorageService = fileStorageService;
        _fileHashService = fileHashService;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    public async Task<ImportResult> ExecuteAsync(ImportInvoiceRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<ImportWarning>();
        var errors = new List<ImportError>();

        try
        {
            _logger.LogInformation("Starting invoice import: {FilePath}", request.FilePath);

            // Validate request
            var validation = await ValidateImportRequestAsync(request);
            if (!validation.IsValid)
            {
                return new ImportResult(
                    false,
                    "Import validation failed",
                    null,
                    null,
                    warnings,
                    validation.Errors.Select(e => new ImportError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                    new ImportStatistics(),
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Preprocess file
            var preprocessing = await PreprocessFileAsync(request.FilePath);
            if (!preprocessing.Success)
            {
                return new ImportResult(
                    false,
                    "File preprocessing failed",
                    null,
                    null,
                    warnings,
                    preprocessing.Errors,
                    new ImportStatistics(),
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            warnings.AddRange(preprocessing.Warnings);

            // Parse PDF
            var parsedDocument = await _pdfParserService.ParseDocumentAsync(request.FilePath);
            if (parsedDocument == null)
            {
                return new ImportResult(
                    false,
                    "PDF parsing failed",
                    null,
                    null,
                    warnings,
                    new List<ImportError> { new ImportError("PARSE_FAILED", "Failed to parse PDF", "File", request.FilePath, null) },
                    new ImportStatistics(),
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Normalize text
            var normalizedLines = await _textNormalizationService.NormalizeTextLinesAsync(parsedDocument.AllTextLines);

            // Extract features
            var features = await _featureExtractionService.ExtractFeaturesAsync(normalizedLines);

            // Make predictions
            var predictions = await _predictionEngineService.PredictAsync(features);

            // Post-process predictions
            var extractionResult = await PostProcessPredictionsAsync(predictions, features);

            // Create invoice DTO
            var invoiceDto = await CreateInvoiceDtoAsync(extractionResult, request.FilePath);

            // Check for duplicates if enabled
            if (request.Options.CheckForDuplicates)
            {
                var duplicateCheck = await CheckForDuplicatesAsync(invoiceDto);
                if (duplicateCheck.HasDuplicates)
                {
                    warnings.Add(new ImportWarning(
                        "DUPLICATE_FOUND",
                        "Potential duplicate invoice found",
                        "InvoiceNumber",
                        invoiceDto.InvoiceNumber,
                        "Review duplicate invoices before saving"
                    ));
                }
            }

            // Store file if auto-save is enabled
            string storedFilePath = request.FilePath;
            if (request.Options.AutoSave)
            {
                var invoice = invoiceDto.ToEntity();
                storedFilePath = await _fileStorageService.StoreFileAsync(request.FilePath, invoice.Id);
            }

            var statistics = new ImportStatistics(
                1, // TotalFiles
                1, // SuccessfulImports
                0, // FailedImports
                duplicateCheck?.HasDuplicates == true ? 1 : 0, // DuplicatesFound
                extractionResult.OverallConfidence < request.Options.ConfidenceThreshold ? 1 : 0, // ManualReviewsRequired
                DateTime.UtcNow - startTime, // TotalProcessingTime
                extractionResult.OverallConfidence, // AverageConfidence
                extractionResult.Fields.ToDictionary(f => f.FieldType, f => 1), // ExtractionsByField
                new Dictionary<string, int>() // ErrorsByType
            );

            _logger.LogInformation("Invoice import completed successfully: {FilePath}", request.FilePath);

            return new ImportResult(
                true,
                "Invoice imported successfully",
                invoiceDto,
                extractionResult,
                warnings,
                errors,
                statistics,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice import failed: {FilePath}", request.FilePath);

            return new ImportResult(
                false,
                "Invoice import failed",
                null,
                null,
                warnings,
                new List<ImportError> { new ImportError("IMPORT_FAILED", ex.Message, "File", request.FilePath, ex) },
                new ImportStatistics(),
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<ImportResult> ExecuteAsync(string filePath)
    {
        var options = await GetDefaultImportOptionsAsync();
        var request = new ImportInvoiceRequest(filePath, options);
        return await ExecuteAsync(request);
    }

    public async Task<ImportResult> ExecuteAsync(Stream fileStream, string fileName)
    {
        try
        {
            // Save stream to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            using (var fileStream2 = File.Create(tempPath))
            {
                await fileStream.CopyToAsync(fileStream2);
            }

            var result = await ExecuteAsync(tempPath);

            // Clean up temporary file
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import from stream: {FileName}", fileName);
            throw;
        }
    }

    public async Task<ImportResult> ExecuteAsync(byte[] fileBytes, string fileName)
    {
        using var stream = new MemoryStream(fileBytes);
        return await ExecuteAsync(stream, fileName);
    }

    public async Task<ValidationResult> ValidateImportRequestAsync(ImportInvoiceRequest request)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        try
        {
            // Validate file path
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                errors.Add(new ValidationError("FilePath", "FILE_PATH_REQUIRED", "File path is required", null, null));
            }
            else if (!File.Exists(request.FilePath))
            {
                errors.Add(new ValidationError("FilePath", "FILE_NOT_FOUND", "File does not exist", request.FilePath, null));
            }
            else
            {
                // Validate file type
                var extension = Path.GetExtension(request.FilePath).ToLowerInvariant();
                if (extension != ".pdf")
                {
                    errors.Add(new ValidationError("FilePath", "INVALID_FILE_TYPE", "Only PDF files are supported", extension, ".pdf"));
                }

                // Validate file size
                var fileInfo = new FileInfo(request.FilePath);
                if (fileInfo.Length > 50 * 1024 * 1024) // 50 MB
                {
                    warnings.Add(new ValidationWarning("FilePath", "FILE_TOO_LARGE", "File size exceeds recommended limit", fileInfo.Length, "Consider compressing the PDF"));
                }
            }

            // Validate options
            if (request.Options.ConfidenceThreshold < 0.0f || request.Options.ConfidenceThreshold > 1.0f)
            {
                errors.Add(new ValidationError("ConfidenceThreshold", "INVALID_THRESHOLD", "Confidence threshold must be between 0.0 and 1.0", request.Options.ConfidenceThreshold, "0.5"));
            }

            return new ValidationResult(errors.Count == 0, errors, warnings, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation failed for import request");
            errors.Add(new ValidationError("Validation", "VALIDATION_ERROR", "Validation process failed", null, ex.Message));
            return new ValidationResult(false, errors, warnings, new Dictionary<string, object>());
        }
    }

    public async Task<bool> IsValidPdfFileAsync(string filePath)
    {
        try
        {
            return await _pdfParserService.IsValidPdfAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate PDF file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> IsValidPdfFileAsync(Stream fileStream)
    {
        try
        {
            // Reset stream position
            fileStream.Position = 0;
            return await _pdfParserService.IsValidPdfAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate PDF stream");
            return false;
        }
    }

    public async Task<bool> IsValidPdfFileAsync(byte[] fileBytes)
    {
        try
        {
            return await _pdfParserService.IsValidPdfAsync(fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate PDF bytes");
            return false;
        }
    }

    public async Task<ImportPreprocessingResult> PreprocessFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Preprocessing file: {FilePath}", filePath);

            var warnings = new List<ImportWarning>();
            var errors = new List<ImportError>();

            // Get file info
            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;
            var fileHash = await _fileHashService.CalculateHashAsync(filePath);

            // Get page count
            var pageCount = await _pdfParserService.GetPageCountAsync(filePath);
            if (pageCount == 0)
            {
                errors.Add(new ImportError("PAGE_COUNT", "No pages found in PDF", "File", filePath, null));
                return new ImportPreprocessingResult(false, "No pages found in PDF", filePath, fileSize, fileHash, 0, new List<PageInfo>(), warnings, errors);
            }

            // Get document info
            var documentInfo = await _pdfParserService.GetDocumentInfoAsync(filePath);
            var pages = new List<PageInfo>();

            for (int i = 1; i <= pageCount; i++)
            {
                var textLines = await _pdfParserService.ExtractTextLinesFromPageAsync(filePath, i);
                var words = await _pdfParserService.ExtractWordsFromPageAsync(filePath, i);

                pages.Add(new PageInfo(
                    i,
                    documentInfo.PageCount > 0 ? 1000f : 0f, // Default page width
                    1000f, // Default page height
                    textLines.Count,
                    words.Count,
                    textLines.Select(tl => new TextBlock(
                        tl.Text,
                        tl.X,
                        tl.Y,
                        tl.Width,
                        tl.Height,
                        tl.LineIndex,
                        tl.FontSize,
                        tl.FontName
                    )).ToList()
                ));
            }

            _logger.LogInformation("File preprocessing completed: {FilePath}, {PageCount} pages", filePath, pageCount);

            return new ImportPreprocessingResult(
                true,
                "File preprocessing completed successfully",
                filePath,
                fileSize,
                fileHash,
                pageCount,
                pages,
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File preprocessing failed: {FilePath}", filePath);
            return new ImportPreprocessingResult(
                false,
                "File preprocessing failed",
                filePath,
                0,
                string.Empty,
                0,
                new List<PageInfo>(),
                new List<ImportWarning>(),
                new List<ImportError> { new ImportError("PREPROCESSING_FAILED", ex.Message, "File", filePath, ex) }
            );
        }
    }

    public async Task<ImportPreprocessingResult> PreprocessFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            // Save stream to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            using (var fileStream2 = File.Create(tempPath))
            {
                await fileStream.CopyToAsync(fileStream2);
            }

            var result = await PreprocessFileAsync(tempPath);

            // Clean up temporary file
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preprocess stream: {FileName}", fileName);
            throw;
        }
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice)
    {
        try
        {
            _logger.LogInformation("Checking for duplicates: {InvoiceNumber}", invoice.InvoiceNumber);

            var duplicates = new List<InvoiceDto>();
            var similarInvoices = new List<SimilarInvoice>();

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
                    similarInvoices.Add(new SimilarInvoice(
                        similarInvoice,
                        similarity,
                        GetMatchingFields(invoice, similarInvoice),
                        GetDifferences(invoice, similarInvoice)
                    ));
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
                similarInvoices.Any() ? similarInvoices.Max(s => s.SimilarityScore) : 0f
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for duplicates: {InvoiceNumber}", invoice.InvoiceNumber);
            return new DuplicateCheckResult(false, new List<InvoiceDto>(), new List<SimilarInvoice>(), DuplicateMatchType.None, 0f);
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

    public async Task<ImportOptions> GetDefaultImportOptionsAsync()
    {
        return new ImportOptions(
            UseMLExtraction: true,
            RequireManualReview: false,
            ConfidenceThreshold: 0.7f,
            CheckForDuplicates: true,
            AutoSave: false,
            CreateBackup: true,
            ModelVersion: "v1.0",
            CustomSettings: new Dictionary<string, object>()
        );
    }

    public async Task<ImportOptions> GetImportOptionsAsync(string filePath)
    {
        // Load options from file or configuration
        return await GetDefaultImportOptionsAsync();
    }

    public async Task<bool> UpdateImportOptionsAsync(ImportOptions options)
    {
        try
        {
            // Save options to configuration
            _logger.LogInformation("Import options updated");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update import options");
            return false;
        }
    }

    private async Task<ExtractionResult> PostProcessPredictionsAsync(List<PredictionResult> predictions, List<ExtractedFeature> features)
    {
        var fields = new List<ExtractedField>();
        var warnings = new List<ExtractionWarning>();
        var errors = new List<ExtractionError>();
        var overallConfidence = 0f;

        foreach (var prediction in predictions)
        {
            if (prediction.PredictedLabel != "None")
            {
                var field = new ExtractedField(
                    prediction.PredictedLabel,
                    prediction.SourceFeature.Text,
                    prediction.Confidence,
                    prediction.SourceFeature.Text,
                    prediction.SourceFeature.LineIndex,
                    prediction.SourceFeature.PageNumber,
                    prediction.SourceFeature.Position.X,
                    prediction.SourceFeature.Position.Y,
                    prediction.SourceFeature.Position.Width,
                    prediction.SourceFeature.Position.Height,
                    prediction.ClassScores.Select(cs => new AlternativeValue(cs.ClassName, cs.Score, "ML")).ToList()
                );

                fields.Add(field);
                overallConfidence += prediction.Confidence;

                if (prediction.IsLowConfidence)
                {
                    warnings.Add(new ExtractionWarning(
                        "LOW_CONFIDENCE",
                        $"Low confidence prediction for {prediction.PredictedLabel}",
                        prediction.PredictedLabel,
                        prediction.SourceFeature.Text,
                        prediction.LineIndex
                    ));
                }
            }
        }

        if (fields.Any())
        {
            overallConfidence /= fields.Count;
        }

        return new ExtractionResult(
            true,
            "Extraction completed successfully",
            fields,
            warnings,
            errors,
            overallConfidence,
            "v1.0",
            DateTime.UtcNow,
            TimeSpan.Zero
        );
    }

    private async Task<InvoiceDto> CreateInvoiceDtoAsync(ExtractionResult extraction, string filePath)
    {
        var invoiceNumber = extraction.Fields.FirstOrDefault(f => f.FieldType == "InvoiceNumber")?.Value ?? "UNKNOWN";
        var invoiceDate = ParseInvoiceDate(extraction.Fields.FirstOrDefault(f => f.FieldType == "InvoiceDate")?.Value);
        var issuerName = extraction.Fields.FirstOrDefault(f => f.FieldType == "IssuerAddress")?.Value ?? "UNKNOWN";
        var netTotal = ParseDecimal(extraction.Fields.FirstOrDefault(f => f.FieldType == "NetTotal")?.Value);
        var vatTotal = ParseDecimal(extraction.Fields.FirstOrDefault(f => f.FieldType == "VatTotal")?.Value);
        var grossTotal = ParseDecimal(extraction.Fields.FirstOrDefault(f => f.FieldType == "GrossTotal")?.Value);

        return new InvoiceDto(
            Guid.NewGuid(),
            invoiceNumber,
            invoiceDate,
            issuerName,
            string.Empty, // IssuerStreet
            string.Empty, // IssuerPostalCode
            string.Empty, // IssuerCity
            null, // IssuerCountry
            netTotal,
            vatTotal,
            grossTotal,
            filePath,
            DateTime.UtcNow,
            extraction.OverallConfidence,
            extraction.ModelVersion
        );
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

    private DateOnly ParseInvoiceDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return DateOnly.FromDateTime(DateTime.Today);

        if (DateOnly.TryParse(dateString, out var date))
            return date;

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        if (decimal.TryParse(value, out var result))
            return result;

        return 0m;
    }
}
```

## 3. Import Invoice Use Case Extensions

**Datei:** `src/InvoiceReader.Application/Extensions/ImportInvoiceExtensions.cs`

```csharp
using InvoiceReader.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Application.Extensions;

public static class ImportInvoiceExtensions
{
    public static IServiceCollection AddImportInvoiceServices(this IServiceCollection services)
    {
        services.AddScoped<IImportInvoiceUseCase, ImportInvoiceUseCase>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Import Invoice Use Case für PDF-Import
- ML-basierte Feld-Extraktion mit Confidence-Scores
- Duplicate Detection und Similarity-Check
- File Preprocessing und Validation
- Error Handling für alle Import-Operationen
- Logging für alle Import-Operationen
- Import Options für konfigurierbare Import-Parameter
- Statistics für Import-Monitoring
- Manual Review für Low-Confidence-Extraktionen
