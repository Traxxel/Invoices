using Invoice.Application.DTOs;

namespace Invoice.Application.Interfaces;

public interface IExtractFieldsUseCase
{
    Task<ExtractionResult> ExecuteAsync(ExtractFieldsRequest request);
    Task<ExtractionResult> ExecuteAsync(List<Models.TextLine> textLines);
    Task<ExtractionResult> ExecuteAsync(string filePath);
    Task<ExtractionResult> ExecuteAsync(Stream fileStream, string fileName);

    // Pre-processing
    Task<List<Models.TextLine>> PreprocessTextLinesAsync(List<Models.TextLine> textLines);
    Task<List<Models.TextLine>> NormalizeTextLinesAsync(List<Models.TextLine> textLines);
    Task<List<Models.TextLine>> FilterRelevantLinesAsync(List<Models.TextLine> textLines);

    // Feature extraction
    Task<List<Models.ExtractedFeature>> ExtractFeaturesAsync(List<Models.TextLine> textLines);
    Task<List<Models.ExtractedFeature>> ExtractFeaturesAsync(string filePath);
    Task<List<Models.ExtractedFeature>> ExtractFeaturesAsync(Stream fileStream, string fileName);

    // ML prediction
    Task<List<PredictionResult>> PredictAsync(List<Models.ExtractedFeature> features);
    Task<List<PredictionResult>> PredictAsync(string filePath);
    Task<List<PredictionResult>> PredictAsync(Stream fileStream, string fileName);

    // Post-processing
    Task<ExtractionResult> PostProcessPredictionsAsync(List<PredictionResult> predictions, List<Models.ExtractedFeature> features);
    Task<ExtractionResult> ValidateExtractedFieldsAsync(ExtractionResult extraction);
    Task<ExtractionResult> ApplyBusinessRulesAsync(ExtractionResult extraction);

    // Confidence scoring
    Task<float> CalculateOverallConfidenceAsync(ExtractionResult extraction);
    Task<float> CalculateFieldConfidenceAsync(ExtractedField field);
    Task<List<ConfidenceScore>> CalculateConfidenceScoresAsync(ExtractionResult extraction);

    // Model management
    Task<bool> LoadModelAsync(string modelPath);
    Task<bool> IsModelLoadedAsync();
    Task<string> GetModelVersionAsync();
    Task<ModelInfo> GetModelInfoAsync();
}

public record ExtractFieldsRequest(
    List<Models.TextLine> TextLines,
    ExtractFieldsOptions Options,
    string? ModelVersion = null,
    string? UserId = null,
    string? SessionId = null
);

public record ExtractFieldsOptions(
    bool UseMLExtraction,
    bool UseRegexValidation,
    bool UsePostProcessing,
    bool UseBusinessRules,
    float ConfidenceThreshold,
    bool RequireManualReview,
    List<string> RequiredFields,
    List<string> OptionalFields,
    Dictionary<string, object> CustomSettings
);

public record ConfidenceScore(
    string FieldType,
    float Score,
    string Source,
    DateTime CalculatedAt
);

