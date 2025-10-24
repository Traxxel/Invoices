using Invoice.Application.DTOs;
using Invoice.Application.Interfaces;
using Invoice.Application.Extensions;
using Invoice.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Invoice.Application.UseCases;

public class SaveInvoiceUseCase : ISaveInvoiceUseCase
{
    private readonly ILogger<SaveInvoiceUseCase> _logger;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IFileStorageService _fileStorageService;

    public SaveInvoiceUseCase(
        ILogger<SaveInvoiceUseCase> logger,
        IInvoiceRepository invoiceRepository,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _invoiceRepository = invoiceRepository;
        _fileStorageService = fileStorageService;
    }

    public Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice)
    {
        var options = new SaveInvoiceOptions(
            CheckForDuplicates: true,
            RequireValidation: true,
            ApplyBusinessRules: true,
            CreateBackup: false,
            StoreFile: false,
            LogOperation: true,
            DuplicateStrategy: DuplicateHandlingStrategy.CreateNew,
            ValidationLevel: ValidationLevel.Standard,
            BusinessRulesLevel: BusinessRulesLevel.Standard,
            FileStorageStrategy: FileStorageStrategy.Local,
            CustomSettings: new Dictionary<string, object>()
        );

        return ExecuteAsync(invoice, options);
    }

    public async Task<SaveInvoiceResult> ExecuteAsync(InvoiceDto invoice, SaveInvoiceOptions options)
    {
        var request = new SaveInvoiceRequest(invoice, options);
        return await ExecuteAsync(request);
    }

    public async Task<SaveInvoiceResult> ExecuteAsync(SaveInvoiceRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<SaveWarning>();
        var errors = new List<SaveError>();

        try
        {
            _logger.LogInformation("Saving invoice {InvoiceNumber}", request.Invoice.InvoiceNumber);

            // Validate
            if (request.Options.RequireValidation)
            {
                var validationResult = await ValidateInvoiceAsync(request.Invoice);
                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors.Select(e => 
                        new SaveError(e.Code, e.Message, e.Field, e.Value, null)));
                    
                    return new SaveInvoiceResult(
                        Success: false,
                        Message: "Validation failed",
                        Invoice: null,
                        Warnings: warnings,
                        Errors: errors,
                        Statistics: new SaveStatistics(0, 0, 1, 0, 0, 1, 0, TimeSpan.Zero, 0, new(), new()),
                        SavedAt: DateTime.UtcNow,
                        SaveTime: DateTime.UtcNow - startTime
                    );
                }
            }

            // Check duplicates
            if (request.Options.CheckForDuplicates)
            {
                var duplicateCheck = await CheckForDuplicatesAsync(request.Invoice);
                if (duplicateCheck.HasDuplicates)
                {
                    warnings.Add(new SaveWarning(
                        "DUPLICATE_FOUND",
                        $"{duplicateCheck.Duplicates.Count} duplicates found",
                        "",
                        null,
                        "Review duplicates before saving"
                    ));

                    if (request.Options.DuplicateStrategy == DuplicateHandlingStrategy.Skip)
                    {
                        return new SaveInvoiceResult(
                            Success: false,
                            Message: "Duplicate found, skipping",
                            Invoice: null,
                            Warnings: warnings,
                            Errors: errors,
                            Statistics: new SaveStatistics(0, 0, 1, duplicateCheck.Duplicates.Count, 0, 0, 0, TimeSpan.Zero, 0, new(), new()),
                            SavedAt: DateTime.UtcNow,
                            SaveTime: DateTime.UtcNow - startTime
                        );
                    }
                }
            }

            // Save to database
            var dbResult = await SaveInvoiceToDatabaseAsync(request.Invoice);
            if (!dbResult.Success)
            {
                var saveErrors = dbResult.Errors.Select(e => new SaveError(e.Code, e.Message, e.Field, e.Value, e.Exception)).ToList();
                errors.AddRange(saveErrors);
                return new SaveInvoiceResult(
                    Success: false,
                    Message: "Database save failed",
                    Invoice: null,
                    Warnings: warnings,
                    Errors: errors,
                    Statistics: new SaveStatistics(1, 0, 1, 0, 0, 0, 0, TimeSpan.Zero, 0, new(), new()),
                    SavedAt: DateTime.UtcNow,
                    SaveTime: DateTime.UtcNow - startTime
                );
            }

            var saveTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Invoice {InvoiceNumber} saved successfully", request.Invoice.InvoiceNumber);

            return new SaveInvoiceResult(
                Success: true,
                Message: "Invoice saved successfully",
                Invoice: dbResult.Invoice,
                Warnings: warnings,
                Errors: errors,
                Statistics: new SaveStatistics(1, 1, 0, 0, 1, 1, 0, saveTime, request.Invoice.ExtractionConfidence, new(), new()),
                SavedAt: DateTime.UtcNow,
                SaveTime: saveTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving invoice {InvoiceNumber}", request.Invoice.InvoiceNumber);
            errors.Add(new SaveError("SAVE_ERROR", ex.Message, "", null, ex));

            return new SaveInvoiceResult(
                Success: false,
                Message: $"Save error: {ex.Message}",
                Invoice: null,
                Warnings: warnings,
                Errors: errors,
                Statistics: new SaveStatistics(1, 0, 1, 0, 0, 0, 0, TimeSpan.Zero, 0, new(), new()),
                SavedAt: DateTime.UtcNow,
                SaveTime: DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceDto invoice)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            errors.Add(new ValidationError("InvoiceNumber", "INVOICE_NUMBER_REQUIRED", "Invoice number is required", invoice.InvoiceNumber, "Please provide a valid invoice number"));
        }

        if (string.IsNullOrWhiteSpace(invoice.IssuerName))
        {
            errors.Add(new ValidationError("IssuerName", "ISSUER_NAME_REQUIRED", "Issuer name is required", invoice.IssuerName, "Please provide the issuer name"));
        }

        if (invoice.GrossTotal <= 0)
        {
            warnings.Add(new ValidationWarning("GrossTotal", "GROSS_TOTAL_ZERO", "Gross total is zero or negative", invoice.GrossTotal, "Verify amount"));
        }

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors,
            Warnings: warnings,
            Data: new Dictionary<string, object>()
        );
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
                .Where(i => i.InvoiceNumber == invoice.InvoiceNumber && i.Id != invoice.Id)
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

    public Task<FileOperationResult> StoreInvoiceFileAsync(InvoiceDto invoice, string sourceFilePath)
    {
        // TODO: Implement file storage
        return Task.FromResult(new FileOperationResult(
            Success: true,
            Message: "File storage not implemented",
            FilePath: null,
            FileSize: null,
            FileHash: null,
            Warnings: new List<FileOperationWarning>(),
            Errors: new List<FileOperationError>()
        ));
    }

    public async Task<DatabaseOperationResult> SaveInvoiceToDatabaseAsync(InvoiceDto invoice)
    {
        try
        {
            Domain.Entities.Invoice entity;

            if (invoice.Id == Guid.Empty || invoice.Id == default)
            {
                // New invoice
                entity = invoice.ToEntity();
                await _invoiceRepository.AddAsync(entity);
            }
            else
            {
                // Update existing
                entity = await _invoiceRepository.GetByIdAsync(invoice.Id);
                if (entity == null)
                {
                    return new DatabaseOperationResult(
                        Success: false,
                        Message: "Invoice not found",
                        Invoice: null,
                        Warnings: new List<DatabaseOperationWarning>(),
                        Errors: new List<DatabaseOperationError> 
                        { 
                            new DatabaseOperationError("INVOICE_NOT_FOUND", "Invoice not found", "Id", invoice.Id, null) 
                        }
                    );
                }

                // Update entity properties
                entity.InvoiceNumber = invoice.InvoiceNumber;
                entity.IssuerName = invoice.IssuerName;
                entity.IssuerStreet = invoice.IssuerStreet;
                entity.IssuerPostalCode = invoice.IssuerPostalCode;
                entity.IssuerCity = invoice.IssuerCity;
                entity.IssuerCountry = invoice.IssuerCountry;
                entity.InvoiceDate = invoice.InvoiceDate;
                entity.NetTotal = invoice.NetTotal;
                entity.VatTotal = invoice.VatTotal;
                entity.GrossTotal = invoice.GrossTotal;
                entity.ExtractionConfidence = invoice.ExtractionConfidence;

                await _invoiceRepository.UpdateAsync(entity);
            }

            return new DatabaseOperationResult(
                Success: true,
                Message: "Invoice saved successfully",
                Invoice: entity.ToDto(),
                Warnings: new List<DatabaseOperationWarning>(),
                Errors: new List<DatabaseOperationError>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving invoice to database");
            return new DatabaseOperationResult(
                Success: false,
                Message: $"Database error: {ex.Message}",
                Invoice: null,
                Warnings: new List<DatabaseOperationWarning>(),
                Errors: new List<DatabaseOperationError> 
                { 
                    new DatabaseOperationError("DATABASE_ERROR", ex.Message, "", null, ex) 
                }
            );
        }
    }
}


