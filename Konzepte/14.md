# Aufgabe 14: Repository Pattern Interfaces und Implementation

## Ziel

Repository Pattern für saubere Trennung von Domain und Infrastructure mit vollständigen CRUD-Operationen.

## 1. Repository Interfaces

**Datei:** `src/InvoiceReader.Application/Interfaces/IInvoiceRepository.cs`

```csharp
using InvoiceReader.Domain.Entities;
using InvoiceReader.Domain.ValueObjects;

namespace InvoiceReader.Application.Interfaces;

public interface IInvoiceRepository
{
    // Basic CRUD
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice> AddAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Query methods
    Task<List<Invoice>> GetByIssuerAsync(string issuerName);
    Task<List<Invoice>> GetByDateRangeAsync(DateRange dateRange);
    Task<List<Invoice>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);
    Task<List<Invoice>> GetByConfidenceAsync(float minConfidence);
    Task<List<Invoice>> GetByModelVersionAsync(string modelVersion);
    Task<List<Invoice>> GetRecentAsync(int days = 30);
    Task<List<Invoice>> GetLowConfidenceAsync();
    Task<List<Invoice>> GetHighConfidenceAsync();

    // Search methods
    Task<List<Invoice>> SearchAsync(string searchTerm);
    Task<List<Invoice>> SearchByInvoiceNumberAsync(string invoiceNumber);
    Task<List<Invoice>> SearchByIssuerAsync(string issuerName);

    // Statistics
    Task<int> GetCountAsync();
    Task<decimal> GetTotalAmountAsync();
    Task<decimal> GetAverageAmountAsync();
    Task<DateOnly?> GetEarliestDateAsync();
    Task<DateOnly?> GetLatestDateAsync();
    Task<Dictionary<string, int>> GetIssuerStatisticsAsync();
    Task<Dictionary<string, int>> GetModelVersionStatisticsAsync();

    // Pagination
    Task<(List<Invoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<(List<Invoice> Items, int TotalCount)> GetPagedByIssuerAsync(string issuerName, int page, int pageSize);
    Task<(List<Invoice> Items, int TotalCount)> GetPagedByDateRangeAsync(DateRange dateRange, int page, int pageSize);

    // Duplicate detection
    Task<List<Invoice>> FindDuplicatesAsync();
    Task<bool> IsDuplicateAsync(string invoiceNumber, decimal grossTotal, DateOnly invoiceDate);

    // Batch operations
    Task<List<Invoice>> AddRangeAsync(List<Invoice> invoices);
    Task<bool> DeleteRangeAsync(List<Guid> ids);
    Task<List<Invoice>> UpdateRangeAsync(List<Invoice> invoices);
}
```

**Datei:** `src/InvoiceReader.Application/Interfaces/IInvoiceRawBlockRepository.cs`

```csharp
using InvoiceReader.Domain.Entities;

namespace InvoiceReader.Application.Interfaces;

public interface IInvoiceRawBlockRepository
{
    // Basic CRUD
    Task<InvoiceRawBlock?> GetByIdAsync(Guid id);
    Task<List<InvoiceRawBlock>> GetAllAsync();
    Task<InvoiceRawBlock> AddAsync(InvoiceRawBlock rawBlock);
    Task<InvoiceRawBlock> UpdateAsync(InvoiceRawBlock rawBlock);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Query methods
    Task<List<InvoiceRawBlock>> GetByInvoiceIdAsync(Guid invoiceId);
    Task<List<InvoiceRawBlock>> GetByPageAsync(int page);
    Task<List<InvoiceRawBlock>> GetByPredictedLabelAsync(string predictedLabel);
    Task<List<InvoiceRawBlock>> GetByActualLabelAsync(string actualLabel);
    Task<List<InvoiceRawBlock>> GetByConfidenceRangeAsync(float minConfidence, float maxConfidence);
    Task<List<InvoiceRawBlock>> GetHighConfidenceAsync();
    Task<List<InvoiceRawBlock>> GetLowConfidenceAsync();
    Task<List<InvoiceRawBlock>> GetCorrectlyPredictedAsync();
    Task<List<InvoiceRawBlock>> GetMisclassifiedAsync();
    Task<List<InvoiceRawBlock>> GetUnlabeledAsync();

    // Search methods
    Task<List<InvoiceRawBlock>> SearchByTextAsync(string searchText);
    Task<List<InvoiceRawBlock>> GetByPositionAsync(float minX, float maxX, float minY, float maxY);

    // Statistics
    Task<int> GetCountAsync();
    Task<int> GetCountByInvoiceIdAsync(Guid invoiceId);
    Task<Dictionary<string, int>> GetPredictedLabelStatisticsAsync();
    Task<Dictionary<string, int>> GetActualLabelStatisticsAsync();
    Task<float> GetAverageConfidenceAsync();
    Task<Dictionary<string, float>> GetConfidenceByLabelAsync();

    // Training data
    Task<List<InvoiceRawBlock>> GetTrainingDataAsync();
    Task<List<InvoiceRawBlock>> GetValidationDataAsync();
    Task<List<InvoiceRawBlock>> GetTestDataAsync();
    Task<List<InvoiceRawBlock>> GetLabeledDataAsync();
    Task<List<InvoiceRawBlock>> GetUnlabeledDataAsync();

    // Batch operations
    Task<List<InvoiceRawBlock>> AddRangeAsync(List<InvoiceRawBlock> rawBlocks);
    Task<bool> DeleteRangeAsync(List<Guid> ids);
    Task<List<InvoiceRawBlock>> UpdateRangeAsync(List<InvoiceRawBlock> rawBlocks);
    Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId);
}
```

## 2. Repository Implementations

**Datei:** `src/InvoiceReader.Infrastructure/Data/Repositories/InvoiceRepository.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Domain.Entities;
using InvoiceReader.Domain.ValueObjects;
using InvoiceReader.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceReader.Infrastructure.Data.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _context;
    private readonly DbSet<Invoice> _invoices;

    public InvoiceRepository(InvoiceDbContext context)
    {
        _context = context;
        _invoices = context.Invoices;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _invoices
            .Include(i => i.RawBlocks)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<Invoice> AddAsync(Invoice invoice)
    {
        _invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
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

    public async Task<List<Invoice>> GetByIssuerAsync(string issuerName)
    {
        return await _invoices
            .Where(i => i.IssuerName.Contains(issuerName))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetByDateRangeAsync(DateRange dateRange)
    {
        return await _invoices
            .Where(i => i.InvoiceDate >= dateRange.StartDate && i.InvoiceDate <= dateRange.EndDate)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
    {
        return await _invoices
            .Where(i => i.GrossTotal >= minAmount && i.GrossTotal <= maxAmount)
            .OrderByDescending(i => i.GrossTotal)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetByConfidenceAsync(float minConfidence)
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence >= minConfidence)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetByModelVersionAsync(string modelVersion)
    {
        return await _invoices
            .Where(i => i.ModelVersion == modelVersion)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetRecentAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _invoices
            .Where(i => i.ImportedAt >= cutoffDate)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetLowConfidenceAsync()
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence < 0.3f)
            .OrderBy(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetHighConfidenceAsync()
    {
        return await _invoices
            .Where(i => i.ExtractionConfidence >= 0.8f)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public async Task<List<Invoice>> SearchAsync(string searchTerm)
    {
        return await _invoices
            .Where(i => i.InvoiceNumber.Contains(searchTerm) ||
                       i.IssuerName.Contains(searchTerm) ||
                       i.IssuerCity.Contains(searchTerm))
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<Invoice>> SearchByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _invoices
            .Where(i => i.InvoiceNumber.Contains(invoiceNumber))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> SearchByIssuerAsync(string issuerName)
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
        return await _invoices.AverageAsync(i => i.GrossTotal);
    }

    public async Task<DateOnly?> GetEarliestDateAsync()
    {
        return await _invoices.MinAsync(i => i.InvoiceDate);
    }

    public async Task<DateOnly?> GetLatestDateAsync()
    {
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
            .GroupBy(i => i.ModelVersion)
            .Select(g => new { Model = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Model, x => x.Count);
    }

    public async Task<(List<Invoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _invoices.CountAsync();
        var items = await _invoices
            .OrderByDescending(i => i.ImportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Invoice> Items, int TotalCount)> GetPagedByIssuerAsync(string issuerName, int page, int pageSize)
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

    public async Task<(List<Invoice> Items, int TotalCount)> GetPagedByDateRangeAsync(DateRange dateRange, int page, int pageSize)
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

    public async Task<List<Invoice>> FindDuplicatesAsync()
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

    public async Task<List<Invoice>> AddRangeAsync(List<Invoice> invoices)
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

    public async Task<List<Invoice>> UpdateRangeAsync(List<Invoice> invoices)
    {
        _invoices.UpdateRange(invoices);
        await _context.SaveChangesAsync();
        return invoices;
    }
}
```

## 3. Repository Service Registration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Extensions/RepositoryExtensions.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Data.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceRawBlockRepository, InvoiceRawBlockRepository>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständige Repository Interfaces für beide Entities
- CRUD-Operationen mit Include für Navigation Properties
- Query-Methoden für häufige Abfragen
- Search-Funktionalität für Textsuche
- Statistics-Methoden für Reporting
- Pagination für große Datenmengen
- Duplicate Detection für Business Logic
- Batch-Operationen für Performance
- Service Registration für DI-Container
- Async/await für alle Database-Operationen
