namespace Invoice.Application.Interfaces;

public interface IFileStorageService
{
    // File operations
    Task<string> StoreFileAsync(string sourcePath, Guid invoiceId);
    Task<string> StoreFileAsync(Stream fileStream, string fileName, Guid invoiceId);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    Task<Stream> GetFileStreamAsync(string filePath);
    Task<byte[]> GetFileBytesAsync(string filePath);

    // Path operations
    string GetStoragePath(Guid invoiceId);
    string GetRelativePath(string absolutePath);
    string GetAbsolutePath(string relativePath);
    string GetFileName(Guid invoiceId);
    string GetDirectoryPath(Guid invoiceId);

    // File information
    Task<FileInformation> GetFileInfoAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    Task<DateTime> GetFileCreationTimeAsync(string filePath);
    Task<DateTime> GetFileModificationTimeAsync(string filePath);

    // Directory operations
    Task<bool> CreateDirectoryAsync(string path);
    Task<bool> DirectoryExistsAsync(string path);
    Task<List<string>> GetFilesInDirectoryAsync(string path);
    Task<List<string>> GetSubdirectoriesAsync(string path);

    // Cleanup operations
    Task<bool> CleanupOldFilesAsync(int retentionDays);
    Task<bool> CleanupEmptyDirectoriesAsync();
    Task<long> GetTotalStorageSizeAsync();
    Task<StorageStatistics> GetStorageStatisticsAsync();
}

public class FileInformation
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public bool Exists { get; set; }
}

public class StorageStatistics
{
    public long TotalSize { get; set; }
    public int FileCount { get; set; }
    public int DirectoryCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, long> SizeByYear { get; set; } = new();
    public Dictionary<string, int> FileCountByYear { get; set; } = new();
}

