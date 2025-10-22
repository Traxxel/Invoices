namespace Invoice.Application.DTOs;

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string Message,
    List<string> Errors,
    DateTime Timestamp
);

public record ApiResponse(
    bool Success,
    string Message,
    List<string> Errors,
    DateTime Timestamp
);

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

public record ErrorResponse(
    string Code,
    string Message,
    string? Details,
    string? StackTrace,
    DateTime Timestamp
);

public record SuccessResponse(
    string Message,
    object? Data,
    DateTime Timestamp
);

public record StatusResponse(
    string Status,
    string Message,
    DateTime Timestamp,
    Dictionary<string, object> Details
);

public record HealthCheckResponse(
    string Status,
    DateTime Timestamp,
    Dictionary<string, HealthCheck> Checks
);

public record HealthCheck(
    string Name,
    string Status,
    string Message,
    TimeSpan Duration,
    Dictionary<string, object> Details
);

public record ConfigurationResponse(
    Dictionary<string, object> Settings,
    DateTime LastUpdated,
    string Version
);

public record LogEntry(
    DateTime Timestamp,
    string Level,
    string Message,
    string? Exception,
    Dictionary<string, object> Properties
);

public record AuditEntry(
    DateTime Timestamp,
    string User,
    string Action,
    string Entity,
    string EntityId,
    Dictionary<string, object> Changes
);

