using Invoice.Application.DTOs;

namespace Invoice.Application.Interfaces;

public interface ITrainModelsUseCase
{
    Task<TrainingResult> ExecuteAsync(TrainingRequest request);
    Task<TrainingResult> ExecuteAsync(string trainingDataPath);
    Task<TrainingResult> ExecuteAsync(List<TrainingDataRow> trainingData);

    // Training data preparation
    Task<TrainingDataResult> PrepareTrainingDataAsync(string sourcePath);
    Task<TrainingDataResult> ValidateTrainingDataAsync(TrainingDataResult trainingData);
    Task<TrainingDataResult> CleanTrainingDataAsync(TrainingDataResult trainingData);

    // Model training
    Task<ModelTrainingResult> TrainModelAsync(TrainingDataResult trainingData, TrainingOptions options);

    // Model evaluation
    Task<ModelEvaluationResult> EvaluateModelAsync(string modelPath, string testDataPath);
    Task<ModelEvaluationResult> CrossValidateModelAsync(TrainingDataResult trainingData, TrainingOptions options);

    // Model management
    Task<string> GetModelInfoAsync(string modelPath);
    Task<List<string>> GetAvailableModelsAsync();
    Task<bool> LoadModelAsync(string modelPath);

    // Training options
    Task<TrainingOptions> GetDefaultTrainingOptionsAsync();
}

public record TrainingRequest(
    TrainingDataResult TrainingData,
    TrainingOptions Options,
    string? ModelName = null,
    string? UserId = null,
    string? SessionId = null
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
    int TotalSamples = 0,
    int TrainingSamples = 0,
    int ValidationSamples = 0,
    int TestSamples = 0,
    int Features = 0,
    int Classes = 0,
    float TrainingAccuracy = 0f,
    float ValidationAccuracy = 0f,
    float TestAccuracy = 0f,
    float TrainingLoss = 0f,
    float ValidationLoss = 0f,
    float TestLoss = 0f,
    TimeSpan TrainingTime = default,
    TimeSpan ValidationTime = default,
    TimeSpan TestTime = default,
    Dictionary<string, float>? ClassAccuracies = null,
    Dictionary<string, float>? FeatureImportances = null,
    Dictionary<string, object>? AlgorithmSpecificMetrics = null
);

public record ModelTrainingResult(
    bool Success,
    string Message,
    TrainingStatistics Statistics,
    List<TrainingWarning> Warnings,
    List<TrainingError> Errors,
    DateTime TrainedAt,
    TimeSpan TrainingTime
);

public record ModelEvaluationResult(
    bool Success,
    string Message,
    EvaluationStatistics Statistics,
    List<EvaluationWarning> Warnings,
    List<EvaluationError> Errors,
    DateTime EvaluatedAt,
    TimeSpan EvaluationTime
);

public record EvaluationStatistics(
    float OverallAccuracy = 0f,
    float Precision = 0f,
    float Recall = 0f,
    float F1Score = 0f,
    float AUC = 0f,
    float ConfusionMatrix = 0f,
    Dictionary<string, float>? ClassMetrics = null,
    Dictionary<string, float>? FeatureImportances = null,
    Dictionary<string, object>? AlgorithmSpecificMetrics = null
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

