using Invoice.Domain.Entities;

namespace Invoice.Domain.BusinessRules;

public static class InvoiceBusinessRules
{
    public static bool IsDuplicateInvoice(Entities.Invoice invoice, IEnumerable<Entities.Invoice> existingInvoices)
    {
        return existingInvoices.Any(existing =>
            existing.InvoiceNumber == invoice.InvoiceNumber &&
            existing.GrossTotal == invoice.GrossTotal &&
            Math.Abs((existing.InvoiceDate.ToDateTime(TimeOnly.MinValue) - invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays) <= 7);
    }

    public static bool IsSuspiciousAmount(Entities.Invoice invoice)
    {
        // Flag invoices with unusual amounts
        return invoice.GrossTotal > 1000000m || // Over 1 million
               invoice.GrossTotal < 0.01m ||   // Less than 1 cent
               invoice.VatRate > 50m ||        // VAT rate over 50%
               invoice.VatRate < 0m;           // Negative VAT rate
    }

    public static bool IsHighConfidenceExtraction(Entities.Invoice invoice)
    {
        return invoice.ExtractionConfidence >= 0.8f;
    }

    public static bool IsLowConfidenceExtraction(Entities.Invoice invoice)
    {
        return invoice.ExtractionConfidence < 0.3f;
    }

    public static bool IsRecentInvoice(Entities.Invoice invoice, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return invoice.ImportedAt >= cutoffDate;
    }

    public static bool IsValidVatCalculation(Entities.Invoice invoice)
    {
        var calculatedTotal = invoice.NetTotal + invoice.VatTotal;
        var difference = Math.Abs(calculatedTotal - invoice.GrossTotal);
        return difference <= 0.02m; // 2 cents tolerance
    }

    public static bool IsStandardVatRate(Entities.Invoice invoice)
    {
        if (invoice.NetTotal <= 0) return false;

        var vatRate = (invoice.VatTotal / invoice.NetTotal) * 100;
        var standardRates = new[] { 0m, 7m, 19m, 21m }; // Common VAT rates

        return standardRates.Any(rate => Math.Abs(vatRate - rate) < 0.1m);
    }

    public static bool IsCompleteAddress(Entities.Invoice invoice)
    {
        return !string.IsNullOrWhiteSpace(invoice.IssuerName) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerStreet) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerPostalCode) &&
               !string.IsNullOrWhiteSpace(invoice.IssuerCity);
    }

    public static bool IsCompleteFinancials(Entities.Invoice invoice)
    {
        return invoice.NetTotal > 0 &&
               invoice.VatTotal >= 0 &&
               invoice.GrossTotal > 0;
    }

    public static bool IsReadyForProcessing(Entities.Invoice invoice)
    {
        return !string.IsNullOrWhiteSpace(invoice.InvoiceNumber) &&
               invoice.InvoiceDate != default &&
               !string.IsNullOrWhiteSpace(invoice.IssuerName) &&
               IsCompleteFinancials(invoice) &&
               !string.IsNullOrWhiteSpace(invoice.SourceFilePath);
    }
}

