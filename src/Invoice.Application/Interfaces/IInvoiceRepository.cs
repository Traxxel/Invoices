using Invoice.Domain.ValueObjects;

namespace Invoice.Application.Interfaces;

public interface IInvoiceRepository
{
    // Basic CRUD
    Task<Domain.Entities.Invoice?> GetByIdAsync(Guid id);
    Task<Domain.Entities.Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<List<Domain.Entities.Invoice>> GetAllAsync();
    Task<Domain.Entities.Invoice> AddAsync(Domain.Entities.Invoice invoice);
    Task<Domain.Entities.Invoice> UpdateAsync(Domain.Entities.Invoice invoice);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Query methods
    Task<List<Domain.Entities.Invoice>> GetByIssuerAsync(string issuerName);
    Task<List<Domain.Entities.Invoice>> GetByDateRangeAsync(DateRange dateRange);
    Task<List<Domain.Entities.Invoice>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);
    Task<List<Domain.Entities.Invoice>> GetByConfidenceAsync(float minConfidence);
    Task<List<Domain.Entities.Invoice>> GetByModelVersionAsync(string modelVersion);
    Task<List<Domain.Entities.Invoice>> GetRecentAsync(int days = 30);
    Task<List<Domain.Entities.Invoice>> GetLowConfidenceAsync();
    Task<List<Domain.Entities.Invoice>> GetHighConfidenceAsync();

    // Search methods
    Task<List<Domain.Entities.Invoice>> SearchAsync(string searchTerm);
    Task<List<Domain.Entities.Invoice>> SearchByInvoiceNumberAsync(string invoiceNumber);
    Task<List<Domain.Entities.Invoice>> SearchByIssuerAsync(string issuerName);

    // Statistics
    Task<int> GetCountAsync();
    Task<decimal> GetTotalAmountAsync();
    Task<decimal> GetAverageAmountAsync();
    Task<DateOnly?> GetEarliestDateAsync();
    Task<DateOnly?> GetLatestDateAsync();
    Task<Dictionary<string, int>> GetIssuerStatisticsAsync();
    Task<Dictionary<string, int>> GetModelVersionStatisticsAsync();

    // Pagination
    Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedByIssuerAsync(string issuerName, int page, int pageSize);
    Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedByDateRangeAsync(DateRange dateRange, int page, int pageSize);

    // Duplicate detection
    Task<List<Domain.Entities.Invoice>> FindDuplicatesAsync();
    Task<bool> IsDuplicateAsync(string invoiceNumber, decimal grossTotal, DateOnly invoiceDate);

    // Batch operations
    Task<List<Domain.Entities.Invoice>> AddRangeAsync(List<Domain.Entities.Invoice> invoices);
    Task<bool> DeleteRangeAsync(List<Guid> ids);
    Task<List<Domain.Entities.Invoice>> UpdateRangeAsync(List<Domain.Entities.Invoice> invoices);
}

