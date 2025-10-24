using Invoice.Application.DTOs;
using Invoice.Application.Extensions;
using Invoice.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Invoice.Application.UseCases;

public class ImportInvoiceUseCase : IImportInvoiceUseCase
{
    private readonly ILogger<ImportInvoiceUseCase> _logger;
    private readonly IPdfParserService _pdfParser;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;
    private readonly ISaveInvoiceUseCase _saveInvoiceUseCase;
    private readonly IInvoiceRepository _invoiceRepository;

    public ImportInvoiceUseCase(
        ILogger<ImportInvoiceUseCase> logger,
        IPdfParserService pdfParser,
        IExtractFieldsUseCase extractFieldsUseCase,
        ISaveInvoiceUseCase saveInvoiceUseCase,
        IInvoiceRepository invoiceRepository)
    {
        _logger = logger;
        _pdfParser = pdfParser;
        _extractFieldsUseCase = extractFieldsUseCase;
        _saveInvoiceUseCase = saveInvoiceUseCase;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<ImportResult> ExecuteAsync(string filePath)
    {
        var options = await GetDefaultImportOptionsAsync();
        var request = new ImportInvoiceRequest(filePath, options);
        return await ExecuteAsync(request);
    }

    public async Task<ImportResult> ExecuteAsync(ImportInvoiceRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<ImportWarning>();
        var errors = new List<ImportError>();

        try
        {
            _logger.LogInformation("Starting import from {FilePath}", request.FilePath);

            // Validate request
            var validationResult = await ValidateImportRequestAsync(request);
            if (!validationResult.IsValid)
            {
                return new ImportResult(
                    Success: false,
                    Message: "Validation failed",
                    Invoice: null,
                    Extraction: null,
                    Warnings: warnings,
                    Errors: validationResult.Errors.Select(e => new ImportError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                    Statistics: new ImportStatistics(0, 0, 1, 0, 0, TimeSpan.Zero, 0, new(), new()),
                    ImportedAt: DateTime.UtcNow,
                    ImportTime: DateTime.UtcNow - startTime
                );
            }

            // Extract fields
            var extractionResult = await _extractFieldsUseCase.ExecuteAsync(request.FilePath);
            if (!extractionResult.Success)
            {
                errors.Add(new ImportError("EXTRACTION_FAILED", extractionResult.Message, "", null, null));
                return new ImportResult(
                    Success: false,
                    Message: "Field extraction failed",
                    Invoice: null,
                    Extraction: extractionResult,
                    Warnings: warnings,
                    Errors: errors,
                    Statistics: new ImportStatistics(0, 0, 1, 0, 0, TimeSpan.Zero, 0, new(), new()),
                    ImportedAt: DateTime.UtcNow,
                    ImportTime: DateTime.UtcNow - startTime
                );
            }

            // Create invoice DTO from extracted fields
            var invoiceDto = CreateInvoiceDtoFromExtraction(extractionResult, request.FilePath);
            if (invoiceDto == null)
            {
                errors.Add(new ImportError("INVOICE_CREATION_FAILED", "Failed to create invoice from extracted fields", "", null, null));
                return new ImportResult(
                    Success: false,
                    Message: "Invoice creation failed",
                    Invoice: null,
                    Extraction: extractionResult,
                    Warnings: warnings,
                    Errors: errors,
                    Statistics: new ImportStatistics(0, 0, 1, 0, 0, TimeSpan.Zero, 0, new(), new()),
                    ImportedAt: DateTime.UtcNow,
                    ImportTime: DateTime.UtcNow - startTime
                );
            }
            
            var invoice = invoiceDto;

            // Check for duplicates if enabled
            if (request.Options.CheckForDuplicates)
            {
                var duplicateCheck = await CheckForDuplicatesAsync(invoice);
                if (duplicateCheck.HasDuplicates)
                {
                    warnings.Add(new ImportWarning("DUPLICATE_FOUND", $"{duplicateCheck.Duplicates.Count} duplicates found", "", null, "Review duplicates"));
                }
            }

            // Save if enabled
            if (request.Options.AutoSave)
            {
                var saveOptions = new SaveInvoiceOptions(
                    CheckForDuplicates: false, // Already checked
                    RequireValidation: true,
                    ApplyBusinessRules: true,
                    CreateBackup: request.Options.CreateBackup,
                    StoreFile: true,
                    LogOperation: true,
                    DuplicateStrategy: DuplicateHandlingStrategy.CreateNew,
                    ValidationLevel: ValidationLevel.Standard,
                    BusinessRulesLevel: BusinessRulesLevel.Standard,
                    FileStorageStrategy: FileStorageStrategy.Local,
                    CustomSettings: new Dictionary<string, object>()
                );

                var saveResult = await _saveInvoiceUseCase.ExecuteAsync(invoice, saveOptions);
                if (!saveResult.Success)
                {
                    var importErrors = saveResult.Errors.Select(e => new ImportError(e.Code, e.Message, e.Field, e.Value, e.Exception)).ToList();
                    errors.AddRange(importErrors);
                    return new ImportResult(
                        Success: false,
                        Message: "Save failed",
                        Invoice: invoice,
                        Extraction: extractionResult,
                        Warnings: warnings,
                        Errors: errors,
                        Statistics: new ImportStatistics(1, 0, 1, 0, 0, TimeSpan.Zero, extractionResult.OverallConfidence, new(), new()),
                        ImportedAt: DateTime.UtcNow,
                        ImportTime: DateTime.UtcNow - startTime
                    );
                }

                invoice = saveResult.Invoice ?? invoice;
            }

            var importTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Import completed successfully in {ImportTime}ms", importTime.TotalMilliseconds);

            return new ImportResult(
                Success: true,
                Message: "Import successful",
                Invoice: invoice,
                Extraction: extractionResult,
                Warnings: warnings,
                Errors: errors,
                Statistics: new ImportStatistics(1, 1, 0, 0, 0, importTime, extractionResult.OverallConfidence, new(), new()),
                ImportedAt: DateTime.UtcNow,
                ImportTime: importTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing invoice from {FilePath}", request.FilePath);
            errors.Add(new ImportError("IMPORT_ERROR", ex.Message, "", null, ex));

            return new ImportResult(
                Success: false,
                Message: $"Import error: {ex.Message}",
                Invoice: null,
                Extraction: null,
                Warnings: warnings,
                Errors: errors,
                Statistics: new ImportStatistics(0, 0, 1, 0, 0, TimeSpan.Zero, 0, new(), new()),
                ImportedAt: DateTime.UtcNow,
                ImportTime: DateTime.UtcNow - startTime
            );
        }
    }

    public Task<ImportResult> ExecuteAsync(Stream fileStream, string fileName)
    {
        throw new NotImplementedException("Stream import not yet implemented");
    }

    public Task<ImportResult> ExecuteAsync(byte[] fileBytes, string fileName)
    {
        throw new NotImplementedException("Byte array import not yet implemented");
    }

    public async Task<ValidationResult> ValidateImportRequestAsync(ImportInvoiceRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            errors.Add(new ValidationError("FilePath", "FILE_PATH_REQUIRED", "File path is required", request.FilePath, "Please provide a valid file path"));
        }
        else if (!File.Exists(request.FilePath))
        {
            errors.Add(new ValidationError("FilePath", "FILE_NOT_FOUND", "File not found", request.FilePath, "Check if the file exists"));
        }
        else if (!await IsValidPdfFileAsync(request.FilePath))
        {
            errors.Add(new ValidationError("FilePath", "INVALID_PDF", "File is not a valid PDF", request.FilePath, "Provide a valid PDF file"));
        }

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors,
            Warnings: new List<ValidationWarning>(),
            Data: new Dictionary<string, object>()
        );
    }

    public async Task<bool> IsValidPdfFileAsync(string filePath)
    {
        try
        {
            return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase) &&
                   new System.IO.FileInfo(filePath).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> IsValidPdfFileAsync(Stream fileStream)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsValidPdfFileAsync(byte[] fileBytes)
    {
        throw new NotImplementedException();
    }

    public Task<ImportPreprocessingResult> PreprocessFileAsync(string filePath)
    {
        throw new NotImplementedException();
    }

    public Task<ImportPreprocessingResult> PreprocessFileAsync(Stream fileStream, string fileName)
    {
        throw new NotImplementedException();
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(InvoiceDto invoice)
    {
        try
        {
            var similar = await FindSimilarInvoicesAsync(invoice);
            var similarInvoices = similar.Select(s => new SimilarInvoice(
                Invoice: s,
                SimilarityScore: 0.9f,
                MatchingFields: new List<string> { "InvoiceNumber", "IssuerName" },
                Differences: new List<string>()
            )).ToList();
            
            return new DuplicateCheckResult(
                HasDuplicates: similar.Count > 0,
                Duplicates: similar,
                SimilarInvoices: similarInvoices,
                MatchType: similar.Count > 0 ? DuplicateMatchType.Similar : DuplicateMatchType.None,
                SimilarityScore: similar.Count > 0 ? 0.9f : 0.0f
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicates");
            return new DuplicateCheckResult(
                HasDuplicates: false, 
                Duplicates: new List<InvoiceDto>(), 
                SimilarInvoices: new List<SimilarInvoice>(),
                MatchType: DuplicateMatchType.None,
                SimilarityScore: 0.0f
            );
        }
    }

    public async Task<List<InvoiceDto>> FindSimilarInvoicesAsync(InvoiceDto invoice)
    {
        try
        {
            var allInvoices = await _invoiceRepository.GetAllAsync();
            var similar = allInvoices
                .Where(i => i.InvoiceNumber == invoice.InvoiceNumber ||
                           i.IssuerName == invoice.IssuerName && i.InvoiceDate == invoice.InvoiceDate)
                .Select(i => i.ToDto())
                .ToList();

            return similar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar invoices");
            return new List<InvoiceDto>();
        }
    }

    public Task<ImportOptions> GetDefaultImportOptionsAsync()
    {
        return Task.FromResult(new ImportOptions(
            UseMLExtraction: true,
            RequireManualReview: false,
            ConfidenceThreshold: 0.7f,
            CheckForDuplicates: true,
            AutoSave: true,
            CreateBackup: true,
            ModelVersion: "1.0",
            CustomSettings: new Dictionary<string, object>()
        ));
    }

    public Task<ImportOptions> GetImportOptionsAsync(string filePath)
    {
        return GetDefaultImportOptionsAsync();
    }

    public Task<bool> UpdateImportOptionsAsync(ImportOptions options)
    {
        return Task.FromResult(true);
    }

    private InvoiceDto? CreateInvoiceDtoFromExtraction(ExtractionResult extraction, string sourceFilePath)
    {
        try
        {
            // Helper function to get field value
            string GetFieldValue(string fieldType)
            {
                return extraction.Fields.FirstOrDefault(f => f.FieldType.Equals(fieldType, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
            }

            decimal GetDecimalValue(string fieldType)
            {
                var value = GetFieldValue(fieldType);
                return decimal.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0m;
            }

            DateOnly GetDateValue(string fieldType)
            {
                var value = GetFieldValue(fieldType);
                return DateOnly.TryParse(value, out var result) ? result : DateOnly.FromDateTime(DateTime.UtcNow);
            }

            var invoiceNumber = GetFieldValue("InvoiceNumber");
            var invoiceDate = GetDateValue("InvoiceDate");
            var issuerName = GetFieldValue("IssuerName");
            var issuerStreet = GetFieldValue("IssuerStreet");
            var issuerPostalCode = GetFieldValue("IssuerPostalCode");
            var issuerCity = GetFieldValue("IssuerCity");
            var issuerCountry = GetFieldValue("IssuerCountry");
            var netTotal = GetDecimalValue("NetTotal");
            var vatTotal = GetDecimalValue("VatTotal");
            var grossTotal = GetDecimalValue("GrossTotal");

            // If critical fields are missing, return null
            if (string.IsNullOrWhiteSpace(invoiceNumber) || string.IsNullOrWhiteSpace(issuerName))
            {
                return null;
            }

            return new InvoiceDto(
                Id: Guid.NewGuid(),
                InvoiceNumber: invoiceNumber,
                InvoiceDate: invoiceDate,
                IssuerName: issuerName,
                IssuerStreet: issuerStreet,
                IssuerPostalCode: issuerPostalCode,
                IssuerCity: issuerCity,
                IssuerCountry: string.IsNullOrWhiteSpace(issuerCountry) ? null : issuerCountry,
                NetTotal: netTotal,
                VatTotal: vatTotal,
                GrossTotal: grossTotal,
                SourceFilePath: sourceFilePath,
                ImportedAt: DateTime.UtcNow,
                ExtractionConfidence: extraction.OverallConfidence,
                ModelVersion: extraction.ModelVersion
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating InvoiceDto from extraction");
            return null;
        }
    }
}


