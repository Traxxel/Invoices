using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Data.Extensions;

public static class DbContextExtensions
{
    public static async Task<Domain.Entities.Invoice?> FindInvoiceByNumberAsync(this InvoiceDbContext context, string invoiceNumber)
    {
        return await context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public static async Task<List<Domain.Entities.Invoice>> FindInvoicesByIssuerAsync(this InvoiceDbContext context, string issuerName)
    {
        return await context.Invoices
            .Where(i => i.IssuerName.Contains(issuerName))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public static async Task<List<Domain.Entities.Invoice>> FindInvoicesByDateRangeAsync(
        this InvoiceDbContext context,
        DateOnly startDate,
        DateOnly endDate)
    {
        return await context.Invoices
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public static async Task<List<Domain.Entities.Invoice>> FindInvoicesByAmountRangeAsync(
        this InvoiceDbContext context,
        decimal minAmount,
        decimal maxAmount)
    {
        return await context.Invoices
            .Where(i => i.GrossTotal >= minAmount && i.GrossTotal <= maxAmount)
            .OrderByDescending(i => i.GrossTotal)
            .ToListAsync();
    }

    public static async Task<List<Domain.Entities.Invoice>> FindInvoicesByConfidenceAsync(
        this InvoiceDbContext context,
        float minConfidence)
    {
        return await context.Invoices
            .Where(i => i.ExtractionConfidence >= minConfidence)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public static async Task<List<Domain.Entities.InvoiceRawBlock>> FindRawBlocksByInvoiceAsync(
        this InvoiceDbContext context,
        Guid invoiceId)
    {
        return await context.InvoiceRawBlocks
            .Where(rb => rb.InvoiceId == invoiceId)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public static async Task<List<Domain.Entities.InvoiceRawBlock>> FindRawBlocksByPredictionAsync(
        this InvoiceDbContext context,
        string predictedLabel)
    {
        return await context.InvoiceRawBlocks
            .Where(rb => rb.PredictedLabel == predictedLabel)
            .OrderBy(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public static async Task<bool> InvoiceNumberExistsAsync(this InvoiceDbContext context, string invoiceNumber)
    {
        return await context.Invoices
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public static async Task<int> GetInvoiceCountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.CountAsync();
    }

    public static async Task<decimal> GetTotalInvoiceAmountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.SumAsync(i => i.GrossTotal);
    }

    public static async Task<decimal> GetAverageInvoiceAmountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.AverageAsync(i => i.GrossTotal);
    }

    public static async Task<DateOnly?> GetEarliestInvoiceDateAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.MinAsync(i => i.InvoiceDate);
    }

    public static async Task<DateOnly?> GetLatestInvoiceDateAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.MaxAsync(i => i.InvoiceDate);
    }
}

