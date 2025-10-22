using Invoice.Application.Interfaces;
using Invoice.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Data.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _context;
    private readonly DbSet<Domain.Entities.Invoice> _invoices;

    public InvoiceRepository(InvoiceDbContext context)
    {
        _context = context;
        _invoices = context.Invoices;
    }

    public async Task<Domain.Entities.Invoice?> GetByIdAsync(Guid id)
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Domain.Entities.Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<List<Domain.Entities.Invoice>> GetAllAsync()
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<Domain.Entities.Invoice> AddAsync(Domain.Entities.Invoice invoice)
    {
        _invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Domain.Entities.Invoice> UpdateAsync(Domain.Entities.Invoice invoice)
    {
        _invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var invoice = await _invoices.FindAsync(id);
        if (invoice == null) return false;

        _invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _invoices.AnyAsync(i => i.Id == id);
    }

    public async Task<List<Domain.Entities.Invoice>> GetByIssuerAsync(string issuerName)
    {
        return await _invoices
            .Where(i => i.IssuerName.Contains(issuerName))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetByDateRangeAsync(DateRange dateRange)
    {
        return await _invoices
            .Where(i => i.InvoiceDate >= dateRange.StartDate && i.InvoiceDate <= dateRange.EndDate)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
    {
        return await _invoices
            .Where(i => i.GrossTotal >= minAmount && i.GrossTotal <= maxAmount)
            .OrderByDescending(i => i.GrossTotal)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetByConfidenceAsync(float minConfidence)
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence >= minConfidence)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetByModelVersionAsync(string modelVersion)
    {
        return await _invoices
            .Where(i => i.ModelVersion == modelVersion)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetRecentAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _invoices
            .Where(i => i.ImportedAt >= cutoffDate)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetLowConfidenceAsync()
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence < 0.3f)
            .OrderBy(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> GetHighConfidenceAsync()
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence >= 0.8f)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> SearchAsync(string searchTerm)
    {
        return await _invoices
            .Where(i => i.InvoiceNumber.Contains(searchTerm) ||
                       i.IssuerName.Contains(searchTerm) ||
                       i.IssuerCity != null && i.IssuerCity.Contains(searchTerm))
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> SearchByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _invoices
            .Where(i => i.InvoiceNumber.Contains(invoiceNumber))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Invoice>> SearchByIssuerAsync(string issuerName)
    {
        return await _invoices
            .Where(i => i.IssuerName.Contains(issuerName))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _invoices.CountAsync();
    }

    public async Task<decimal> GetTotalAmountAsync()
    {
        return await _invoices.SumAsync(i => i.GrossTotal);
    }

    public async Task<decimal> GetAverageAmountAsync()
    {
        var count = await _invoices.CountAsync();
        if (count == 0) return 0;
        return await _invoices.AverageAsync(i => i.GrossTotal);
    }

    public async Task<DateOnly?> GetEarliestDateAsync()
    {
        if (!await _invoices.AnyAsync()) return null;
        return await _invoices.MinAsync(i => i.InvoiceDate);
    }

    public async Task<DateOnly?> GetLatestDateAsync()
    {
        if (!await _invoices.AnyAsync()) return null;
        return await _invoices.MaxAsync(i => i.InvoiceDate);
    }

    public async Task<Dictionary<string, int>> GetIssuerStatisticsAsync()
    {
        return await _invoices
            .GroupBy(i => i.IssuerName)
            .Select(g => new { Issuer = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Issuer, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetModelVersionStatisticsAsync()
    {
        return await _invoices
            .Where(i => i.ModelVersion != null)
            .GroupBy(i => i.ModelVersion!)
            .Select(g => new { Model = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Model, x => x.Count);
    }

    public async Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _invoices.CountAsync();
        var items = await _invoices
            .OrderByDescending(i => i.ImportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedByIssuerAsync(string issuerName, int page, int pageSize)
    {
        var query = _invoices.Where(i => i.IssuerName.Contains(issuerName));
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedByDateRangeAsync(DateRange dateRange, int page, int pageSize)
    {
        var query = _invoices.Where(i => i.InvoiceDate >= dateRange.StartDate && i.InvoiceDate <= dateRange.EndDate);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Domain.Entities.Invoice>> FindDuplicatesAsync()
    {
        return await _invoices
            .GroupBy(i => new { i.InvoiceNumber, i.GrossTotal })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToListAsync();
    }

    public async Task<bool> IsDuplicateAsync(string invoiceNumber, decimal grossTotal, DateOnly invoiceDate)
    {
        return await _invoices.AnyAsync(i =>
            i.InvoiceNumber == invoiceNumber &&
            i.GrossTotal == grossTotal &&
            Math.Abs((i.InvoiceDate.ToDateTime(TimeOnly.MinValue) - invoiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays) <= 7);
    }

    public async Task<List<Domain.Entities.Invoice>> AddRangeAsync(List<Domain.Entities.Invoice> invoices)
    {
        _invoices.AddRange(invoices);
        await _context.SaveChangesAsync();
        return invoices;
    }

    public async Task<bool> DeleteRangeAsync(List<Guid> ids)
    {
        var invoices = await _invoices.Where(i => ids.Contains(i.Id)).ToListAsync();
        if (!invoices.Any()) return false;

        _invoices.RemoveRange(invoices);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Domain.Entities.Invoice>> UpdateRangeAsync(List<Domain.Entities.Invoice> invoices)
    {
        _invoices.UpdateRange(invoices);
        await _context.SaveChangesAsync();
        return invoices;
    }
}

