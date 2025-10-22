namespace Invoice.Application.Interfaces;

public interface IRetentionPolicyService
{
    // Policy configuration
    Task<bool> SetRetentionPolicyAsync(int retentionYears);
    Task<int> GetRetentionPolicyAsync();
    Task<bool> IsRetentionPolicyEnabledAsync();

    // Cleanup operations
    Task<CleanupResult> CleanupExpiredFilesAsync();
    Task<CleanupResult> CleanupExpiredInvoicesAsync();
    Task<CleanupResult> CleanupExpiredRawBlocksAsync();
    Task<CleanupResult> CleanupAllExpiredDataAsync();

    // Analysis
    Task<RetentionAnalysis> AnalyzeRetentionAsync();
    Task<List<ExpiredItem>> GetExpiredItemsAsync();
    Task<long> GetExpiredDataSizeAsync();
    Task<int> GetExpiredItemCountAsync();

    // Manual operations
    Task<bool> ArchiveExpiredDataAsync(string archivePath);
    Task<bool> RestoreArchivedDataAsync(string archivePath);
    Task<bool> DeleteExpiredDataAsync();
}

public class CleanupResult
{
    public bool Success { get; set; }
    public int FilesDeleted { get; set; }
    public int InvoicesDeleted { get; set; }
    public int RawBlocksDeleted { get; set; }
    public long SpaceFreed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime CleanupDate { get; set; } = DateTime.UtcNow;
}

public class RetentionAnalysis
{
    public int TotalItems { get; set; }
    public int ExpiredItems { get; set; }
    public int ItemsToExpireSoon { get; set; }
    public long TotalSize { get; set; }
    public long ExpiredSize { get; set; }
    public long SpaceToBeFreed { get; set; }
    public DateTime OldestItem { get; set; }
    public DateTime NewestItem { get; set; }
    public Dictionary<int, int> ItemsByYear { get; set; } = new();
    public Dictionary<int, long> SizeByYear { get; set; } = new();
}

public class ExpiredItem
{
    public string Type { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public long Size { get; set; }
    public string Path { get; set; } = string.Empty;
}

