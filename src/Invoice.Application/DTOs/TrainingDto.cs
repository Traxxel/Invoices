namespace Invoice.Application.DTOs;

public record TrainingRequest(
    string ModelName,
    string Version,
    List<TrainingSample> Samples,
    TrainingOptions Options
);

public record TrainingSample(
    string Text,
    string Label,
    int LineIndex,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string DocumentId,
    Dictionary<string, object> Features
);

public record TrainingOptions(
    string TrainerType,
    Dictionary<string, object> TrainerParameters,
    bool UseCrossValidation,
    int CrossValidationFolds,
    bool UseFeatureSelection,
    bool UseFeatureNormalization,
    int MaxIterations,
    float LearningRate,
    float L1Regularization,
    float L2Regularization,
    string Description,
    Dictionary<string, string> Tags
);

public record TrainingResponse(
    bool Success,
    string ModelVersion,
    string ModelPath,
    TrainingMetrics Metrics,
    ModelEvaluation Evaluation,
    TimeSpan TrainingTime,
    DateTime TrainedAt,
    List<string> Errors,
    List<string> Warnings
);

public record TrainingResult(
    bool Success,
    string Message,
    string? ModelPath,
    object? Metrics,
    TimeSpan TrainingTime,
    DateTime TrainedAt
);

public record TrainingMetrics(
    float Accuracy,
    float MicroF1Score,
    float MacroF1Score,
    float WeightedF1Score,
    float LogLoss,
    float PerClassLogLoss,
    Dictionary<string, float> PerClassMetrics,
    ConfusionMatrix ConfusionMatrix,
    int TotalSamples,
    int CorrectPredictions,
    int IncorrectPredictions
);

public record ModelEvaluation(
    float Accuracy,
    float MicroF1Score,
    float MacroF1Score,
    float WeightedF1Score,
    Dictionary<string, float> PerClassF1Score,
    Dictionary<string, float> PerClassPrecision,
    Dictionary<string, float> PerClassRecall,
    ConfusionMatrix ConfusionMatrix,
    List<ModelError> Errors,
    DateTime EvaluatedAt
);

public record ConfusionMatrix(
    Dictionary<string, Dictionary<string, int>> Matrix,
    List<string> Classes,
    int TotalSamples,
    int CorrectPredictions,
    int IncorrectPredictions
);

public record ModelError(
    string ActualLabel,
    string PredictedLabel,
    float Confidence,
    string Text,
    int LineIndex,
    string DocumentId
);

public record ModelInfo(
    string Version,
    string Name,
    string Description,
    string TrainerType,
    Dictionary<string, object> Parameters,
    ModelEvaluation Evaluation,
    DateTime CreatedAt,
    DateTime LastUsed,
    bool IsActive,
    bool IsLoaded,
    long FileSize,
    string FilePath,
    Dictionary<string, string> Tags
);

public record ModelPerformance(
    string ModelVersion,
    int TotalPredictions,
    int CorrectPredictions,
    float Accuracy,
    float AverageConfidence,
    float AveragePredictionTime,
    Dictionary<string, int> PredictionsByClass,
    Dictionary<string, float> ConfidenceByClass,
    DateTime LastUsed,
    DateTime PerformanceCalculatedAt,
    List<PerformanceMetric> Metrics
);

public record PerformanceMetric(
    string Name,
    float Value,
    string Unit,
    DateTime CalculatedAt
);

