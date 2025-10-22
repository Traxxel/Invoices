using Invoice.Domain.Entities;

namespace Invoice.Application.Interfaces;

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

