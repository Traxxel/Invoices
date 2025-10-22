using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Services;

public class RetentionPolicyService : IRetentionPolicyService
{
    private readonly InvoiceDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<RetentionPolicyService> _logger;
    private int _retentionYears = 10; // Default retention period

    public RetentionPolicyService(
        InvoiceDbContext context,
        IFileStorageService fileStorageService,
        ILogger<RetentionPolicyService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<bool> SetRetentionPolicyAsync(int retentionYears)
    {
        try
        {
            if (retentionYears < 1 || retentionYears > 50)
            {
                throw new ArgumentException("Retention years must be between 1 and 50", nameof(retentionYears));
            }

            _retentionYears = retentionYears;

            _logger.LogInformation("Retention policy set to {Years} years", retentionYears);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set retention policy");
            return false;
        }
    }

    public async Task<int> GetRetentionPolicyAsync()
    {
        return _retentionYears;
    }

    public async Task<bool> IsRetentionPolicyEnabledAsync()
    {
        return _retentionYears > 0;
    }

    public async Task<CleanupResult> CleanupExpiredFilesAsync()
    {
        var result = new CleanupResult();

        try
        {
            _logger.LogInformation("Starting cleanup of expired files...");

            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredInvoices = await _context.Invoices
                .Where(i => i.ImportedAt < cutoffDate)
                .ToListAsync();

            foreach (var invoice in expiredInvoices)
            {
                try
                {
                    if (await _fileStorageService.FileExistsAsync(invoice.SourceFilePath))
                    {
                        var fileSize = await _fileStorageService.GetFileSizeAsync(invoice.SourceFilePath);
                        if (await _fileStorageService.DeleteFileAsync(invoice.SourceFilePath))
                        {
                            result.FilesDeleted++;
                            result.SpaceFreed += fileSize;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to delete file for invoice {invoice.Id}: {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;
            _logger.LogInformation("File cleanup completed: {FilesDeleted} files deleted, {SpaceFreed} bytes freed",
                result.FilesDeleted, result.SpaceFreed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired files");
            result.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return result;
    }

    public async Task<CleanupResult> CleanupExpiredInvoicesAsync()
    {
        var result = new CleanupResult();

        try
        {
            _logger.LogInformation("Starting cleanup of expired invoices...");

            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredInvoices = await _context.Invoices
                .Where(i => i.ImportedAt < cutoffDate)
                .ToListAsync();

            foreach (var invoice in expiredInvoices)
            {
                try
                {
                    // Delete associated raw blocks first
                    var rawBlocks = await _context.InvoiceRawBlocks
                        .Where(rb => rb.InvoiceId == invoice.Id)
                        .ToListAsync();

                    _context.InvoiceRawBlocks.RemoveRange(rawBlocks);
                    result.RawBlocksDeleted += rawBlocks.Count;

                    // Delete invoice
                    _context.Invoices.Remove(invoice);
                    result.InvoicesDeleted++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to delete invoice {invoice.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.Errors.Count == 0;

            _logger.LogInformation("Invoice cleanup completed: {InvoicesDeleted} invoices deleted, {RawBlocksDeleted} raw blocks deleted",
                result.InvoicesDeleted, result.RawBlocksDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired invoices");
            result.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return result;
    }

    public async Task<CleanupResult> CleanupExpiredRawBlocksAsync()
    {
        var result = new CleanupResult();

        try
        {
            _logger.LogInformation("Starting cleanup of expired raw blocks...");

            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredRawBlocks = await _context.InvoiceRawBlocks
                .Where(rb => rb.CreatedAt < cutoffDate)
                .ToListAsync();

            _context.InvoiceRawBlocks.RemoveRange(expiredRawBlocks);
            await _context.SaveChangesAsync();

            result.RawBlocksDeleted = expiredRawBlocks.Count;
            result.Success = true;

            _logger.LogInformation("Raw blocks cleanup completed: {Count} raw blocks deleted", result.RawBlocksDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired raw blocks");
            result.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return result;
    }

    public async Task<CleanupResult> CleanupAllExpiredDataAsync()
    {
        var result = new CleanupResult();

        try
        {
            _logger.LogInformation("Starting comprehensive cleanup of expired data...");

            // Cleanup files first
            var fileResult = await CleanupExpiredFilesAsync();
            result.FilesDeleted = fileResult.FilesDeleted;
            result.SpaceFreed = fileResult.SpaceFreed;
            result.Errors.AddRange(fileResult.Errors);

            // Cleanup database data
            var invoiceResult = await CleanupExpiredInvoicesAsync();
            result.InvoicesDeleted = invoiceResult.InvoicesDeleted;
            result.RawBlocksDeleted = invoiceResult.RawBlocksDeleted;
            result.Errors.AddRange(invoiceResult.Errors);

            result.Success = result.Errors.Count == 0;

            _logger.LogInformation("Comprehensive cleanup completed: {FilesDeleted} files, {InvoicesDeleted} invoices, {RawBlocksDeleted} raw blocks",
                result.FilesDeleted, result.InvoicesDeleted, result.RawBlocksDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform comprehensive cleanup");
            result.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return result;
    }

    public async Task<RetentionAnalysis> AnalyzeRetentionAsync()
    {
        var analysis = new RetentionAnalysis();

        try
        {
            _logger.LogInformation("Analyzing retention data...");

            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var soonCutoffDate = DateTime.UtcNow.AddYears(-_retentionYears + 1);

            // Analyze invoices
            var allInvoices = await _context.Invoices.ToListAsync();
            var expiredInvoices = allInvoices.Where(i => i.ImportedAt < cutoffDate).ToList();
            var soonExpiredInvoices = allInvoices.Where(i => i.ImportedAt >= cutoffDate && i.ImportedAt < soonCutoffDate).ToList();

            analysis.TotalItems = allInvoices.Count;
            analysis.ExpiredItems = expiredInvoices.Count;
            analysis.ItemsToExpireSoon = soonExpiredInvoices.Count;

            if (allInvoices.Any())
            {
                analysis.OldestItem = allInvoices.Min(i => i.ImportedAt);
                analysis.NewestItem = allInvoices.Max(i => i.ImportedAt);
            }

            // Group by year
            foreach (var invoice in allInvoices)
            {
                var year = invoice.ImportedAt.Year;
                if (!analysis.ItemsByYear.ContainsKey(year))
                {
                    analysis.ItemsByYear[year] = 0;
                    analysis.SizeByYear[year] = 0;
                }

                analysis.ItemsByYear[year]++;
                analysis.SizeByYear[year] += (long)invoice.GrossTotal; // Simplified size calculation
            }

            // Calculate sizes
            analysis.TotalSize = allInvoices.Sum(i => (long)i.GrossTotal);
            analysis.ExpiredSize = expiredInvoices.Sum(i => (long)i.GrossTotal);
            analysis.SpaceToBeFreed = analysis.ExpiredSize;

            _logger.LogInformation("Retention analysis completed: {TotalItems} total, {ExpiredItems} expired, {SoonExpired} soon to expire",
                analysis.TotalItems, analysis.ExpiredItems, analysis.ItemsToExpireSoon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze retention data");
        }

        return analysis;
    }

    public async Task<List<ExpiredItem>> GetExpiredItemsAsync()
    {
        var expiredItems = new List<ExpiredItem>();

        try
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredInvoices = await _context.Invoices
                .Where(i => i.ImportedAt < cutoffDate)
                .ToListAsync();

            foreach (var invoice in expiredInvoices)
            {
                expiredItems.Add(new ExpiredItem
                {
                    Type = "Invoice",
                    Id = invoice.Id,
                    Name = invoice.InvoiceNumber,
                    CreatedDate = invoice.ImportedAt,
                    ExpiryDate = invoice.ImportedAt.AddYears(_retentionYears),
                    Size = (long)invoice.GrossTotal,
                    Path = invoice.SourceFilePath
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get expired items");
        }

        return expiredItems;
    }

    public async Task<long> GetExpiredDataSizeAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredInvoices = await _context.Invoices
                .Where(i => i.ImportedAt < cutoffDate)
                .ToListAsync();

            return expiredInvoices.Sum(i => (long)i.GrossTotal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate expired data size");
            return 0;
        }
    }

    public async Task<int> GetExpiredItemCountAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            return await _context.Invoices
                .CountAsync(i => i.ImportedAt < cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get expired item count");
            return 0;
        }
    }

    public async Task<bool> ArchiveExpiredDataAsync(string archivePath)
    {
        try
        {
            _logger.LogInformation("Archiving expired data to: {ArchivePath}", archivePath);

            var cutoffDate = DateTime.UtcNow.AddYears(-_retentionYears);
            var expiredInvoices = await _context.Invoices
                .Where(i => i.ImportedAt < cutoffDate)
                .ToListAsync();

            // Create archive directory
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            // Archive files and database records
            foreach (var invoice in expiredInvoices)
            {
                // Archive file
                if (await _fileStorageService.FileExistsAsync(invoice.SourceFilePath))
                {
                    var archiveFilePath = Path.Combine(archivePath, $"{invoice.Id}.pdf");
                    var sourceBytes = await _fileStorageService.GetFileBytesAsync(invoice.SourceFilePath);
                    await File.WriteAllBytesAsync(archiveFilePath, sourceBytes);
                }

                // Archive database record (export to JSON)
                var jsonPath = Path.Combine(archivePath, $"{invoice.Id}.json");
                var json = System.Text.Json.JsonSerializer.Serialize(invoice, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(jsonPath, json);
            }

            _logger.LogInformation("Archived {Count} expired items", expiredInvoices.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive expired data");
            return false;
        }
    }

    public async Task<bool> RestoreArchivedDataAsync(string archivePath)
    {
        try
        {
            _logger.LogInformation("Restoring archived data from: {ArchivePath}", archivePath);

            if (!Directory.Exists(archivePath))
            {
                _logger.LogWarning("Archive path does not exist: {ArchivePath}", archivePath);
                return false;
            }

            var jsonFiles = Directory.GetFiles(archivePath, "*.json");
            var restoredCount = 0;

            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(jsonFile);
                    var invoice = System.Text.Json.JsonSerializer.Deserialize<Domain.Entities.Invoice>(json);

                    if (invoice != null)
                    {
                        _context.Invoices.Add(invoice);
                        restoredCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to restore archived item: {JsonFile}", jsonFile);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Restored {Count} archived items", restoredCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore archived data");
            return false;
        }
    }

    public async Task<bool> DeleteExpiredDataAsync()
    {
        try
        {
            _logger.LogInformation("Deleting expired data...");

            var result = await CleanupAllExpiredDataAsync();
            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete expired data");
            return false;
        }
    }
}

