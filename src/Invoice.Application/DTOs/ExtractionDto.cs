namespace Invoice.Application.DTOs;

public record ExtractionResult(
    bool Success,
    string Message,
    List<ExtractedField> Fields,
    List<ExtractionWarning> Warnings,
    List<ExtractionError> Errors,
    float OverallConfidence,
    string ModelVersion,
    DateTime ExtractedAt,
    TimeSpan ExtractionTime
);

public record ExtractedField(
    string FieldType,
    string Value,
    float Confidence,
    string OriginalText,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    List<AlternativeValue> Alternatives
);

public record AlternativeValue(
    string Value,
    float Confidence,
    string Source
);

public record ExtractionWarning(
    string Code,
    string Message,
    string FieldType,
    string Value,
    int LineIndex
);

public record ExtractionError(
    string Code,
    string Message,
    string FieldType,
    string Value,
    Exception? Exception
);

public record FieldExtractionRequest(
    string Text,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string Context
);

public record FieldExtractionResponse(
    string FieldType,
    string Value,
    float Confidence,
    List<AlternativeValue> Alternatives,
    bool IsHighConfidence,
    bool IsLowConfidence,
    string Recommendation
);

public record BatchExtractionRequest(
    List<FieldExtractionRequest> Fields,
    string DocumentId,
    string ModelVersion
);

public record BatchExtractionResponse(
    List<FieldExtractionResponse> Fields,
    float OverallConfidence,
    int HighConfidenceCount,
    int LowConfidenceCount,
    List<ExtractionWarning> Warnings,
    List<ExtractionError> Errors,
    TimeSpan ProcessingTime
);

