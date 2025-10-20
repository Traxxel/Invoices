# Aufgabe 28: IExtractFieldsUseCase und Post-Processing

## Ziel

Extract Fields Use Case für ML-basierte Feld-Extraktion mit Post-Processing und Regex-Validierung.

## 1. Extract Fields Use Case Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IExtractFieldsUseCase.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

public interface IExtractFieldsUseCase
{
    Task<ExtractionResult> ExecuteAsync(ExtractFieldsRequest request);
    Task<ExtractionResult> ExecuteAsync(List<TextLine> textLines);
    Task<ExtractionResult> ExecuteAsync(string filePath);
    Task<ExtractionResult> ExecuteAsync(Stream fileStream, string fileName);

    // Pre-processing
    Task<List<TextLine>> PreprocessTextLinesAsync(List<TextLine> textLines);
    Task<List<TextLine>> NormalizeTextLinesAsync(List<TextLine> textLines);
    Task<List<TextLine>> FilterRelevantLinesAsync(List<TextLine> textLines);

    // Feature extraction
    Task<List<ExtractedFeature>> ExtractFeaturesAsync(List<TextLine> textLines);
    Task<List<ExtractedFeature>> ExtractFeaturesAsync(string filePath);
    Task<List<ExtractedFeature>> ExtractFeaturesAsync(Stream fileStream, string fileName);

    // ML prediction
    Task<List<PredictionResult>> PredictAsync(List<ExtractedFeature> features);
    Task<List<PredictionResult>> PredictAsync(string filePath);
    Task<List<PredictionResult>> PredictAsync(Stream fileStream, string fileName);

    // Post-processing
    Task<ExtractionResult> PostProcessPredictionsAsync(List<PredictionResult> predictions, List<ExtractedFeature> features);
    Task<ExtractionResult> ValidateExtractedFieldsAsync(ExtractionResult extraction);
    Task<ExtractionResult> ApplyBusinessRulesAsync(ExtractionResult extraction);

    // Regex validation
    Task<RegexValidationResult> ValidateWithRegexAsync(ExtractionResult extraction);
    Task<RegexValidationResult> ValidateFieldWithRegexAsync(string fieldType, string value);

    // Confidence scoring
    Task<float> CalculateOverallConfidenceAsync(ExtractionResult extraction);
    Task<float> CalculateFieldConfidenceAsync(ExtractedField field);
    Task<List<ConfidenceScore>> CalculateConfidenceScoresAsync(ExtractionResult extraction);

    // Model management
    Task<bool> LoadModelAsync(string modelPath);
    Task<bool> IsModelLoadedAsync();
    Task<string> GetModelVersionAsync();
    Task<ModelInfo> GetModelInfoAsync();

    // Training data
    Task<TrainingDataResult> GenerateTrainingDataAsync(ExtractionResult extraction, List<TextLine> textLines);
    Task<TrainingDataResult> GenerateTrainingDataAsync(string filePath, ExtractionResult extraction);
    Task<bool> SaveTrainingDataAsync(TrainingDataResult trainingData, string outputPath);
}

public record ExtractFieldsRequest(
    List<TextLine> TextLines,
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

public record TextLine(
    string Text,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    float FontSize,
    string FontName,
    bool IsBold,
    bool IsItalic,
    string Alignment,
    List<TextWord> Words
);

public record TextWord(
    string Text,
    float X,
    float Y,
    float Width,
    float Height,
    float FontSize,
    string FontName,
    bool IsBold,
    bool IsItalic
);

public record ExtractedFeature(
    string Text,
    int LineIndex,
    int PageNumber,
    Position Position,
    float FontSize,
    string FontName,
    bool IsBold,
    bool IsItalic,
    string Alignment,
    List<string> RegexHits,
    List<string> Keywords,
    Dictionary<string, object> CustomFeatures
);

public record Position(
    float X,
    float Y,
    float Width,
    float Height
);

public record PredictionResult(
    string PredictedLabel,
    float Confidence,
    ExtractedFeature SourceFeature,
    Dictionary<string, float> ClassScores,
    bool IsLowConfidence,
    int LineIndex,
    string AlternativeLabel,
    float AlternativeConfidence
);

public record ClassScore(
    string ClassName,
    float Score,
    string Source
);

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
    string SourceText,
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

public record RegexValidationResult(
    bool IsValid,
    string Message,
    string FieldType,
    string Value,
    List<RegexMatch> Matches,
    List<RegexWarning> Warnings,
    List<RegexError> Errors
);

public record RegexMatch(
    string Pattern,
    string Match,
    int StartIndex,
    int Length,
    float Confidence
);

public record RegexWarning(
    string Code,
    string Message,
    string FieldType,
    string Value,
    string Suggestion
);

public record RegexError(
    string Code,
    string Message,
    string FieldType,
    string Value,
    Exception? Exception
);

public record ConfidenceScore(
    string FieldType,
    float Score,
    string Source,
    DateTime CalculatedAt
);

public record ModelInfo(
    string Version,
    string Path,
    DateTime CreatedAt,
    DateTime LastModified,
    long Size,
    Dictionary<string, object> Metadata
);

public record TrainingDataResult(
    bool Success,
    string Message,
    List<TrainingDataRow> Rows,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    List<TrainingDataWarning> Warnings,
    List<TrainingDataError> Errors
);

public record TrainingDataRow(
    string Text,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string Label,
    float Confidence,
    Dictionary<string, object> Features
);

public record TrainingDataWarning(
    string Code,
    string Message,
    string FieldType,
    string Value,
    string Suggestion
);

public record TrainingDataError(
    string Code,
    string Message,
    string FieldType,
    string Value,
    Exception? Exception
);
```

## 2. Extract Fields Use Case Implementation

**Datei:** `src/InvoiceReader.Application/UseCases/ExtractFieldsUseCase.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.Application.UseCases;

public class ExtractFieldsUseCase : IExtractFieldsUseCase
{
    private readonly IPdfParserService _pdfParserService;
    private readonly ITextNormalizationService _textNormalizationService;
    private readonly IFeatureExtractionService _featureExtractionService;
    private readonly IPredictionEngineService _predictionEngineService;
    private readonly IRegexValidationService _regexValidationService;
    private readonly IBusinessRulesService _businessRulesService;
    private readonly ILogger<ExtractFieldsUseCase> _logger;

    public ExtractFieldsUseCase(
        IPdfParserService pdfParserService,
        ITextNormalizationService textNormalizationService,
        IFeatureExtractionService featureExtractionService,
        IPredictionEngineService predictionEngineService,
        IRegexValidationService regexValidationService,
        IBusinessRulesService businessRulesService,
        ILogger<ExtractFieldsUseCase> logger)
    {
        _pdfParserService = pdfParserService;
        _textNormalizationService = textNormalizationService;
        _featureExtractionService = featureExtractionService;
        _predictionEngineService = predictionEngineService;
        _regexValidationService = regexValidationService;
        _businessRulesService = businessRulesService;
        _logger = logger;
    }

    public async Task<ExtractionResult> ExecuteAsync(ExtractFieldsRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<ExtractionWarning>();
        var errors = new List<ExtractionError>();

        try
        {
            _logger.LogInformation("Starting field extraction: {LineCount} lines", request.TextLines.Count);

            // Preprocess text lines
            var preprocessedLines = await PreprocessTextLinesAsync(request.TextLines);
            _logger.LogInformation("Preprocessed {Count} lines", preprocessedLines.Count);

            // Extract features
            var features = await ExtractFeaturesAsync(preprocessedLines);
            _logger.LogInformation("Extracted {Count} features", features.Count);

            // Make predictions
            var predictions = await PredictAsync(features);
            _logger.LogInformation("Generated {Count} predictions", predictions.Count);

            // Post-process predictions
            var extraction = await PostProcessPredictionsAsync(predictions, features);
            _logger.LogInformation("Post-processed extraction with {Count} fields", extraction.Fields.Count);

            // Apply business rules if enabled
            if (request.Options.UseBusinessRules)
            {
                extraction = await ApplyBusinessRulesAsync(extraction);
                _logger.LogInformation("Applied business rules");
            }

            // Validate with regex if enabled
            if (request.Options.UseRegexValidation)
            {
                var regexValidation = await ValidateWithRegexAsync(extraction);
                if (!regexValidation.IsValid)
                {
                    warnings.Add(new ExtractionWarning(
                        "REGEX_VALIDATION_FAILED",
                        "Regex validation failed",
                        "Extraction",
                        "Multiple fields",
                        0
                    ));
                }
            }

            // Calculate overall confidence
            var overallConfidence = await CalculateOverallConfidenceAsync(extraction);
            extraction = extraction with { OverallConfidence = overallConfidence };

            _logger.LogInformation("Field extraction completed: {FieldCount} fields, {Confidence:F2} confidence",
                extraction.Fields.Count, overallConfidence);

            return extraction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Field extraction failed");

            return new ExtractionResult(
                false,
                "Field extraction failed",
                new List<ExtractedField>(),
                warnings,
                new List<ExtractionError> { new ExtractionError("EXTRACTION_FAILED", ex.Message, "Extraction", "Multiple fields", ex) },
                0f,
                "v1.0",
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<ExtractionResult> ExecuteAsync(List<TextLine> textLines)
    {
        var options = new ExtractFieldsOptions(
            UseMLExtraction: true,
            UseRegexValidation: true,
            UsePostProcessing: true,
            UseBusinessRules: true,
            ConfidenceThreshold: 0.7f,
            RequireManualReview: false,
            RequiredFields: new List<string> { "InvoiceNumber", "InvoiceDate", "GrossTotal" },
            OptionalFields: new List<string> { "IssuerName", "IssuerAddress", "NetTotal", "VatTotal" },
            CustomSettings: new Dictionary<string, object>()
        );

        var request = new ExtractFieldsRequest(textLines, options);
        return await ExecuteAsync(request);
    }

    public async Task<ExtractionResult> ExecuteAsync(string filePath)
    {
        try
        {
            var textLines = await _pdfParserService.ExtractTextLinesAsync(filePath);
            return await ExecuteAsync(textLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract fields from file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<ExtractionResult> ExecuteAsync(Stream fileStream, string fileName)
    {
        try
        {
            var textLines = await _pdfParserService.ExtractTextLinesAsync(fileStream);
            return await ExecuteAsync(textLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract fields from stream: {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<TextLine>> PreprocessTextLinesAsync(List<TextLine> textLines)
    {
        try
        {
            _logger.LogInformation("Preprocessing {Count} text lines", textLines.Count);

            // Normalize text
            var normalizedLines = await NormalizeTextLinesAsync(textLines);

            // Filter relevant lines
            var relevantLines = await FilterRelevantLinesAsync(normalizedLines);

            _logger.LogInformation("Preprocessed {Count} lines to {RelevantCount} relevant lines",
                textLines.Count, relevantLines.Count);

            return relevantLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preprocess text lines");
            return textLines; // Return original lines if preprocessing fails
        }
    }

    public async Task<List<TextLine>> NormalizeTextLinesAsync(List<TextLine> textLines)
    {
        try
        {
            var normalizedLines = new List<TextLine>();

            foreach (var line in textLines)
            {
                var normalizedText = await _textNormalizationService.NormalizeTextAsync(line.Text);
                var normalizedWords = new List<TextWord>();

                foreach (var word in line.Words)
                {
                    var normalizedWordText = await _textNormalizationService.NormalizeTextAsync(word.Text);
                    normalizedWords.Add(word with { Text = normalizedWordText });
                }

                normalizedLines.Add(line with {
                    Text = normalizedText,
                    Words = normalizedWords
                });
            }

            return normalizedLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize text lines");
            return textLines; // Return original lines if normalization fails
        }
    }

    public async Task<List<TextLine>> FilterRelevantLinesAsync(List<TextLine> textLines)
    {
        try
        {
            var relevantLines = new List<TextLine>();

            foreach (var line in textLines)
            {
                // Filter out empty lines
                if (string.IsNullOrWhiteSpace(line.Text))
                    continue;

                // Filter out very short lines (likely noise)
                if (line.Text.Length < 3)
                    continue;

                // Filter out lines that are likely headers/footers
                if (IsHeaderOrFooter(line))
                    continue;

                // Filter out lines that are likely page numbers
                if (IsPageNumber(line))
                    continue;

                relevantLines.Add(line);
            }

            return relevantLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to filter relevant lines");
            return textLines; // Return original lines if filtering fails
        }
    }

    public async Task<List<ExtractedFeature>> ExtractFeaturesAsync(List<TextLine> textLines)
    {
        try
        {
            _logger.LogInformation("Extracting features from {Count} text lines", textLines.Count);

            var features = new List<ExtractedFeature>();

            for (int i = 0; i < textLines.Count; i++)
            {
                var line = textLines[i];
                var feature = await _featureExtractionService.ExtractFeatureAsync(line, i);
                features.Add(feature);
            }

            _logger.LogInformation("Extracted {Count} features", features.Count);
            return features;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract features");
            return new List<ExtractedFeature>();
        }
    }

    public async Task<List<ExtractedFeature>> ExtractFeaturesAsync(string filePath)
    {
        try
        {
            var textLines = await _pdfParserService.ExtractTextLinesAsync(filePath);
            return await ExtractFeaturesAsync(textLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract features from file: {FilePath}", filePath);
            return new List<ExtractedFeature>();
        }
    }

    public async Task<List<ExtractedFeature>> ExtractFeaturesAsync(Stream fileStream, string fileName)
    {
        try
        {
            var textLines = await _pdfParserService.ExtractTextLinesAsync(fileStream);
            return await ExtractFeaturesAsync(textLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract features from stream: {FileName}", fileName);
            return new List<ExtractedFeature>();
        }
    }

    public async Task<List<PredictionResult>> PredictAsync(List<ExtractedFeature> features)
    {
        try
        {
            _logger.LogInformation("Making predictions for {Count} features", features.Count);

            var predictions = new List<PredictionResult>();

            foreach (var feature in features)
            {
                var prediction = await _predictionEngineService.PredictAsync(feature);
                predictions.Add(prediction);
            }

            _logger.LogInformation("Generated {Count} predictions", predictions.Count);
            return predictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make predictions");
            return new List<PredictionResult>();
        }
    }

    public async Task<List<PredictionResult>> PredictAsync(string filePath)
    {
        try
        {
            var features = await ExtractFeaturesAsync(filePath);
            return await PredictAsync(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict from file: {FilePath}", filePath);
            return new List<PredictionResult>();
        }
    }

    public async Task<List<PredictionResult>> PredictAsync(Stream fileStream, string fileName)
    {
        try
        {
            var features = await ExtractFeaturesAsync(fileStream, fileName);
            return await PredictAsync(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict from stream: {FileName}", fileName);
            return new List<PredictionResult>();
        }
    }

    public async Task<ExtractionResult> PostProcessPredictionsAsync(List<PredictionResult> predictions, List<ExtractedFeature> features)
    {
        try
        {
            _logger.LogInformation("Post-processing {Count} predictions", predictions.Count);

            var fields = new List<ExtractedField>();
            var warnings = new List<ExtractionWarning>();
            var errors = new List<ExtractionError>();

            // Group predictions by field type
            var fieldGroups = predictions
                .Where(p => p.PredictedLabel != "None")
                .GroupBy(p => p.PredictedLabel)
                .ToList();

            foreach (var group in fieldGroups)
            {
                var fieldType = group.Key;
                var bestPrediction = group.OrderByDescending(p => p.Confidence).First();

                // Create alternatives from other predictions
                var alternatives = group
                    .OrderByDescending(p => p.Confidence)
                    .Skip(1)
                    .Take(3)
                    .Select(p => new AlternativeValue(p.PredictedLabel, p.Confidence, "ML"))
                    .ToList();

                var field = new ExtractedField(
                    fieldType,
                    bestPrediction.SourceFeature.Text,
                    bestPrediction.Confidence,
                    bestPrediction.SourceFeature.Text,
                    bestPrediction.LineIndex,
                    bestPrediction.SourceFeature.PageNumber,
                    bestPrediction.SourceFeature.Position.X,
                    bestPrediction.SourceFeature.Position.Y,
                    bestPrediction.SourceFeature.Position.Width,
                    bestPrediction.SourceFeature.Position.Height,
                    alternatives
                );

                fields.Add(field);

                // Add warnings for low confidence predictions
                if (bestPrediction.IsLowConfidence)
                {
                    warnings.Add(new ExtractionWarning(
                        "LOW_CONFIDENCE",
                        $"Low confidence prediction for {fieldType}",
                        fieldType,
                        bestPrediction.SourceFeature.Text,
                        bestPrediction.LineIndex
                    ));
                }
            }

            var overallConfidence = fields.Any() ? fields.Average(f => f.Confidence) : 0f;

            _logger.LogInformation("Post-processed {Count} fields with {Confidence:F2} overall confidence",
                fields.Count, overallConfidence);

            return new ExtractionResult(
                true,
                "Field extraction completed successfully",
                fields,
                warnings,
                errors,
                overallConfidence,
                "v1.0",
                DateTime.UtcNow,
                TimeSpan.Zero
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post-process predictions");

            return new ExtractionResult(
                false,
                "Post-processing failed",
                new List<ExtractedField>(),
                new List<ExtractionWarning>(),
                new List<ExtractionError> { new ExtractionError("POST_PROCESSING_FAILED", ex.Message, "Extraction", "Multiple fields", ex) },
                0f,
                "v1.0",
                DateTime.UtcNow,
                TimeSpan.Zero
            );
        }
    }

    public async Task<ExtractionResult> ValidateExtractedFieldsAsync(ExtractionResult extraction)
    {
        try
        {
            _logger.LogInformation("Validating {Count} extracted fields", extraction.Fields.Count);

            var warnings = new List<ExtractionWarning>();
            var errors = new List<ExtractionError>();

            foreach (var field in extraction.Fields)
            {
                // Validate field value
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    errors.Add(new ExtractionError(
                        "EMPTY_FIELD",
                        "Field value is empty",
                        field.FieldType,
                        field.Value,
                        null
                    ));
                    continue;
                }

                // Validate field type
                if (!IsValidFieldType(field.FieldType))
                {
                    errors.Add(new ExtractionError(
                        "INVALID_FIELD_TYPE",
                        "Invalid field type",
                        field.FieldType,
                        field.Value,
                        null
                    ));
                    continue;
                }

                // Validate field format
                var formatValidation = await ValidateFieldFormatAsync(field);
                if (!formatValidation.IsValid)
                {
                    warnings.Add(new ExtractionWarning(
                        "INVALID_FORMAT",
                        "Field format is invalid",
                        field.FieldType,
                        field.Value,
                        field.LineIndex
                    ));
                }
            }

            return extraction with {
                Warnings = extraction.Warnings.Concat(warnings).ToList(),
                Errors = extraction.Errors.Concat(errors).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate extracted fields");
            return extraction;
        }
    }

    public async Task<ExtractionResult> ApplyBusinessRulesAsync(ExtractionResult extraction)
    {
        try
        {
            _logger.LogInformation("Applying business rules to {Count} fields", extraction.Fields.Count);

            var rules = await _businessRulesService.GetBusinessRulesAsync();
            var warnings = new List<ExtractionWarning>();
            var errors = new List<ExtractionError>();

            foreach (var rule in rules)
            {
                var ruleResult = await _businessRulesService.ApplyRuleAsync(rule, extraction);
                if (!ruleResult.IsValid)
                {
                    if (ruleResult.IsError)
                    {
                        errors.Add(new ExtractionError(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.FieldType,
                            ruleResult.Value,
                            null
                        ));
                    }
                    else
                    {
                        warnings.Add(new ExtractionWarning(
                            ruleResult.Code,
                            ruleResult.Message,
                            ruleResult.FieldType,
                            ruleResult.Value,
                            ruleResult.LineIndex
                        ));
                    }
                }
            }

            return extraction with {
                Warnings = extraction.Warnings.Concat(warnings).ToList(),
                Errors = extraction.Errors.Concat(errors).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply business rules");
            return extraction;
        }
    }

    public async Task<RegexValidationResult> ValidateWithRegexAsync(ExtractionResult extraction)
    {
        try
        {
            _logger.LogInformation("Validating {Count} fields with regex", extraction.Fields.Count);

            var warnings = new List<RegexWarning>();
            var errors = new List<RegexError>();
            var isValid = true;

            foreach (var field in extraction.Fields)
            {
                var fieldValidation = await ValidateFieldWithRegexAsync(field.FieldType, field.Value);
                if (!fieldValidation.IsValid)
                {
                    isValid = false;
                    warnings.AddRange(fieldValidation.Warnings);
                    errors.AddRange(fieldValidation.Errors);
                }
            }

            return new RegexValidationResult(
                isValid,
                isValid ? "All fields passed regex validation" : "Some fields failed regex validation",
                "Multiple",
                "Multiple",
                new List<RegexMatch>(),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate with regex");
            return new RegexValidationResult(
                false,
                "Regex validation failed",
                "Multiple",
                "Multiple",
                new List<RegexMatch>(),
                new List<RegexWarning>(),
                new List<RegexError> { new RegexError("REGEX_VALIDATION_FAILED", ex.Message, "Multiple", "Multiple", ex) }
            );
        }
    }

    public async Task<RegexValidationResult> ValidateFieldWithRegexAsync(string fieldType, string value)
    {
        try
        {
            return await _regexValidationService.ValidateFieldAsync(fieldType, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate field with regex: {FieldType}", fieldType);
            return new RegexValidationResult(
                false,
                "Field validation failed",
                fieldType,
                value,
                new List<RegexMatch>(),
                new List<RegexWarning>(),
                new List<RegexError> { new RegexError("FIELD_VALIDATION_FAILED", ex.Message, fieldType, value, ex) }
            );
        }
    }

    public async Task<float> CalculateOverallConfidenceAsync(ExtractionResult extraction)
    {
        try
        {
            if (!extraction.Fields.Any())
                return 0f;

            var totalConfidence = extraction.Fields.Sum(f => f.Confidence);
            var averageConfidence = totalConfidence / extraction.Fields.Count;

            // Apply confidence adjustments based on warnings and errors
            var warningPenalty = extraction.Warnings.Count * 0.05f;
            var errorPenalty = extraction.Errors.Count * 0.1f;

            var adjustedConfidence = Math.Max(0f, averageConfidence - warningPenalty - errorPenalty);

            return adjustedConfidence;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate overall confidence");
            return 0f;
        }
    }

    public async Task<float> CalculateFieldConfidenceAsync(ExtractedField field)
    {
        try
        {
            var baseConfidence = field.Confidence;

            // Apply confidence adjustments based on field characteristics
            var lengthBonus = Math.Min(0.1f, field.Value.Length * 0.01f);
            var alternativesBonus = field.Alternatives.Count * 0.02f;

            var adjustedConfidence = Math.Min(1f, baseConfidence + lengthBonus + alternativesBonus);

            return adjustedConfidence;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate field confidence");
            return field.Confidence;
        }
    }

    public async Task<List<ConfidenceScore>> CalculateConfidenceScoresAsync(ExtractionResult extraction)
    {
        try
        {
            var scores = new List<ConfidenceScore>();

            foreach (var field in extraction.Fields)
            {
                var fieldConfidence = await CalculateFieldConfidenceAsync(field);
                scores.Add(new ConfidenceScore(
                    field.FieldType,
                    fieldConfidence,
                    "ML",
                    DateTime.UtcNow
                ));
            }

            return scores;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate confidence scores");
            return new List<ConfidenceScore>();
        }
    }

    public async Task<bool> LoadModelAsync(string modelPath)
    {
        try
        {
            return await _predictionEngineService.LoadModelAsync(modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model: {ModelPath}", modelPath);
            return false;
        }
    }

    public async Task<bool> IsModelLoadedAsync()
    {
        try
        {
            return await _predictionEngineService.IsModelLoadedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if model is loaded");
            return false;
        }
    }

    public async Task<string> GetModelVersionAsync()
    {
        try
        {
            return await _predictionEngineService.GetModelVersionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model version");
            return "unknown";
        }
    }

    public async Task<ModelInfo> GetModelInfoAsync()
    {
        try
        {
            return await _predictionEngineService.GetModelInfoAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model info");
            return new ModelInfo("unknown", "", DateTime.MinValue, DateTime.MinValue, 0, new Dictionary<string, object>());
        }
    }

    public async Task<TrainingDataResult> GenerateTrainingDataAsync(ExtractionResult extraction, List<TextLine> textLines)
    {
        try
        {
            _logger.LogInformation("Generating training data from {Count} fields and {Count} lines",
                extraction.Fields.Count, textLines.Count);

            var rows = new List<TrainingDataRow>();
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            foreach (var line in textLines)
            {
                var field = extraction.Fields.FirstOrDefault(f => f.LineIndex == line.LineIndex);
                var label = field?.FieldType ?? "None";
                var confidence = field?.Confidence ?? 0f;

                var row = new TrainingDataRow(
                    line.Text,
                    line.LineIndex,
                    line.PageNumber,
                    line.X,
                    line.Y,
                    line.Width,
                    line.Height,
                    label,
                    confidence,
                    new Dictionary<string, object>
                    {
                        ["FontSize"] = line.FontSize,
                        ["FontName"] = line.FontName,
                        ["IsBold"] = line.IsBold,
                        ["IsItalic"] = line.IsItalic,
                        ["Alignment"] = line.Alignment
                    }
                );

                rows.Add(row);
            }

            var validRows = rows.Count(r => r.Label != "None");
            var invalidRows = rows.Count(r => r.Label == "None");

            _logger.LogInformation("Generated {Count} training data rows ({ValidCount} valid, {InvalidCount} invalid)",
                rows.Count, validRows, invalidRows);

            return new TrainingDataResult(
                true,
                "Training data generated successfully",
                rows,
                rows.Count,
                validRows,
                invalidRows,
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate training data");
            return new TrainingDataResult(
                false,
                "Training data generation failed",
                new List<TrainingDataRow>(),
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("TRAINING_DATA_GENERATION_FAILED", ex.Message, "Multiple", "Multiple", ex) }
            );
        }
    }

    public async Task<TrainingDataResult> GenerateTrainingDataAsync(string filePath, ExtractionResult extraction)
    {
        try
        {
            var textLines = await _pdfParserService.ExtractTextLinesAsync(filePath);
            return await GenerateTrainingDataAsync(extraction, textLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate training data from file: {FilePath}", filePath);
            return new TrainingDataResult(
                false,
                "Training data generation failed",
                new List<TrainingDataRow>(),
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("TRAINING_DATA_GENERATION_FAILED", ex.Message, "Multiple", "Multiple", ex) }
            );
        }
    }

    public async Task<bool> SaveTrainingDataAsync(TrainingDataResult trainingData, string outputPath)
    {
        try
        {
            _logger.LogInformation("Saving training data to: {OutputPath}", outputPath);

            var json = System.Text.Json.JsonSerializer.Serialize(trainingData.Rows, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, json);

            _logger.LogInformation("Training data saved successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save training data: {OutputPath}", outputPath);
            return false;
        }
    }

    private bool IsHeaderOrFooter(TextLine line)
    {
        // Simple heuristic to identify headers/footers
        var text = line.Text.ToLowerInvariant();
        return text.Contains("page") || text.Contains("seite") ||
               text.Contains("invoice") || text.Contains("rechnung") ||
               text.Length < 10;
    }

    private bool IsPageNumber(TextLine line)
    {
        // Simple heuristic to identify page numbers
        var text = line.Text.Trim();
        return int.TryParse(text, out _) && text.Length <= 3;
    }

    private bool IsValidFieldType(string fieldType)
    {
        var validTypes = new[] { "InvoiceNumber", "InvoiceDate", "IssuerName", "IssuerAddress", "NetTotal", "VatTotal", "GrossTotal", "None" };
        return validTypes.Contains(fieldType);
    }

    private async Task<FormatValidationResult> ValidateFieldFormatAsync(ExtractedField field)
    {
        try
        {
            return await _businessRulesService.ValidateFieldFormatAsync(field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate field format: {FieldType}", field.FieldType);
            return new FormatValidationResult(false, "Format validation failed", field.FieldType, field.Value, ex.Message);
        }
    }
}

public record FormatValidationResult(
    bool IsValid,
    string Message,
    string FieldType,
    string Value,
    string Error
);
```

## 3. Extract Fields Use Case Extensions

**Datei:** `src/InvoiceReader.Application/Extensions/ExtractFieldsExtensions.cs`

```csharp
using InvoiceReader.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Application.Extensions;

public static class ExtractFieldsExtensions
{
    public static IServiceCollection AddExtractFieldsServices(this IServiceCollection services)
    {
        services.AddScoped<IExtractFieldsUseCase, ExtractFieldsUseCase>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Extract Fields Use Case für ML-basierte Feld-Extraktion
- Pre-processing, Feature Extraction und ML Prediction
- Post-processing mit Business Rules und Regex-Validierung
- Confidence Scoring und Model Management
- Training Data Generation für ML-Modelle
- Error Handling für alle Extraktions-Operationen
- Logging für alle Extraktions-Operationen
- Validation für alle extrahierten Felder
- Business Rules für fachliche Validierung
