using Invoice.Domain.Entities;

namespace Invoice.Domain.Validators;

public class InvoiceValidator
{
    public ValidationResult Validate(Entities.Invoice invoice)
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

    private void ValidateInvoiceNumber(Entities.Invoice invoice, ValidationResult result)
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

    private void ValidateInvoiceDate(Entities.Invoice invoice, ValidationResult result)
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

    private void ValidateIssuerInfo(Entities.Invoice invoice, ValidationResult result)
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

    private void ValidateFinancials(Entities.Invoice invoice, ValidationResult result)
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

    private void ValidateFileInfo(Entities.Invoice invoice, ValidationResult result)
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

    private void ValidateExtractionInfo(Entities.Invoice invoice, ValidationResult result)
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

