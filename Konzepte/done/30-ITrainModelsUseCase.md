# Aufgabe 30: ITrainModelsUseCase und TrainingOptions

## Ziel

Train Models Use Case für ML-Modelltraining mit verschiedenen Trainingsoptionen und Evaluierung.

## 1. Train Models Use Case Interface

**Datei:** `src/Invoice.Application/Interfaces/ITrainModelsUseCase.cs`

```csharp
namespace Invoice.Application.Interfaces;

public interface ITrainModelsUseCase
{
    Task<TrainingResult> ExecuteAsync(TrainingRequest request);
    Task<TrainingResult> ExecuteAsync(string trainingDataPath);
    Task<TrainingResult> ExecuteAsync(List<TrainingDataRow> trainingData);
    Task<TrainingResult> ExecuteAsync(TrainingDataResult trainingData);

    // Training data preparation
    Task<TrainingDataResult> PrepareTrainingDataAsync(string sourcePath);
    Task<TrainingDataResult> PrepareTrainingDataAsync(List<InvoiceDto> invoices);
    Task<TrainingDataResult> PrepareTrainingDataAsync(List<ExtractionResult> extractions);
    Task<TrainingDataResult> ValidateTrainingDataAsync(TrainingDataResult trainingData);
    Task<TrainingDataResult> CleanTrainingDataAsync(TrainingDataResult trainingData);
    Task<TrainingDataResult> AugmentTrainingDataAsync(TrainingDataResult trainingData);

    // Model training
    Task<ModelTrainingResult> TrainModelAsync(TrainingDataResult trainingData, TrainingOptions options);
    Task<ModelTrainingResult> TrainModelAsync(string trainingDataPath, TrainingOptions options);
    Task<ModelTrainingResult> TrainModelAsync(List<TrainingDataRow> trainingData, TrainingOptions options);
    Task<ModelTrainingResult> TrainModelAsync(TrainingDataResult trainingData, string modelName, TrainingOptions options);

    // Model evaluation
    Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, string testDataPath);
    Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, List<TrainingDataRow> testData);
    Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, TrainingDataResult testData);
    Task<ModelEvaluationResult> CrossValidateModelAsync(TrainingDataResult trainingData, TrainingOptions options);

    // Model management
    Task<ModelInfo> GetModelInfoAsync(string modelPath);
    Task<List<ModelInfo>> GetAvailableModelsAsync();
    Task<bool> LoadModelAsync(string modelPath);
    Task<bool> SaveModelAsync(string modelPath, byte[] modelData);
    Task<bool> DeleteModelAsync(string modelPath);
    Task<bool> BackupModelAsync(string modelPath, string backupPath);
    Task<bool> RestoreModelAsync(string backupPath, string modelPath);

    // Training options
    Task<TrainingOptions> GetDefaultTrainingOptionsAsync();
    Task<TrainingOptions> GetTrainingOptionsAsync(string modelName);
    Task<bool> UpdateTrainingOptionsAsync(TrainingOptions options);
    Task<List<TrainingOption>> GetAvailableTrainingOptionsAsync();

    // Training progress
    Task<TrainingProgress> GetTrainingProgressAsync(string trainingId);
    Task<List<TrainingProgress>> GetTrainingHistoryAsync();
    Task<bool> CancelTrainingAsync(string trainingId);
    Task<bool> PauseTrainingAsync(string trainingId);
    Task<bool> ResumeTrainingAsync(string trainingId);

    // Training statistics
    Task<TrainingStatistics> GetTrainingStatisticsAsync();
    Task<TrainingStatistics> GetTrainingStatisticsAsync(DateTime fromDate, DateTime toDate);
    Task<ModelPerformanceStatistics> GetModelPerformanceStatisticsAsync(string modelPath);
    Task<ModelPerformanceStatistics> GetModelPerformanceStatisticsAsync(string modelPath, DateTime fromDate, DateTime toDate);
}

public record TrainingRequest(
    TrainingDataResult TrainingData,
    TrainingOptions Options,
    string? ModelName = null,
    string? UserId = null,
    string? SessionId = null
);

public record TrainingOptions(
    string ModelName,
    string Algorithm,
    Dictionary<string, object> AlgorithmParameters,
    int MaxIterations,
    float LearningRate,
    float Regularization,
    int BatchSize,
    int ValidationSplit,
    bool UseCrossValidation,
    int CrossValidationFolds,
    bool UseEarlyStopping,
    int EarlyStoppingPatience,
    bool UseDataAugmentation,
    Dictionary<string, object> DataAugmentationParameters,
    bool UseFeatureSelection,
    List<string> SelectedFeatures,
    bool UseHyperparameterTuning,
    Dictionary<string, object> HyperparameterRanges,
    bool UseEnsemble,
    List<string> EnsembleModels,
    Dictionary<string, object> CustomSettings
);

public record TrainingResult(
    bool Success,
    string Message,
    ModelInfo? Model,
    TrainingStatistics Statistics,
    List<TrainingWarning> Warnings,
    List<TrainingError> Errors,
    DateTime TrainedAt,
    TimeSpan TrainingTime
);

public record TrainingWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record TrainingError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record TrainingStatistics(
    int TotalSamples,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples,
    int Features,
    int Classes,
    float TrainingAccuracy,
    float ValidationAccuracy,
    float TestAccuracy,
    float TrainingLoss,
    float ValidationLoss,
    float TestLoss,
    TimeSpan TrainingTime,
    TimeSpan ValidationTime,
    TimeSpan TestTime,
    Dictionary<string, float> ClassAccuracies,
    Dictionary<string, float> FeatureImportances,
    Dictionary<string, object> AlgorithmSpecificMetrics
);

public record ModelTrainingResult(
    bool Success,
    string Message,
    ModelInfo? Model,
    TrainingStatistics Statistics,
    List<TrainingWarning> Warnings,
    List<TrainingError> Errors,
    DateTime TrainedAt,
    TimeSpan TrainingTime
);

public record ModelEvaluationResult(
    bool Success,
    string Message,
    ModelInfo? Model,
    EvaluationStatistics Statistics,
    List<EvaluationWarning> Warnings,
    List<EvaluationError> Errors,
    DateTime EvaluatedAt,
    TimeSpan EvaluationTime
);

public record EvaluationStatistics(
    float OverallAccuracy,
    float Precision,
    float Recall,
    float F1Score,
    float AUC,
    float ConfusionMatrix,
    Dictionary<string, float> ClassMetrics,
    Dictionary<string, float> FeatureImportances,
    Dictionary<string, object> AlgorithmSpecificMetrics
);

public record EvaluationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record EvaluationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record ModelInfo(
    string Name,
    string Version,
    string Path,
    string Algorithm,
    DateTime CreatedAt,
    DateTime LastModified,
    long Size,
    Dictionary<string, object> Metadata,
    TrainingOptions? TrainingOptions,
    TrainingStatistics? TrainingStatistics,
    EvaluationStatistics? EvaluationStatistics
);

public record TrainingProgress(
    string TrainingId,
    string ModelName,
    TrainingStatus Status,
    float Progress,
    int CurrentEpoch,
    int TotalEpochs,
    float CurrentLoss,
    float CurrentAccuracy,
    DateTime StartedAt,
    DateTime? CompletedAt,
    TimeSpan? EstimatedTimeRemaining,
    List<string> Logs
);

public enum TrainingStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public record TrainingOption(
    string Name,
    string Description,
    string Type,
    object DefaultValue,
    object MinValue,
    object MaxValue,
    List<object> AllowedValues,
    bool IsRequired,
    string Category
);

public record ModelPerformanceStatistics(
    int TotalPredictions,
    int CorrectPredictions,
    int IncorrectPredictions,
    float OverallAccuracy,
    float AverageConfidence,
    Dictionary<string, int> PredictionsByClass,
    Dictionary<string, float> AccuracyByClass,
    Dictionary<string, float> ConfidenceByClass,
    TimeSpan AveragePredictionTime,
    TimeSpan TotalPredictionTime,
    Dictionary<string, object> CustomMetrics
);
```

## 2. Train Models Use Case Implementation

**Datei:** `src/Invoice.Application/UseCases/TrainModelsUseCase.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Invoice.Application.UseCases;

public class TrainModelsUseCase : ITrainModelsUseCase
{
    private readonly IMLContextService _mlContextService;
    private readonly IFeatureExtractionService _featureExtractionService;
    private readonly IModelStorageService _modelStorageService;
    private readonly ILogger<TrainModelsUseCase> _logger;

    public TrainModelsUseCase(
        IMLContextService mlContextService,
        IFeatureExtractionService featureExtractionService,
        IModelStorageService modelStorageService,
        ILogger<TrainModelsUseCase> logger)
    {
        _mlContextService = mlContextService;
        _featureExtractionService = featureExtractionService;
        _modelStorageService = modelStorageService;
        _logger = logger;
    }

    public async Task<TrainingResult> ExecuteAsync(TrainingRequest request)
    {
        var startTime = DateTime.UtcNow;
        var warnings = new List<TrainingWarning>();
        var errors = new List<TrainingError>();

        try
        {
            _logger.LogInformation("Starting model training: {ModelName}", request.Options.ModelName);

            // Validate training data
            var validation = await ValidateTrainingDataAsync(request.TrainingData);
            if (!validation.Success)
            {
                return new TrainingResult(
                    false,
                    "Training data validation failed",
                    null,
                    new TrainingStatistics(),
                    warnings,
                    validation.Errors.Select(e => new TrainingError(e.Code, e.Message, e.Field, e.Value, null)).ToList(),
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Clean training data
            var cleanedData = await CleanTrainingDataAsync(request.TrainingData);
            _logger.LogInformation("Training data cleaned: {Count} rows", cleanedData.Rows.Count);

            // Augment training data if enabled
            if (request.Options.UseDataAugmentation)
            {
                cleanedData = await AugmentTrainingDataAsync(cleanedData);
                _logger.LogInformation("Training data augmented: {Count} rows", cleanedData.Rows.Count);
            }

            // Train model
            var trainingResult = await TrainModelAsync(cleanedData, request.Options);
            if (!trainingResult.Success)
            {
                return new TrainingResult(
                    false,
                    "Model training failed",
                    null,
                    new TrainingStatistics(),
                    warnings,
                    trainingResult.Errors,
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Evaluate model
            var evaluationResult = await EvaluateModelAsync(trainingResult.Model.Path, cleanedData);
            if (!evaluationResult.Success)
            {
                warnings.Add(new TrainingWarning(
                    "EVALUATION_FAILED",
                    "Model evaluation failed",
                    "Model",
                    trainingResult.Model.Name,
                    "Model was trained but evaluation failed"
                ));
            }

            _logger.LogInformation("Model training completed successfully: {ModelName}", request.Options.ModelName);

            return new TrainingResult(
                true,
                "Model training completed successfully",
                trainingResult.Model,
                trainingResult.Statistics,
                warnings,
                errors,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model training failed: {ModelName}", request.Options.ModelName);

            return new TrainingResult(
                false,
                "Model training failed",
                null,
                new TrainingStatistics(),
                warnings,
                new List<TrainingError> { new TrainingError("TRAINING_FAILED", ex.Message, "Model", request.Options.ModelName, ex) },
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<TrainingResult> ExecuteAsync(string trainingDataPath)
    {
        try
        {
            var trainingData = await PrepareTrainingDataAsync(trainingDataPath);
            var options = await GetDefaultTrainingOptionsAsync();
            var request = new TrainingRequest(trainingData, options);
            return await ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model from data path: {TrainingDataPath}", trainingDataPath);
            throw;
        }
    }

    public async Task<TrainingResult> ExecuteAsync(List<TrainingDataRow> trainingData)
    {
        try
        {
            var trainingDataResult = new TrainingDataResult(
                true,
                "Training data prepared",
                trainingData,
                trainingData.Count,
                trainingData.Count(r => r.Label != "None"),
                trainingData.Count(r => r.Label == "None"),
                new List<TrainingDataWarning>(),
                new List<TrainingDataError>()
            );

            var options = await GetDefaultTrainingOptionsAsync();
            var request = new TrainingRequest(trainingDataResult, options);
            return await ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model from training data rows");
            throw;
        }
    }

    public async Task<TrainingResult> ExecuteAsync(TrainingDataResult trainingData)
    {
        try
        {
            var options = await GetDefaultTrainingOptionsAsync();
            var request = new TrainingRequest(trainingData, options);
            return await ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model from training data result");
            throw;
        }
    }

    public async Task<TrainingDataResult> PrepareTrainingDataAsync(string sourcePath)
    {
        try
        {
            _logger.LogInformation("Preparing training data from: {SourcePath}", sourcePath);

            var trainingData = new List<TrainingDataRow>();
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            // Load data from file
            var json = await File.ReadAllTextAsync(sourcePath);
            var data = System.Text.Json.JsonSerializer.Deserialize<List<TrainingDataRow>>(json);

            if (data == null)
            {
                errors.Add(new TrainingDataError("DATA_LOAD_FAILED", "Failed to load training data", "File", sourcePath, null));
                return new TrainingDataResult(false, "Failed to load training data", new List<TrainingDataRow>(), 0, 0, 0, new List<TrainingDataWarning>(), errors);
            }

            trainingData.AddRange(data);

            _logger.LogInformation("Training data prepared: {Count} rows", trainingData.Count);

            return new TrainingDataResult(
                true,
                "Training data prepared successfully",
                trainingData,
                trainingData.Count,
                trainingData.Count(r => r.Label != "None"),
                trainingData.Count(r => r.Label == "None"),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare training data from: {SourcePath}", sourcePath);
            return new TrainingDataResult(
                false,
                "Training data preparation failed",
                new List<TrainingDataRow>(),
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("DATA_PREPARATION_FAILED", ex.Message, "File", sourcePath, ex) }
            );
        }
    }

    public async Task<TrainingDataResult> PrepareTrainingDataAsync(List<InvoiceDto> invoices)
    {
        try
        {
            _logger.LogInformation("Preparing training data from {Count} invoices", invoices.Count);

            var trainingData = new List<TrainingDataRow>();
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            foreach (var invoice in invoices)
            {
                // Extract features from invoice
                var features = await _featureExtractionService.ExtractFeaturesFromInvoiceAsync(invoice);

                foreach (var feature in features)
                {
                    var row = new TrainingDataRow(
                        feature.Text,
                        feature.LineIndex,
                        feature.PageNumber,
                        feature.Position.X,
                        feature.Position.Y,
                        feature.Position.Width,
                        feature.Position.Height,
                        feature.Label,
                        feature.Confidence,
                        feature.CustomFeatures
                    );

                    trainingData.Add(row);
                }
            }

            _logger.LogInformation("Training data prepared from invoices: {Count} rows", trainingData.Count);

            return new TrainingDataResult(
                true,
                "Training data prepared successfully from invoices",
                trainingData,
                trainingData.Count,
                trainingData.Count(r => r.Label != "None"),
                trainingData.Count(r => r.Label == "None"),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare training data from invoices");
            return new TrainingDataResult(
                false,
                "Training data preparation failed",
                new List<TrainingDataRow>(),
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("DATA_PREPARATION_FAILED", ex.Message, "Invoices", invoices.Count.ToString(), ex) }
            );
        }
    }

    public async Task<TrainingDataResult> PrepareTrainingDataAsync(List<ExtractionResult> extractions)
    {
        try
        {
            _logger.LogInformation("Preparing training data from {Count} extractions", extractions.Count);

            var trainingData = new List<TrainingDataRow>();
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            foreach (var extraction in extractions)
            {
                foreach (var field in extraction.Fields)
                {
                    var row = new TrainingDataRow(
                        field.SourceText,
                        field.LineIndex,
                        field.PageNumber,
                        field.X,
                        field.Y,
                        field.Width,
                        field.Height,
                        field.FieldType,
                        field.Confidence,
                        new Dictionary<string, object>
                        {
                            ["FontSize"] = field.FontSize,
                            ["FontName"] = field.FontName,
                            ["IsBold"] = field.IsBold,
                            ["IsItalic"] = field.IsItalic,
                            ["Alignment"] = field.Alignment
                        }
                    );

                    trainingData.Add(row);
                }
            }

            _logger.LogInformation("Training data prepared from extractions: {Count} rows", trainingData.Count);

            return new TrainingDataResult(
                true,
                "Training data prepared successfully from extractions",
                trainingData,
                trainingData.Count,
                trainingData.Count(r => r.Label != "None"),
                trainingData.Count(r => r.Label == "None"),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare training data from extractions");
            return new TrainingDataResult(
                false,
                "Training data preparation failed",
                new List<TrainingDataRow>(),
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("DATA_PREPARATION_FAILED", ex.Message, "Extractions", extractions.Count.ToString(), ex) }
            );
        }
    }

    public async Task<TrainingDataResult> ValidateTrainingDataAsync(TrainingDataResult trainingData)
    {
        try
        {
            _logger.LogInformation("Validating training data: {Count} rows", trainingData.Rows.Count);

            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            // Check for empty data
            if (trainingData.Rows.Count == 0)
            {
                errors.Add(new TrainingDataError("EMPTY_DATA", "Training data is empty", "Data", "0", null));
                return new TrainingDataResult(false, "Training data validation failed", trainingData.Rows, 0, 0, 0, warnings, errors);
            }

            // Check for valid labels
            var validLabels = trainingData.Rows.Count(r => r.Label != "None");
            if (validLabels == 0)
            {
                errors.Add(new TrainingDataError("NO_VALID_LABELS", "No valid labels found", "Data", "0", null));
                return new TrainingDataResult(false, "Training data validation failed", trainingData.Rows, 0, 0, 0, warnings, errors);
            }

            // Check for balanced classes
            var classDistribution = trainingData.Rows
                .Where(r => r.Label != "None")
                .GroupBy(r => r.Label)
                .ToDictionary(g => g.Key, g => g.Count());

            var minClassCount = classDistribution.Values.Min();
            var maxClassCount = classDistribution.Values.Max();
            var imbalanceRatio = (float)maxClassCount / minClassCount;

            if (imbalanceRatio > 10.0f)
            {
                warnings.Add(new TrainingDataWarning(
                    "CLASS_IMBALANCE",
                    "Significant class imbalance detected",
                    "Data",
                    imbalanceRatio.ToString(),
                    "Consider using class balancing techniques"
                ));
            }

            // Check for missing features
            var missingFeatures = trainingData.Rows.Count(r => string.IsNullOrWhiteSpace(r.Text));
            if (missingFeatures > 0)
            {
                warnings.Add(new TrainingDataWarning(
                    "MISSING_FEATURES",
                    "Some rows have missing text features",
                    "Data",
                    missingFeatures.ToString(),
                    "Consider cleaning or removing rows with missing features"
                ));
            }

            _logger.LogInformation("Training data validation completed: {ValidCount} valid, {WarningCount} warnings, {ErrorCount} errors",
                validLabels, warnings.Count, errors.Count);

            return new TrainingDataResult(
                errors.Count == 0,
                "Training data validation completed",
                trainingData.Rows,
                trainingData.TotalRows,
                validLabels,
                trainingData.InvalidRows,
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate training data");
            return new TrainingDataResult(
                false,
                "Training data validation failed",
                trainingData.Rows,
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("VALIDATION_FAILED", ex.Message, "Data", "Multiple", ex) }
            );
        }
    }

    public async Task<TrainingDataResult> CleanTrainingDataAsync(TrainingDataResult trainingData)
    {
        try
        {
            _logger.LogInformation("Cleaning training data: {Count} rows", trainingData.Rows.Count);

            var cleanedRows = new List<TrainingDataRow>();
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            foreach (var row in trainingData.Rows)
            {
                // Remove rows with empty text
                if (string.IsNullOrWhiteSpace(row.Text))
                {
                    warnings.Add(new TrainingDataWarning(
                        "EMPTY_TEXT",
                        "Row with empty text removed",
                        "Text",
                        row.Text,
                        "Consider providing text for this row"
                    ));
                    continue;
                }

                // Remove rows with invalid labels
                if (string.IsNullOrWhiteSpace(row.Label))
                {
                    warnings.Add(new TrainingDataWarning(
                        "EMPTY_LABEL",
                        "Row with empty label removed",
                        "Label",
                        row.Label,
                        "Consider providing a label for this row"
                    ));
                    continue;
                }

                // Clean text
                var cleanedText = row.Text.Trim();
                if (cleanedText != row.Text)
                {
                    warnings.Add(new TrainingDataWarning(
                        "TEXT_TRIMMED",
                        "Text was trimmed",
                        "Text",
                        row.Text,
                        "Consider providing clean text"
                    ));
                }

                // Create cleaned row
                var cleanedRow = row with { Text = cleanedText };
                cleanedRows.Add(cleanedRow);
            }

            _logger.LogInformation("Training data cleaned: {OriginalCount} -> {CleanedCount} rows",
                trainingData.Rows.Count, cleanedRows.Count);

            return new TrainingDataResult(
                true,
                "Training data cleaned successfully",
                cleanedRows,
                cleanedRows.Count,
                cleanedRows.Count(r => r.Label != "None"),
                cleanedRows.Count(r => r.Label == "None"),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean training data");
            return new TrainingDataResult(
                false,
                "Training data cleaning failed",
                trainingData.Rows,
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("CLEANING_FAILED", ex.Message, "Data", "Multiple", ex) }
            );
        }
    }

    public async Task<TrainingDataResult> AugmentTrainingDataAsync(TrainingDataResult trainingData)
    {
        try
        {
            _logger.LogInformation("Augmenting training data: {Count} rows", trainingData.Rows.Count);

            var augmentedRows = new List<TrainingDataRow>(trainingData.Rows);
            var warnings = new List<TrainingDataWarning>();
            var errors = new List<TrainingDataError>();

            // Apply data augmentation techniques
            foreach (var row in trainingData.Rows)
            {
                if (row.Label == "None")
                    continue;

                // Text augmentation
                var augmentedTexts = await _featureExtractionService.AugmentTextAsync(row.Text);
                foreach (var augmentedText in augmentedTexts)
                {
                    var augmentedRow = row with { Text = augmentedText };
                    augmentedRows.Add(augmentedRow);
                }

                // Feature augmentation
                var augmentedFeatures = await _featureExtractionService.AugmentFeaturesAsync(row);
                foreach (var augmentedFeature in augmentedFeatures)
                {
                    var augmentedRow = row with { Features = augmentedFeature };
                    augmentedRows.Add(augmentedRow);
                }
            }

            _logger.LogInformation("Training data augmented: {OriginalCount} -> {AugmentedCount} rows",
                trainingData.Rows.Count, augmentedRows.Count);

            return new TrainingDataResult(
                true,
                "Training data augmented successfully",
                augmentedRows,
                augmentedRows.Count,
                augmentedRows.Count(r => r.Label != "None"),
                augmentedRows.Count(r => r.Label == "None"),
                warnings,
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to augment training data");
            return new TrainingDataResult(
                false,
                "Training data augmentation failed",
                trainingData.Rows,
                0,
                0,
                0,
                new List<TrainingDataWarning>(),
                new List<TrainingDataError> { new TrainingDataError("AUGMENTATION_FAILED", ex.Message, "Data", "Multiple", ex) }
            );
        }
    }

    public async Task<ModelTrainingResult> TrainModelAsync(TrainingDataResult trainingData, TrainingOptions options)
    {
        try
        {
            _logger.LogInformation("Training model: {ModelName}, {Algorithm}", options.ModelName, options.Algorithm);

            var startTime = DateTime.UtcNow;
            var warnings = new List<TrainingWarning>();
            var errors = new List<TrainingError>();

            // Create ML context
            var mlContext = _mlContextService.CreateContext();

            // Prepare data
            var dataView = await _mlContextService.PrepareDataViewAsync(trainingData.Rows);

            // Split data
            var trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            // Create pipeline
            var pipeline = await _mlContextService.CreatePipelineAsync(options);

            // Train model
            var model = pipeline.Fit(trainData);

            // Evaluate model
            var predictions = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            // Create model info
            var modelInfo = new ModelInfo(
                options.ModelName,
                "1.0",
                Path.Combine("models", $"{options.ModelName}.zip"),
                options.Algorithm,
                DateTime.UtcNow,
                DateTime.UtcNow,
                0, // Size will be calculated when saved
                new Dictionary<string, object>(),
                options,
                new TrainingStatistics(
                    trainingData.TotalRows,
                    (int)(trainingData.TotalRows * 0.8),
                    (int)(trainingData.TotalRows * 0.2),
                    0, // Test samples
                    trainingData.Rows.FirstOrDefault()?.Features?.Count ?? 0,
                    trainingData.Rows.Select(r => r.Label).Distinct().Count(),
                    (float)metrics.MacroAccuracy,
                    (float)metrics.MacroAccuracy,
                    (float)metrics.MacroAccuracy,
                    0f, // Training loss
                    0f, // Validation loss
                    0f, // Test loss
                    DateTime.UtcNow - startTime,
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    new Dictionary<string, float>(),
                    new Dictionary<string, float>(),
                    new Dictionary<string, object>()
                ),
                null
            );

            // Save model
            await _modelStorageService.SaveModelAsync(modelInfo, model);

            _logger.LogInformation("Model training completed: {ModelName}, {Accuracy:F2} accuracy",
                options.ModelName, metrics.MacroAccuracy);

            return new ModelTrainingResult(
                true,
                "Model training completed successfully",
                modelInfo,
                modelInfo.TrainingStatistics,
                warnings,
                errors,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model training failed: {ModelName}", options.ModelName);
            return new ModelTrainingResult(
                false,
                "Model training failed",
                null,
                new TrainingStatistics(),
                new List<TrainingWarning>(),
                new List<TrainingError> { new TrainingError("TRAINING_FAILED", ex.Message, "Model", options.ModelName, ex) },
                DateTime.UtcNow,
                TimeSpan.Zero
            );
        }
    }

    public async Task<ModelTrainingResult> TrainModelAsync(string trainingDataPath, TrainingOptions options)
    {
        try
        {
            var trainingData = await PrepareTrainingDataAsync(trainingDataPath);
            return await TrainModelAsync(trainingData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model from data path: {TrainingDataPath}", trainingDataPath);
            throw;
        }
    }

    public async Task<ModelTrainingResult> TrainModelAsync(List<TrainingDataRow> trainingData, TrainingOptions options)
    {
        try
        {
            var trainingDataResult = new TrainingDataResult(
                true,
                "Training data prepared",
                trainingData,
                trainingData.Count,
                trainingData.Count(r => r.Label != "None"),
                trainingData.Count(r => r.Label == "None"),
                new List<TrainingDataWarning>(),
                new List<TrainingDataError>()
            );

            return await TrainModelAsync(trainingDataResult, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model from training data rows");
            throw;
        }
    }

    public async Task<ModelTrainingResult> TrainModelAsync(TrainingDataResult trainingData, string modelName, TrainingOptions options)
    {
        try
        {
            var updatedOptions = options with { ModelName = modelName };
            return await TrainModelAsync(trainingData, updatedOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train model with custom name: {ModelName}", modelName);
            throw;
        }
    }

    public async Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, string testDataPath)
    {
        try
        {
            var testData = await PrepareTrainingDataAsync(testDataPath);
            return await EvaluateModelAsync(modelPath, testData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate model from data path: {ModelPath}, {TestDataPath}", modelPath, testDataPath);
            throw;
        }
    }

    public async Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, List<TrainingDataRow> testData)
    {
        try
        {
            var testDataResult = new TrainingDataResult(
                true,
                "Test data prepared",
                testData,
                testData.Count,
                testData.Count(r => r.Label != "None"),
                testData.Count(r => r.Label == "None"),
                new List<TrainingDataWarning>(),
                new List<TrainingDataError>()
            );

            return await EvaluateModelAsync(modelPath, testDataResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate model from test data rows");
            throw;
        }
    }

    public async Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, TrainingDataResult testData)
    {
        try
        {
            _logger.LogInformation("Evaluating model: {ModelPath}", modelPath);

            var startTime = DateTime.UtcNow;
            var warnings = new List<EvaluationWarning>();
            var errors = new List<EvaluationError>();

            // Load model
            var model = await _modelStorageService.LoadModelAsync(modelPath);
            if (model == null)
            {
                return new ModelEvaluationResult(
                    false,
                    "Failed to load model",
                    null,
                    new EvaluationStatistics(),
                    warnings,
                    new List<EvaluationError> { new EvaluationError("MODEL_LOAD_FAILED", "Failed to load model", "Model", modelPath, null) },
                    DateTime.UtcNow,
                    DateTime.UtcNow - startTime
                );
            }

            // Prepare test data
            var dataView = await _mlContextService.PrepareDataViewAsync(testData.Rows);

            // Make predictions
            var predictions = model.Transform(dataView);

            // Evaluate model
            var metrics = _mlContextService.EvaluateModel(predictions);

            var statistics = new EvaluationStatistics(
                metrics.OverallAccuracy,
                metrics.Precision,
                metrics.Recall,
                metrics.F1Score,
                metrics.AUC,
                0f, // Confusion matrix
                metrics.ClassMetrics,
                metrics.FeatureImportances,
                metrics.AlgorithmSpecificMetrics
            );

            _logger.LogInformation("Model evaluation completed: {ModelPath}, {Accuracy:F2} accuracy",
                modelPath, metrics.OverallAccuracy);

            return new ModelEvaluationResult(
                true,
                "Model evaluation completed successfully",
                model,
                statistics,
                warnings,
                errors,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model evaluation failed: {ModelPath}", modelPath);
            return new ModelEvaluationResult(
                false,
                "Model evaluation failed",
                null,
                new EvaluationStatistics(),
                new List<EvaluationWarning>(),
                new List<EvaluationError> { new EvaluationError("EVALUATION_FAILED", ex.Message, "Model", modelPath, ex) },
                DateTime.UtcNow,
                TimeSpan.Zero
            );
        }
    }

    public async Task<ModelEvaluationResult> CrossValidateModelAsync(TrainingDataResult trainingData, TrainingOptions options)
    {
        try
        {
            _logger.LogInformation("Cross-validating model: {ModelName}", options.ModelName);

            var startTime = DateTime.UtcNow;
            var warnings = new List<EvaluationWarning>();
            var errors = new List<EvaluationError>();

            // Create ML context
            var mlContext = _mlContextService.CreateContext();

            // Prepare data
            var dataView = await _mlContextService.PrepareDataViewAsync(trainingData.Rows);

            // Create pipeline
            var pipeline = await _mlContextService.CreatePipelineAsync(options);

            // Perform cross-validation
            var cvResults = mlContext.MulticlassClassification.CrossValidate(dataView, pipeline, numberOfFolds: options.CrossValidationFolds);

            // Calculate average metrics
            var avgAccuracy = cvResults.Average(r => r.Metrics.MacroAccuracy);
            var avgPrecision = cvResults.Average(r => r.Metrics.MacroPrecision);
            var avgRecall = cvResults.Average(r => r.Metrics.MacroRecall);
            var avgF1Score = cvResults.Average(r => r.Metrics.MacroF1Score);

            var statistics = new EvaluationStatistics(
                avgAccuracy,
                avgPrecision,
                avgRecall,
                avgF1Score,
                0f, // AUC
                0f, // Confusion matrix
                new Dictionary<string, float>(),
                new Dictionary<string, float>(),
                new Dictionary<string, object>()
            );

            _logger.LogInformation("Cross-validation completed: {ModelName}, {Accuracy:F2} accuracy",
                options.ModelName, avgAccuracy);

            return new ModelEvaluationResult(
                true,
                "Cross-validation completed successfully",
                null,
                statistics,
                warnings,
                errors,
                DateTime.UtcNow,
                DateTime.UtcNow - startTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cross-validation failed: {ModelName}", options.ModelName);
            return new ModelEvaluationResult(
                false,
                "Cross-validation failed",
                null,
                new EvaluationStatistics(),
                new List<EvaluationWarning>(),
                new List<EvaluationError> { new EvaluationError("CROSS_VALIDATION_FAILED", ex.Message, "Model", options.ModelName, ex) },
                DateTime.UtcNow,
                TimeSpan.Zero
            );
        }
    }

    public async Task<ModelInfo> GetModelInfoAsync(string modelPath)
    {
        try
        {
            return await _modelStorageService.GetModelInfoAsync(modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model info: {ModelPath}", modelPath);
            return new ModelInfo("unknown", "unknown", modelPath, "unknown", DateTime.MinValue, DateTime.MinValue, 0, new Dictionary<string, object>(), null, null, null);
        }
    }

    public async Task<List<ModelInfo>> GetAvailableModelsAsync()
    {
        try
        {
            return await _modelStorageService.GetAvailableModelsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models");
            return new List<ModelInfo>();
        }
    }

    public async Task<bool> LoadModelAsync(string modelPath)
    {
        try
        {
            return await _modelStorageService.LoadModelAsync(modelPath) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model: {ModelPath}", modelPath);
            return false;
        }
    }

    public async Task<bool> SaveModelAsync(string modelPath, byte[] modelData)
    {
        try
        {
            return await _modelStorageService.SaveModelAsync(modelPath, modelData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save model: {ModelPath}", modelPath);
            return false;
        }
    }

    public async Task<bool> DeleteModelAsync(string modelPath)
    {
        try
        {
            return await _modelStorageService.DeleteModelAsync(modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete model: {ModelPath}", modelPath);
            return false;
        }
    }

    public async Task<bool> BackupModelAsync(string modelPath, string backupPath)
    {
        try
        {
            return await _modelStorageService.BackupModelAsync(modelPath, backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to backup model: {ModelPath}, {BackupPath}", modelPath, backupPath);
            return false;
        }
    }

    public async Task<bool> RestoreModelAsync(string backupPath, string modelPath)
    {
        try
        {
            return await _modelStorageService.RestoreModelAsync(backupPath, modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore model: {BackupPath}, {ModelPath}", backupPath, modelPath);
            return false;
        }
    }

    public async Task<TrainingOptions> GetDefaultTrainingOptionsAsync()
    {
        return new TrainingOptions(
            ModelName: "DefaultModel",
            Algorithm: "SdcaMaximumEntropy",
            AlgorithmParameters: new Dictionary<string, object>
            {
                ["L1Regularization"] = 0.1f,
                ["L2Regularization"] = 0.1f
            },
            MaxIterations: 100,
            LearningRate: 0.01f,
            Regularization: 0.1f,
            BatchSize: 32,
            ValidationSplit: 20,
            UseCrossValidation: true,
            CrossValidationFolds: 5,
            UseEarlyStopping: true,
            EarlyStoppingPatience: 10,
            UseDataAugmentation: false,
            DataAugmentationParameters: new Dictionary<string, object>(),
            UseFeatureSelection: false,
            SelectedFeatures: new List<string>(),
            UseHyperparameterTuning: false,
            HyperparameterRanges: new Dictionary<string, object>(),
            UseEnsemble: false,
            EnsembleModels: new List<string>(),
            CustomSettings: new Dictionary<string, object>()
        );
    }

    public async Task<TrainingOptions> GetTrainingOptionsAsync(string modelName)
    {
        try
        {
            var modelInfo = await GetModelInfoAsync(modelName);
            return modelInfo.TrainingOptions ?? await GetDefaultTrainingOptionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training options for model: {ModelName}", modelName);
            return await GetDefaultTrainingOptionsAsync();
        }
    }

    public async Task<bool> UpdateTrainingOptionsAsync(TrainingOptions options)
    {
        try
        {
            return await _modelStorageService.UpdateTrainingOptionsAsync(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update training options");
            return false;
        }
    }

    public async Task<List<TrainingOption>> GetAvailableTrainingOptionsAsync()
    {
        try
        {
            return await _modelStorageService.GetAvailableTrainingOptionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available training options");
            return new List<TrainingOption>();
        }
    }

    public async Task<TrainingProgress> GetTrainingProgressAsync(string trainingId)
    {
        try
        {
            return await _modelStorageService.GetTrainingProgressAsync(trainingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training progress: {TrainingId}", trainingId);
            return new TrainingProgress(trainingId, "Unknown", TrainingStatus.Failed, 0f, 0, 0, 0f, 0f, DateTime.MinValue, null, null, new List<string>());
        }
    }

    public async Task<List<TrainingProgress>> GetTrainingHistoryAsync()
    {
        try
        {
            return await _modelStorageService.GetTrainingHistoryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training history");
            return new List<TrainingProgress>();
        }
    }

    public async Task<bool> CancelTrainingAsync(string trainingId)
    {
        try
        {
            return await _modelStorageService.CancelTrainingAsync(trainingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel training: {TrainingId}", trainingId);
            return false;
        }
    }

    public async Task<bool> PauseTrainingAsync(string trainingId)
    {
        try
        {
            return await _modelStorageService.PauseTrainingAsync(trainingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause training: {TrainingId}", trainingId);
            return false;
        }
    }

    public async Task<bool> ResumeTrainingAsync(string trainingId)
    {
        try
        {
            return await _modelStorageService.ResumeTrainingAsync(trainingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume training: {TrainingId}", trainingId);
            return false;
        }
    }

    public async Task<TrainingStatistics> GetTrainingStatisticsAsync()
    {
        try
        {
            return await _modelStorageService.GetTrainingStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training statistics");
            return new TrainingStatistics();
        }
    }

    public async Task<TrainingStatistics> GetTrainingStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await _modelStorageService.GetTrainingStatisticsAsync(fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training statistics for date range");
            return new TrainingStatistics();
        }
    }

    public async Task<ModelPerformanceStatistics> GetModelPerformanceStatisticsAsync(string modelPath)
    {
        try
        {
            return await _modelStorageService.GetModelPerformanceStatisticsAsync(modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model performance statistics: {ModelPath}", modelPath);
            return new ModelPerformanceStatistics();
        }
    }

    public async Task<ModelPerformanceStatistics> GetModelPerformanceStatisticsAsync(string modelPath, DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await _modelStorageService.GetModelPerformanceStatisticsAsync(modelPath, fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model performance statistics for date range: {ModelPath}", modelPath);
            return new ModelPerformanceStatistics();
        }
    }
}
```

## 3. Train Models Use Case Extensions

**Datei:** `src/Invoice.Application/Extensions/TrainModelsExtensions.cs`

```csharp
using Invoice.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Application.Extensions;

public static class TrainModelsExtensions
{
    public static IServiceCollection AddTrainModelsServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainModelsUseCase, TrainModelsUseCase>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Train Models Use Case für ML-Modelltraining
- Training Data Preparation und Validation
- Model Training mit verschiedenen Algorithmen
- Model Evaluation und Cross-Validation
- Model Management für Speicherung und Laden
- Training Options für konfigurierbare Parameter
- Training Progress für Monitoring
- Training Statistics für Performance-Tracking
- Error Handling für alle Training-Operationen
- Logging für alle Training-Operationen
- Data Augmentation für bessere Modelle
- Hyperparameter Tuning für Optimierung
