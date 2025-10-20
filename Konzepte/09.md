# Aufgabe 09: Domain Validierungen und Business Rules

## Ziel

Domain-spezifische Validierungen und Business Rules für die Invoice-Entity definieren.

## 1. Invoice Validator

**Datei:** `src/InvoiceReader.Domain/Validators/InvoiceValidator.cs`

```csharp
using InvoiceReader.Domain.Entities;
using InvoiceReader.Domain.ValueObjects;

namespace InvoiceReader.Domain.Validators;

public class InvoiceValidator
{
    public ValidationResult Validate(Invoice invoice)
    {
        var result = new ValidationResult();

        ValidateInvoiceNumber(invoice, result);
        ValidateInvoiceDate(invoice, result);
        ValidateIssuerInfo(invoice, result);
        ValidateFinancials(invoice, result);
        ValidateFileInfo(invoice, result);
        ValidateExtractionInfo(invoice, result);

        return result;
    }

    private void ValidateInvoiceNumber(Invoice invoice, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            result.AddError("InvoiceNumber", "Invoice number is required");
            return;
        }

        if (invoice.InvoiceNumber.Length < 3)
        {
            result.AddError("InvoiceNumber", "Invoice number must be at least 3 characters long");
        }

        if (invoice.InvoiceNumber.Length > 100)
        {
            result.AddError("InvoiceNumber", "Invoice number cannot exceed 100 characters");
        }

        // Check for valid characters (alphanumeric, hyphens, dots, slashes)
        if (!System.Text.RegularExpressions.Regex.IsMatch(invoice.InvoiceNumber, @"^[A-Za-z0-9\-\/\.]+$"))
        {
            result.AddError("InvoiceNumber", "Invoice number contains invalid characters");
        }
    }

    private void ValidateInvoiceDate(Invoice invoice, ValidationResult result)
    {
        if (invoice.InvoiceDate == default)
        {
            result.AddError("InvoiceDate", "Invoice date is required");
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var sevenDaysFromNow = today.AddDays(7);

        if (invoice.InvoiceDate > sevenDaysFromNow)
        {
            result.AddError("InvoiceDate", "Invoice date cannot be more than 7 days in the future");
        }

        var tenYearsAgo = today.AddYears(-10);
        if (invoice.InvoiceDate < tenYearsAgo)
        {
            result.AddError("InvoiceDate", "Invoice date cannot be more than 10 years in the past");
        }
    }

    private void ValidateIssuerInfo(Invoice invoice, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(invoice.IssuerName))
        {
            result.AddError("IssuerName", "Issuer name is required");
        }
        else if (invoice.IssuerName.Length > 200)
        {
            result.AddError("IssuerName", "Issuer name cannot exceed 200 characters");
        }

        if (!string.IsNullOrWhiteSpace(invoice.IssuerStreet) && invoice.IssuerStreet.Length > 200)
        {
            result.AddError("IssuerStreet", "Issuer street cannot exceed 200 characters");
        }

        if (!string.IsNullOrWhiteSpace(invoice.IssuerPostalCode) && invoice.IssuerPostalCode.Length > 20)
        {
            result.AddError("IssuerPostalCode", "Issuer postal code cannot exceed 20 characters");
        }

        if (!string.IsNullOrWhiteSpace(invoice.IssuerCity) && invoice.IssuerCity.Length > 100)
        {
            result.AddError("IssuerCity", "Issuer city cannot exceed 100 characters");
        }

        if (!string.IsNullOrWhiteSpace(invoice.IssuerCountry) && invoice.IssuerCountry.Length > 100)
        {
            result.AddError("IssuerCountry", "Issuer country cannot exceed 100 characters");
        }
    }

    private void ValidateFinancials(Invoice invoice, ValidationResult result)
    {
        if (invoice.NetTotal < 0)
        {
            result.AddError("NetTotal", "Net total cannot be negative");
        }

        if (invoice.VatTotal < 0)
        {
            result.AddError("VatTotal", "VAT total cannot be negative");
        }

        if (invoice.GrossTotal < 0)
        {
            result.AddError("GrossTotal", "Gross total cannot be negative");
        }

        // Check if NetTotal + VatTotal = GrossTotal (with tolerance)
        var calculatedTotal = invoice.NetTotal + invoice.VatTotal;
        var difference = Math.Abs(calculatedTotal - invoice.GrossTotal);

        if (difference > 0.02m) // 2 cents tolerance
        {
            result.AddError("GrossTotal", $"Gross total ({invoice.GrossTotal:C}) does not match NetTotal + VatTotal ({calculatedTotal:C})");
        }

        // Check for reasonable amounts
        if (invoice.GrossTotal > 1000000m)
        {
            result.AddWarning("GrossTotal", "Gross total exceeds 1,000,000 - please verify");
        }

        // Check VAT rate if both NetTotal and VatTotal are provided
        if (invoice.NetTotal > 0 && invoice.VatTotal > 0)
        {
            var vatRate = (invoice.VatTotal / invoice.NetTotal) * 100;
            if (vatRate < 0 || vatRate > 50)
            {
                result.AddWarning("VatTotal", $"VAT rate of {vatRate:F1}% seems unusual");
            }
        }
    }

    private void ValidateFileInfo(Invoice invoice, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(invoice.SourceFilePath))
        {
            result.AddError("SourceFilePath", "Source file path is required");
            return;
        }

        if (!System.IO.Path.HasExtension(invoice.SourceFilePath))
        {
            result.AddError("SourceFilePath", "Source file path must have a file extension");
        }

        var allowedExtensions = new[] { ".pdf" };
        var extension = System.IO.Path.GetExtension(invoice.SourceFilePath).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            result.AddError("SourceFilePath", $"File extension '{extension}' is not supported. Allowed: {string.Join(", ", allowedExtensions)}");
        }
    }

    private void ValidateExtractionInfo(Invoice invoice, ValidationResult result)
    {
        if (invoice.ExtractionConfidence < 0.0f || invoice.ExtractionConfidence > 1.0f)
        {
            result.AddError("ExtractionConfidence", "Extraction confidence must be between 0.0 and 1.0");
        }

        if (string.IsNullOrWhiteSpace(invoice.ModelVersion))
        {
            result.AddWarning("ModelVersion", "Model version is not specified");
        }
        else if (invoice.ModelVersion.Length > 50)
        {
            result.AddError("ModelVersion", "Model version cannot exceed 50 characters");
        }
    }
}
```

## 2. Validation Result

**Datei:** `src/InvoiceReader.Domain/Validators/ValidationResult.cs`

```csharp
namespace InvoiceReader.Domain.Validators;

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
```

## 3. Business Rules

**Datei:** `src/InvoiceReader.Domain/BusinessRules/InvoiceBusinessRules.cs`

```csharp
using InvoiceReader.Domain.Entities;
using InvoiceReader.Domain.ValueObjects;

namespace InvoiceReader.Domain.BusinessRules;

public static class InvoiceBusinessRules
{
    public static bool IsDuplicateInvoice(Invoice invoice, IEnumerable<Invoice> existingInvoices)
    {
        return existingInvoices.Any(existing =>
            existing.InvoiceNumber == invoice.InvoiceNumber &&
            existing.GrossTotal == invoice.GrossTotal &&
            Math.Abs((existing.InvoiceDate.ToDateTime(TimeOnly.MinValue) - invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays) <= 7);
    }

    public static bool IsSuspiciousAmount(Invoice invoice)
    {
        // Flag invoices with unusual amounts
        return invoice.GrossTotal > 1000000m || // Over 1 million
               invoice.GrossTotal < 0.01m ||   // Less than 1 cent
               invoice.VatRate > 50m ||        // VAT rate over 50%
               invoice.VatRate < 0m;           // Negative VAT rate
    }

    public static bool IsHighConfidenceExtraction(Invoice invoice)
    {
        return invoice.ExtractionConfidence >= 0.8f;
    }

    public static bool IsLowConfidenceExtraction(Invoice invoice)
    {
        return invoice.ExtractionConfidence < 0.3f;
    }

    public static bool IsRecentInvoice(Invoice invoice, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return invoice.ImportedAt >= cutoffDate;
    }

    public static bool IsValidVatCalculation(Invoice invoice)
    {
        var calculatedTotal = invoice.NetTotal + invoice.VatTotal;
        var difference = Math.Abs(calculatedTotal - invoice.GrossTotal);
        return difference <= 0.02m; // 2 cents tolerance
    }

    public static bool IsStandardVatRate(Invoice invoice)
    {
        if (invoice.NetTotal <= 0) return false;

        var vatRate = (invoice.VatTotal / invoice.NetTotal) * 100;
        var standardRates = new[] { 0m, 7m, 19m, 21m }; // Common VAT rates

        return standardRates.Any(rate => Math.Abs(vatRate - rate) < 0.1m);
    }

    public static bool IsCompleteAddress(Invoice invoice)
    {
        return !string.IsNullOrWhiteSpace(invoice.IssuerName) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerStreet) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerPostalCode) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerCity);
    }

    public static bool IsCompleteFinancials(Invoice invoice)
    {
        return invoice.NetTotal > 0 &&
               invoice.VatTotal >= 0 &&
               invoice.GrossTotal > 0;
    }

    public static bool IsReadyForProcessing(Invoice invoice)
    {
        return !string.IsNullOrWhiteSpace(invoice.InvoiceNumber) &&
               invoice.InvoiceDate != default &&
               !string.IsNullOrWhiteSpace(invoice.IssuerName) &&
               IsCompleteFinancials(invoice) &&
               !string.IsNullOrWhiteSpace(invoice.SourceFilePath);
    }
}
```

## 4. Domain Policies

**Datei:** `src/InvoiceReader.Domain/Policies/InvoicePolicies.cs`

```csharp
using InvoiceReader.Domain.Entities;
using InvoiceReader.Domain.ValueObjects;

namespace InvoiceReader.Domain.Policies;

public class InvoicePolicies
{
    public static bool CanImportInvoice(Invoice invoice, IEnumerable<Invoice> existingInvoices)
    {
        // Check for duplicates
        if (InvoiceBusinessRules.IsDuplicateInvoice(invoice, existingInvoices))
            return false;

        // Check if invoice is ready for processing
        if (!InvoiceBusinessRules.IsReadyForProcessing(invoice))
            return false;

        return true;
    }

    public static bool CanUpdateInvoice(Invoice invoice, Invoice updatedInvoice)
    {
        // Cannot update to invalid state
        if (!InvoiceBusinessRules.IsReadyForProcessing(updatedInvoice))
            return false;

        // Cannot change invoice number to existing one
        if (updatedInvoice.InvoiceNumber != invoice.InvoiceNumber)
        {
            // This would need to be checked against existing invoices
            // Implementation depends on repository access
        }

        return true;
    }

    public static bool CanDeleteInvoice(Invoice invoice)
    {
        // Cannot delete if it's the only invoice from this issuer
        // Cannot delete if it's referenced by other entities
        // Implementation depends on business requirements

        return true; // Simplified for now
    }

    public static bool RequiresManualReview(Invoice invoice)
    {
        return InvoiceBusinessRules.IsLowConfidenceExtraction(invoice) ||
               InvoiceBusinessRules.IsSuspiciousAmount(invoice) ||
               !InvoiceBusinessRules.IsValidVatCalculation(invoice) ||
               !InvoiceBusinessRules.IsCompleteAddress(invoice);
    }

    public static bool CanAutoProcess(Invoice invoice)
    {
        return InvoiceBusinessRules.IsHighConfidenceExtraction(invoice) &&
               InvoiceBusinessRules.IsValidVatCalculation(invoice) &&
               InvoiceBusinessRules.IsCompleteAddress(invoice) &&
               InvoiceBusinessRules.IsCompleteFinancials(invoice) &&
               !InvoiceBusinessRules.IsSuspiciousAmount(invoice);
    }
}
```

## 5. Domain Events

**Datei:** `src/InvoiceReader.Domain/Events/InvoiceDomainEvents.cs`

```csharp
using InvoiceReader.Domain.Entities;

namespace InvoiceReader.Domain.Events;

public abstract class DomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class InvoiceCreatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public decimal GrossTotal { get; set; }
    public float ExtractionConfidence { get; set; }
}

public class InvoiceUpdatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string[] ChangedFields { get; set; } = Array.Empty<string>();
    public string ChangedBy { get; set; } = string.Empty;
}

public class InvoiceValidatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string[] ValidationErrors { get; set; } = Array.Empty<string>();
    public string[] ValidationWarnings { get; set; } = Array.Empty<string>();
}

public class InvoiceSuspiciousAmountEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
```

## Wichtige Hinweise

- Umfassende Validierung für alle Invoice-Properties
- Business Rules für Domain-spezifische Logik
- Policies für komplexe Geschäftsregeln
- Domain Events für Audit-Trail
- ValidationResult für strukturierte Fehlerbehandlung
- Warnings für verdächtige aber nicht fehlerhafte Werte
- Typsichere Business Rules
- Erweiterbare Validierungslogik
