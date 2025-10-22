using Invoice.Application.Interfaces;
using Invoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Data.Repositories;

public class InvoiceRawBlockRepository : IInvoiceRawBlockRepository
{
    private readonly InvoiceDbContext _context;
    private readonly DbSet<InvoiceRawBlock> _rawBlocks;

    public InvoiceRawBlockRepository(InvoiceDbContext context)
    {
        _context = context;
        _rawBlocks = context.InvoiceRawBlocks;
    }

    public async Task<InvoiceRawBlock?> GetByIdAsync(Guid id)
    {
        return await _rawBlocks
            .Include(rb => rb.Invoice)
            .FirstOrDefaultAsync(rb => rb.Id == id);
    }

    public async Task<List<InvoiceRawBlock>> GetAllAsync()
    {
        return await _rawBlocks
            .Include(rb => rb.Invoice)
            .OrderBy(rb => rb.CreatedAt)
            .ToListAsync();
    }

    public async Task<InvoiceRawBlock> AddAsync(InvoiceRawBlock rawBlock)
    {
        _rawBlocks.Add(rawBlock);
        await _context.SaveChangesAsync();
        return rawBlock;
    }

    public async Task<InvoiceRawBlock> UpdateAsync(InvoiceRawBlock rawBlock)
    {
        _rawBlocks.Update(rawBlock);
        await _context.SaveChangesAsync();
        return rawBlock;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rawBlock = await _rawBlocks.FindAsync(id);
        if (rawBlock == null) return false;

        _rawBlocks.Remove(rawBlock);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _rawBlocks.AnyAsync(rb => rb.Id == id);
    }

    public async Task<List<InvoiceRawBlock>> GetByInvoiceIdAsync(Guid invoiceId)
    {
        return await _rawBlocks
            .Where(rb => rb.InvoiceId == invoiceId)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetByPageAsync(int page)
    {
        return await _rawBlocks
            .Where(rb => rb.Page == page)
            .OrderBy(rb => rb.Y)
            .ThenBy(rb => rb.X)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetByPredictedLabelAsync(string predictedLabel)
    {
        return await _rawBlocks
            .Where(rb => rb.PredictedLabel == predictedLabel)
            .OrderByDescending(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetByActualLabelAsync(string actualLabel)
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel == actualLabel)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetByConfidenceRangeAsync(float minConfidence, float maxConfidence)
    {
        return await _rawBlocks
            .Where(rb => rb.PredictionConfidence >= minConfidence && rb.PredictionConfidence <= maxConfidence)
            .OrderByDescending(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetHighConfidenceAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.PredictionConfidence >= 0.8f)
            .OrderByDescending(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetLowConfidenceAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.PredictionConfidence != null && rb.PredictionConfidence < 0.3f)
            .OrderBy(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetCorrectlyPredictedAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.PredictedLabel != null && rb.ActualLabel != null && rb.PredictedLabel == rb.ActualLabel)
            .OrderByDescending(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetMisclassifiedAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.PredictedLabel != null && rb.ActualLabel != null && rb.PredictedLabel != rb.ActualLabel)
            .OrderBy(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetUnlabeledAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel == null)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> SearchByTextAsync(string searchText)
    {
        return await _rawBlocks
            .Where(rb => rb.Text.Contains(searchText))
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetByPositionAsync(float minX, float maxX, float minY, float maxY)
    {
        return await _rawBlocks
            .Where(rb => rb.X >= minX && rb.X <= maxX && rb.Y >= minY && rb.Y <= maxY)
            .OrderBy(rb => rb.Y)
            .ThenBy(rb => rb.X)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _rawBlocks.CountAsync();
    }

    public async Task<int> GetCountByInvoiceIdAsync(Guid invoiceId)
    {
        return await _rawBlocks.CountAsync(rb => rb.InvoiceId == invoiceId);
    }

    public async Task<Dictionary<string, int>> GetPredictedLabelStatisticsAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.PredictedLabel != null)
            .GroupBy(rb => rb.PredictedLabel!)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Label, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetActualLabelStatisticsAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel != null)
            .GroupBy(rb => rb.ActualLabel!)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Label, x => x.Count);
    }

    public async Task<float> GetAverageConfidenceAsync()
    {
        var blocks = await _rawBlocks
            .Where(rb => rb.PredictionConfidence != null)
            .ToListAsync();

        if (!blocks.Any()) return 0;
        return blocks.Average(rb => rb.PredictionConfidence!.Value);
    }

    public async Task<Dictionary<string, float>> GetConfidenceByLabelAsync()
    {
        var blocks = await _rawBlocks
            .Where(rb => rb.PredictedLabel != null && rb.PredictionConfidence != null)
            .ToListAsync();

        return blocks
            .GroupBy(rb => rb.PredictedLabel!)
            .ToDictionary(
                g => g.Key,
                g => g.Average(rb => rb.PredictionConfidence!.Value)
            );
    }

    public async Task<List<InvoiceRawBlock>> GetTrainingDataAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel != null)
            .OrderBy(x => Guid.NewGuid())
            .Take((int)(_rawBlocks.Count() * 0.8))
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetValidationDataAsync()
    {
        var totalCount = await _rawBlocks.Where(rb => rb.ActualLabel != null).CountAsync();
        var skip = (int)(totalCount * 0.8);
        var take = (int)(totalCount * 0.1);

        return await _rawBlocks
            .Where(rb => rb.ActualLabel != null)
            .OrderBy(rb => rb.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetTestDataAsync()
    {
        var totalCount = await _rawBlocks.Where(rb => rb.ActualLabel != null).CountAsync();
        var skip = (int)(totalCount * 0.9);

        return await _rawBlocks
            .Where(rb => rb.ActualLabel != null)
            .OrderBy(rb => rb.CreatedAt)
            .Skip(skip)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetLabeledDataAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel != null)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> GetUnlabeledDataAsync()
    {
        return await _rawBlocks
            .Where(rb => rb.ActualLabel == null)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public async Task<List<InvoiceRawBlock>> AddRangeAsync(List<InvoiceRawBlock> rawBlocks)
    {
        _rawBlocks.AddRange(rawBlocks);
        await _context.SaveChangesAsync();
        return rawBlocks;
    }

    public async Task<bool> DeleteRangeAsync(List<Guid> ids)
    {
        var rawBlocks = await _rawBlocks.Where(rb => ids.Contains(rb.Id)).ToListAsync();
        if (!rawBlocks.Any()) return false;

        _rawBlocks.RemoveRange(rawBlocks);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<InvoiceRawBlock>> UpdateRangeAsync(List<InvoiceRawBlock> rawBlocks)
    {
        _rawBlocks.UpdateRange(rawBlocks);
        await _context.SaveChangesAsync();
        return rawBlocks;
    }

    public async Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId)
    {
        var rawBlocks = await _rawBlocks.Where(rb => rb.InvoiceId == invoiceId).ToListAsync();
        if (!rawBlocks.Any()) return false;

        _rawBlocks.RemoveRange(rawBlocks);
        await _context.SaveChangesAsync();
        return true;
    }
}

