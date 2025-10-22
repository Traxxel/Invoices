using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace Invoice.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IFileSystem _fileSystem;
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IFileSystem fileSystem,
        IOptions<FileStorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _fileSystem = fileSystem;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> StoreFileAsync(string sourcePath, Guid invoiceId)
    {
        try
        {
            if (!_fileSystem.File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }

            var targetPath = GetStoragePath(invoiceId);
            var targetDirectory = _fileSystem.Path.GetDirectoryName(targetPath);

            if (!string.IsNullOrEmpty(targetDirectory))
            {
                _fileSystem.Directory.CreateDirectory(targetDirectory);
            }

            _fileSystem.File.Copy(sourcePath, targetPath, true);

            _logger.LogInformation("File stored successfully: {SourcePath} -> {TargetPath}", sourcePath, targetPath);
            return targetPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store file: {SourcePath}", sourcePath);
            throw;
        }
    }

    public async Task<string> StoreFileAsync(Stream fileStream, string fileName, Guid invoiceId)
    {
        try
        {
            var targetPath = GetStoragePath(invoiceId);
            var targetDirectory = _fileSystem.Path.GetDirectoryName(targetPath);

            if (!string.IsNullOrEmpty(targetDirectory))
            {
                _fileSystem.Directory.CreateDirectory(targetDirectory);
            }

            using var targetFileStream = _fileSystem.File.Create(targetPath);
            await fileStream.CopyToAsync(targetFileStream);

            _logger.LogInformation("File stream stored successfully: {TargetPath}", targetPath);
            return targetPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store file stream for invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }

            _fileSystem.File.Delete(filePath);
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return _fileSystem.File.Exists(filePath);
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return _fileSystem.File.OpenRead(filePath);
    }

    public async Task<byte[]> GetFileBytesAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return await _fileSystem.File.ReadAllBytesAsync(filePath);
    }

    public string GetStoragePath(Guid invoiceId)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month.ToString("D2");

        var basePath = _settings.BasePath;
        var invoiceSubPath = _settings.InvoiceSubPath;

        var directoryPath = _fileSystem.Path.Combine(basePath, invoiceSubPath, year.ToString(), month);
        var fileName = $"{invoiceId}.pdf";

        return _fileSystem.Path.Combine(directoryPath, fileName);
    }

    public string GetRelativePath(string absolutePath)
    {
        var basePath = _fileSystem.Path.GetFullPath(_settings.BasePath);
        var fullPath = _fileSystem.Path.GetFullPath(absolutePath);

        if (fullPath.StartsWith(basePath))
        {
            return fullPath.Substring(basePath.Length).TrimStart(_fileSystem.Path.DirectorySeparatorChar);
        }

        return absolutePath;
    }

    public string GetAbsolutePath(string relativePath)
    {
        return _fileSystem.Path.Combine(_settings.BasePath, relativePath);
    }

    public string GetFileName(Guid invoiceId)
    {
        return $"{invoiceId}.pdf";
    }

    public string GetDirectoryPath(Guid invoiceId)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month.ToString("D2");

        var basePath = _settings.BasePath;
        var invoiceSubPath = _settings.InvoiceSubPath;

        return _fileSystem.Path.Combine(basePath, invoiceSubPath, year.ToString(), month);
    }

    public async Task<FileInformation> GetFileInfoAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return new FileInformation { Exists = false };
        }

        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return new FileInformation
        {
            Name = fileInfo.Name,
            FullPath = fileInfo.FullName,
            Size = fileInfo.Length,
            CreationTime = fileInfo.CreationTime,
            LastWriteTime = fileInfo.LastWriteTime,
            Exists = true
        };
    }

    public async Task<long> GetFileSizeAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return 0;
        }

        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return fileInfo.Length;
    }

    public async Task<DateTime> GetFileCreationTimeAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return DateTime.MinValue;
        }

        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return fileInfo.CreationTime;
    }

    public async Task<DateTime> GetFileModificationTimeAsync(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return DateTime.MinValue;
        }

        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return fileInfo.LastWriteTime;
    }

    public async Task<bool> CreateDirectoryAsync(string path)
    {
        try
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                _fileSystem.Directory.CreateDirectory(path);
                _logger.LogInformation("Directory created: {Path}", path);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {Path}", path);
            return false;
        }
    }

    public async Task<bool> DirectoryExistsAsync(string path)
    {
        return _fileSystem.Directory.Exists(path);
    }

    public async Task<List<string>> GetFilesInDirectoryAsync(string path)
    {
        if (!_fileSystem.Directory.Exists(path))
        {
            return new List<string>();
        }

        return _fileSystem.Directory.GetFiles(path).ToList();
    }

    public async Task<List<string>> GetSubdirectoriesAsync(string path)
    {
        if (!_fileSystem.Directory.Exists(path))
        {
            return new List<string>();
        }

        return _fileSystem.Directory.GetDirectories(path).ToList();
    }

    public async Task<bool> CleanupOldFilesAsync(int retentionDays)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var basePath = _fileSystem.Path.Combine(_settings.BasePath, _settings.InvoiceSubPath);

            if (!_fileSystem.Directory.Exists(basePath))
            {
                return true;
            }

            var deletedCount = 0;
            var directories = _fileSystem.Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories);

            foreach (var directory in directories)
            {
                var files = _fileSystem.Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    var fileInfo = _fileSystem.FileInfo.New(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        _fileSystem.File.Delete(file);
                        deletedCount++;
                    }
                }
            }

            _logger.LogInformation("Cleaned up {Count} old files", deletedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old files");
            return false;
        }
    }

    public async Task<bool> CleanupEmptyDirectoriesAsync()
    {
        try
        {
            var basePath = _fileSystem.Path.Combine(_settings.BasePath, _settings.InvoiceSubPath);

            if (!_fileSystem.Directory.Exists(basePath))
            {
                return true;
            }

            var deletedCount = 0;
            var directories = _fileSystem.Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length);

            foreach (var directory in directories)
            {
                if (_fileSystem.Directory.Exists(directory) &&
                    !_fileSystem.Directory.GetFiles(directory).Any() &&
                    !_fileSystem.Directory.GetDirectories(directory).Any())
                {
                    _fileSystem.Directory.Delete(directory);
                    deletedCount++;
                }
            }

            _logger.LogInformation("Cleaned up {Count} empty directories", deletedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup empty directories");
            return false;
        }
    }

    public async Task<long> GetTotalStorageSizeAsync()
    {
        try
        {
            var basePath = _fileSystem.Path.Combine(_settings.BasePath, _settings.InvoiceSubPath);

            if (!_fileSystem.Directory.Exists(basePath))
            {
                return 0;
            }

            var totalSize = 0L;
            var files = _fileSystem.Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileInfo = _fileSystem.FileInfo.New(file);
                totalSize += fileInfo.Length;
            }

            return totalSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate total storage size");
            return 0;
        }
    }

    public async Task<StorageStatistics> GetStorageStatisticsAsync()
    {
        var statistics = new StorageStatistics();

        try
        {
            var basePath = _fileSystem.Path.Combine(_settings.BasePath, _settings.InvoiceSubPath);

            if (!_fileSystem.Directory.Exists(basePath))
            {
                return statistics;
            }

            var files = _fileSystem.Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
            statistics.FileCount = files.Length;
            statistics.TotalSize = files.Sum(f => _fileSystem.FileInfo.New(f).Length);

            // Group by year
            foreach (var file in files)
            {
                var fileInfo = _fileSystem.FileInfo.New(file);
                var year = fileInfo.CreationTime.Year.ToString();

                if (!statistics.SizeByYear.ContainsKey(year))
                {
                    statistics.SizeByYear[year] = 0;
                    statistics.FileCountByYear[year] = 0;
                }

                statistics.SizeByYear[year] += fileInfo.Length;
                statistics.FileCountByYear[year]++;
            }

            statistics.LastUpdated = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get storage statistics");
        }

        return statistics;
    }
}

