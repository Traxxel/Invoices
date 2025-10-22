namespace Invoice.Application.Interfaces;

public interface IFileHashService
{
    // Hash operations
    Task<string> CalculateHashAsync(string filePath);
    Task<string> CalculateHashAsync(Stream fileStream);
    Task<string> CalculateHashAsync(byte[] fileBytes);

    // Hash verification
    Task<bool> VerifyHashAsync(string filePath, string expectedHash);
    Task<bool> VerifyHashAsync(Stream fileStream, string expectedHash);
    Task<bool> VerifyHashAsync(byte[] fileBytes, string expectedHash);

    // Duplicate detection
    Task<List<string>> FindDuplicateFilesAsync(string directoryPath);
    Task<List<string>> FindDuplicateFilesAsync(List<string> filePaths);
    Task<bool> IsDuplicateAsync(string filePath, string directoryPath);
    Task<bool> IsDuplicateAsync(string filePath, List<string> existingHashes);

    // Hash storage
    Task<bool> StoreHashAsync(string filePath, string hash);
    Task<string?> GetStoredHashAsync(string filePath);
    Task<bool> UpdateHashAsync(string filePath, string newHash);
    Task<bool> DeleteHashAsync(string filePath);

    // Hash statistics
    Task<HashStatistics> GetHashStatisticsAsync();
    Task<Dictionary<string, int>> GetHashFrequencyAsync();
    Task<List<string>> GetFilesByHashAsync(string hash);
}

public class HashStatistics
{
    public int TotalFiles { get; set; }
    public int UniqueFiles { get; set; }
    public int DuplicateFiles { get; set; }
    public long TotalSize { get; set; }
    public long UniqueSize { get; set; }
    public long DuplicateSize { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, int> HashFrequency { get; set; } = new();
}

