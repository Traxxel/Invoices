using System.Linq.Expressions;

namespace Invoice.Domain.Entities;

public static class InvoiceSpecifications
{
    public static Expression<Func<Invoice, bool>> ByInvoiceNumber(string invoiceNumber)
    {
        return x => x.InvoiceNumber == invoiceNumber;
    }

    public static Expression<Func<Invoice, bool>> ByIssuerName(string issuerName)
    {
        return x => x.IssuerName.Contains(issuerName);
    }

    public static Expression<Func<Invoice, bool>> ByDateRange(DateOnly startDate, DateOnly endDate)
    {
        return x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate;
    }

    public static Expression<Func<Invoice, bool>> ByAmountRange(decimal minAmount, decimal maxAmount)
    {
        return x => x.GrossTotal >= minAmount && x.GrossTotal <= maxAmount;
    }

    public static Expression<Func<Invoice, bool>> ByConfidenceThreshold(float threshold)
    {
        return x => x.ExtractionConfidence >= threshold;
    }

    public static Expression<Func<Invoice, bool>> ByModelVersion(string modelVersion)
    {
        return x => x.ModelVersion == modelVersion;
    }

    public static Expression<Func<Invoice, bool>> RecentlyImported(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return x => x.ImportedAt >= cutoffDate;
    }
}

