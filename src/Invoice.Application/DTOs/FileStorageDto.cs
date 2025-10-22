namespace Invoice.Application.DTOs;

public record FileStorageRequest(
    string SourcePath,
    Guid InvoiceId,
    string FileName,
    long FileSize,
    string FileHash
);

public record FileStorageResponse(
    bool Success,
    string StoredPath,
    string RelativePath,
    string FileName,
    long FileSize,
    string FileHash,
    DateTime StoredAt,
    string Message
);

public record FileInfo(
    string Name,
    string FullPath,
    long Size,
    DateTime CreationTime,
    DateTime LastWriteTime,
    bool Exists
);

public record StorageStatistics(
    long TotalSize,
    int FileCount,
    int DirectoryCount,
    DateTime LastUpdated,
    Dictionary<string, long> SizeByYear,
    Dictionary<string, int> FileCountByYear
);

public record FileSearchRequest(
    string? FileName,
    string? Extension,
    DateTime? StartDate,
    DateTime? EndDate,
    long? MinSize,
    long? MaxSize,
    string? Directory
);

public record FileSearchResponse(
    List<FileInfo> Files,
    int TotalCount,
    long TotalSize,
    Dictionary<string, int> CountByExtension,
    Dictionary<string, long> SizeByExtension
);

public record FileCleanupRequest(
    int RetentionDays,
    bool DeleteEmptyDirectories,
    bool DryRun
);

public record FileCleanupResponse(
    bool Success,
    int FilesDeleted,
    int DirectoriesDeleted,
    long SpaceFreed,
    List<string> DeletedFiles,
    List<string> Errors
);

