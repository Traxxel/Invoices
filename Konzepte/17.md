# Aufgabe 17: File Hash und Integrity Check

## Ziel

File Hash Service für Integritätsprüfung und Duplikat-Erkennung von PDF-Dateien.

## 1. File Hash Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IFileHashService.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

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
```

## 2. File Hash Implementation

**Datei:** `src/InvoiceReader.Infrastructure/Services/FileHashService.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Security.Cryptography;

namespace InvoiceReader.Infrastructure.Services;

public class FileHashService : IFileHashService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<FileHashService> _logger;
    private readonly Dictionary<string, string> _hashCache = new();

    public FileHashService(IFileSystem fileSystem, ILogger<FileHashService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<string> CalculateHashAsync(string filePath)
    {
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Check cache first
            if (_hashCache.ContainsKey(filePath))
            {
                return _hashCache[filePath];
            }

            using var fileStream = _fileSystem.File.OpenRead(filePath);
            var hash = await CalculateHashAsync(fileStream);

            // Cache the result
            _hashCache[filePath] = hash;

            _logger.LogDebug("Hash calculated for file: {FilePath} -> {Hash}", filePath, hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate hash for file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<string> CalculateHashAsync(Stream fileStream)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            var hash = Convert.ToHexString(hashBytes);

            _logger.LogDebug("Hash calculated for stream: {Hash}", hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate hash for stream");
            throw;
        }
    }

    public async Task<string> CalculateHashAsync(byte[] fileBytes)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(fileBytes);
            var hash = Convert.ToHexString(hashBytes);

            _logger.LogDebug("Hash calculated for bytes: {Hash}", hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate hash for bytes");
            throw;
        }
    }

    public async Task<bool> VerifyHashAsync(string filePath, string expectedHash)
    {
        try
        {
            var actualHash = await CalculateHashAsync(filePath);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug("Hash verification for file: {FilePath} -> {IsValid}", filePath, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify hash for file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> VerifyHashAsync(Stream fileStream, string expectedHash)
    {
        try
        {
            var actualHash = await CalculateHashAsync(fileStream);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug("Hash verification for stream -> {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify hash for stream");
            return false;
        }
    }

    public async Task<bool> VerifyHashAsync(byte[] fileBytes, string expectedHash)
    {
        try
        {
            var actualHash = await CalculateHashAsync(fileBytes);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug("Hash verification for bytes -> {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify hash for bytes");
            return false;
        }
    }

    public async Task<List<string>> FindDuplicateFilesAsync(string directoryPath)
    {
        try
        {
            if (!_fileSystem.Directory.Exists(directoryPath))
            {
                return new List<string>();
            }

            var files = _fileSystem.Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            return await FindDuplicateFilesAsync(files.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find duplicate files in directory: {DirectoryPath}", directoryPath);
            return new List<string>();
        }
    }

    public async Task<List<string>> FindDuplicateFilesAsync(List<string> filePaths)
    {
        try
        {
            var hashGroups = new Dictionary<string, List<string>>();

            foreach (var filePath in filePaths)
            {
                if (!_fileSystem.File.Exists(filePath))
                {
                    continue;
                }

                var hash = await CalculateHashAsync(filePath);

                if (!hashGroups.ContainsKey(hash))
                {
                    hashGroups[hash] = new List<string>();
                }

                hashGroups[hash].Add(filePath);
            }

            var duplicateFiles = new List<string>();
            foreach (var group in hashGroups.Values)
            {
                if (group.Count > 1)
                {
                    duplicateFiles.AddRange(group);
                }
            }

            _logger.LogInformation("Found {Count} duplicate files", duplicateFiles.Count);
            return duplicateFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find duplicate files");
            return new List<string>();
        }
    }

    public async Task<bool> IsDuplicateAsync(string filePath, string directoryPath)
    {
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                return false;
            }

            var fileHash = await CalculateHashAsync(filePath);
            var existingFiles = _fileSystem.Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var existingFile in existingFiles)
            {
                if (existingFile == filePath)
                {
                    continue;
                }

                var existingHash = await CalculateHashAsync(existingFile);
                if (string.Equals(fileHash, existingHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Duplicate file found: {FilePath} matches {ExistingFile}", filePath, existingFile);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for duplicate file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> IsDuplicateAsync(string filePath, List<string> existingHashes)
    {
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                return false;
            }

            var fileHash = await CalculateHashAsync(filePath);
            return existingHashes.Any(h => string.Equals(fileHash, h, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for duplicate file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> StoreHashAsync(string filePath, string hash)
    {
        try
        {
            var hashFilePath = GetHashFilePath(filePath);
            var hashDirectory = _fileSystem.Path.GetDirectoryName(hashFilePath);

            if (!string.IsNullOrEmpty(hashDirectory))
            {
                _fileSystem.Directory.CreateDirectory(hashDirectory);
            }

            await _fileSystem.File.WriteAllTextAsync(hashFilePath, hash);

            _logger.LogDebug("Hash stored for file: {FilePath} -> {Hash}", filePath, hash);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store hash for file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string?> GetStoredHashAsync(string filePath)
    {
        try
        {
            var hashFilePath = GetHashFilePath(filePath);

            if (!_fileSystem.File.Exists(hashFilePath))
            {
                return null;
            }

            var hash = await _fileSystem.File.ReadAllTextAsync(hashFilePath);
            return hash.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stored hash for file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<bool> UpdateHashAsync(string filePath, string newHash)
    {
        try
        {
            var hashFilePath = GetHashFilePath(filePath);
            var hashDirectory = _fileSystem.Path.GetDirectoryName(hashFilePath);

            if (!string.IsNullOrEmpty(hashDirectory))
            {
                _fileSystem.Directory.CreateDirectory(hashDirectory);
            }

            await _fileSystem.File.WriteAllTextAsync(hashFilePath, newHash);

            _logger.LogDebug("Hash updated for file: {FilePath} -> {Hash}", filePath, newHash);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update hash for file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> DeleteHashAsync(string filePath)
    {
        try
        {
            var hashFilePath = GetHashFilePath(filePath);

            if (_fileSystem.File.Exists(hashFilePath))
            {
                _fileSystem.File.Delete(hashFilePath);
                _logger.LogDebug("Hash deleted for file: {FilePath}", filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete hash for file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<HashStatistics> GetHashStatisticsAsync()
    {
        var statistics = new HashStatistics();

        try
        {
            var hashFiles = _fileSystem.Directory.GetFiles(".hashes", "*.hash", SearchOption.AllDirectories);
            var hashGroups = new Dictionary<string, List<string>>();

            foreach (var hashFile in hashFiles)
            {
                var originalFile = GetOriginalFilePathFromHashFile(hashFile);
                var hash = await _fileSystem.File.ReadAllTextAsync(hashFile);

                if (!hashGroups.ContainsKey(hash))
                {
                    hashGroups[hash] = new List<string>();
                }

                hashGroups[hash].Add(originalFile);
            }

            statistics.TotalFiles = hashGroups.Values.Sum(g => g.Count);
            statistics.UniqueFiles = hashGroups.Count;
            statistics.DuplicateFiles = statistics.TotalFiles - statistics.UniqueFiles;

            foreach (var group in hashGroups.Values)
            {
                var fileSize = 0L;
                foreach (var file in group)
                {
                    if (_fileSystem.File.Exists(file))
                    {
                        fileSize += _fileSystem.FileInfo.New(file).Length;
                    }
                }

                if (group.Count > 1)
                {
                    statistics.DuplicateSize += fileSize;
                }
                else
                {
                    statistics.UniqueSize += fileSize;
                }

                statistics.TotalSize += fileSize;
            }

            statistics.LastUpdated = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hash statistics");
        }

        return statistics;
    }

    public async Task<Dictionary<string, int>> GetHashFrequencyAsync()
    {
        try
        {
            var hashFiles = _fileSystem.Directory.GetFiles(".hashes", "*.hash", SearchOption.AllDirectories);
            var frequency = new Dictionary<string, int>();

            foreach (var hashFile in hashFiles)
            {
                var hash = await _fileSystem.File.ReadAllTextAsync(hashFile);
                var trimmedHash = hash.Trim();

                if (frequency.ContainsKey(trimmedHash))
                {
                    frequency[trimmedHash]++;
                }
                else
                {
                    frequency[trimmedHash] = 1;
                }
            }

            return frequency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hash frequency");
            return new Dictionary<string, int>();
        }
    }

    public async Task<List<string>> GetFilesByHashAsync(string hash)
    {
        try
        {
            var hashFiles = _fileSystem.Directory.GetFiles(".hashes", "*.hash", SearchOption.AllDirectories);
            var matchingFiles = new List<string>();

            foreach (var hashFile in hashFiles)
            {
                var storedHash = await _fileSystem.File.ReadAllTextAsync(hashFile);
                if (string.Equals(storedHash.Trim(), hash, StringComparison.OrdinalIgnoreCase))
                {
                    var originalFile = GetOriginalFilePathFromHashFile(hashFile);
                    matchingFiles.Add(originalFile);
                }
            }

            return matchingFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get files by hash: {Hash}", hash);
            return new List<string>();
        }
    }

    private string GetHashFilePath(string originalFilePath)
    {
        var relativePath = _fileSystem.Path.GetRelativePath(".", originalFilePath);
        var hashFileName = _fileSystem.Path.ChangeExtension(relativePath, ".hash");
        return _fileSystem.Path.Combine(".hashes", hashFileName);
    }

    private string GetOriginalFilePathFromHashFile(string hashFilePath)
    {
        var relativePath = _fileSystem.Path.GetRelativePath(".hashes", hashFilePath);
        var originalPath = _fileSystem.Path.ChangeExtension(relativePath, null);
        return _fileSystem.Path.GetFullPath(originalPath);
    }
}
```

## 3. File Hash Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/FileHashExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class FileHashExtensions
{
    public static IServiceCollection AddFileHashServices(this IServiceCollection services)
    {
        services.AddScoped<IFileHashService, FileHashService>();

        return services;
    }
}
```

## Wichtige Hinweise

- SHA256-Hash für Integritätsprüfung
- Hash-Caching für Performance
- Duplikat-Erkennung über Hash-Vergleich
- Hash-Speicherung für Persistenz
- Statistics für Hash-Monitoring
- Error Handling für alle Hash-Operationen
- Logging für alle Hash-Operationen
- Stream-basierte Hash-Berechnung
- File System Abstraktion für Testbarkeit
- Hash-Frequency für Duplikat-Analyse
