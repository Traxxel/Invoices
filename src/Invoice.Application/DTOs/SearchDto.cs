namespace Invoice.Application.DTOs;

public record SearchRequest(
    string Query,
    List<string> Fields,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = true,
    Dictionary<string, object>? Filters = null
);

public record SearchResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    Dictionary<string, object> Facets,
    TimeSpan SearchTime
);

public record FilterRequest(
    Dictionary<string, object> Filters,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20
);

public record FilterResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    Dictionary<string, List<FilterOption>> AvailableFilters
);

public record FilterOption(
    string Value,
    string Label,
    int Count,
    bool IsSelected
);

public record FacetRequest(
    string Field,
    int MaxValues = 10,
    bool IncludeCounts = true
);

public record FacetResponse(
    string Field,
    List<FacetValue> Values,
    int TotalValues
);

public record FacetValue(
    string Value,
    string Label,
    int Count,
    bool IsSelected
);

