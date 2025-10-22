namespace Invoice.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    string IssuerName,
    string IssuerStreet,
    string IssuerPostalCode,
    string IssuerCity,
    string? IssuerCountry,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    string SourceFilePath,
    DateTime ImportedAt,
    float ExtractionConfidence,
    string ModelVersion
);

public record InvoiceCreateDto(
    string InvoiceNumber,
    DateOnly InvoiceDate,
    string IssuerName,
    string IssuerStreet,
    string IssuerPostalCode,
    string IssuerCity,
    string? IssuerCountry,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    string SourceFilePath,
    float ExtractionConfidence = 0.0f,
    string ModelVersion = ""
);

public record InvoiceUpdateDto(
    Guid Id,
    string? InvoiceNumber,
    DateOnly? InvoiceDate,
    string? IssuerName,
    string? IssuerStreet,
    string? IssuerPostalCode,
    string? IssuerCity,
    string? IssuerCountry,
    decimal? NetTotal,
    decimal? VatTotal,
    decimal? GrossTotal,
    float? ExtractionConfidence,
    string? ModelVersion
);

public record InvoiceSearchDto(
    string? InvoiceNumber,
    string? IssuerName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? MinAmount,
    decimal? MaxAmount,
    float? MinConfidence,
    string? ModelVersion,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "ImportedAt",
    bool SortDescending = true
);

public record InvoiceSummaryDto(
    int TotalCount,
    decimal TotalAmount,
    decimal AverageAmount,
    DateOnly? EarliestDate,
    DateOnly? LatestDate,
    Dictionary<string, int> IssuerCounts,
    Dictionary<string, int> ModelVersionCounts,
    Dictionary<string, int> ConfidenceLevelCounts
);

