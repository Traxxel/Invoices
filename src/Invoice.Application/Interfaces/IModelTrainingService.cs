using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Application.Models;

namespace Invoice.Application.Interfaces;

public interface IModelTrainingService
{
    // Training operations
    Task<TrainingResult> TrainModelAsync(TrainingData trainingData, TrainingOptions options);
    Task<TrainingResult> RetrainModelAsync(string modelVersion, TrainingData trainingData, TrainingOptions options);
    Task<TrainingResult> IncrementalTrainingAsync(string modelVersion, TrainingData newData, TrainingOptions options);

    // Model management
    Task<bool> SaveModelAsync(ITransformer model, string version, ModelMetadata metadata);
    Task<ITransformer> LoadModelAsync(string version);
    Task<List<ModelInfo>> GetAvailableModelsAsync();
    Task<bool> DeleteModelAsync(string version);
    Task<bool> SetActiveModelAsync(string version);

    // Training data management
    Task<TrainingData> PrepareTrainingDataAsync(List<ExtractedFeature> features);
    Task<TrainingData> SplitTrainingDataAsync(TrainingData data, float trainRatio = 0.8f, float validationRatio = 0.1f);
    Task<TrainingData> AugmentTrainingDataAsync(TrainingData data, AugmentationOptions options);

    // Model evaluation
    Task<ModelEvaluation> EvaluateModelAsync(ITransformer model, TrainingData testData);
    Task<ModelComparison> CompareModelsAsync(List<string> modelVersions, TrainingData testData);
    Task<ModelPerformance> GetModelPerformanceAsync(string version);

    // Training monitoring
    Task<TrainingProgress> GetTrainingProgressAsync(string trainingId);
    Task<bool> CancelTrainingAsync(string trainingId);
    Task<List<TrainingJob>> GetTrainingJobsAsync();
}

public class TrainingData
{
    public IDataView TrainData { get; set; } = null!;
    public IDataView ValidationData { get; set; } = null!;
    public IDataView TestData { get; set; } = null!;
    public int TotalSamples { get; set; }
    public int TrainSamples { get; set; }
    public int ValidationSamples { get; set; }
    public int TestSamples { get; set; }
    public Dictionary<string, int> ClassDistribution { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TrainingOptions
{
    public string ModelName { get; set; } = "InvoiceFieldClassifier";
    public string Version { get; set; } = "v1.0";
    public string TrainerType { get; set; } = "SdcaMaximumEntropy";
    public Dictionary<string, object> TrainerParameters { get; set; } = new();
    public bool UseCrossValidation { get; set; } = true;
    public int CrossValidationFolds { get; set; } = 5;
    public bool UseFeatureSelection { get; set; } = false;
    public bool UseFeatureNormalization { get; set; } = true;
    public int MaxIterations { get; set; } = 100;
    public float LearningRate { get; set; } = 0.01f;
    public float L1Regularization { get; set; } = 0.01f;
    public float L2Regularization { get; set; } = 0.01f;
    public bool SaveModel { get; set; } = true;
    public bool SetAsActive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class TrainingResult
{
    public bool Success { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
    public ITransformer Model { get; set; } = null!;
    public ModelEvaluation Evaluation { get; set; } = new();
    public TrainingMetrics Metrics { get; set; } = new();
    public TimeSpan TrainingTime { get; set; }
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class TrainingMetrics
{
    public float Accuracy { get; set; }
    public float MicroF1Score { get; set; }
    public float MacroF1Score { get; set; }
    public float WeightedF1Score { get; set; }
    public float LogLoss { get; set; }
    public float PerClassLogLoss { get; set; }
    public Dictionary<string, float> PerClassMetrics { get; set; } = new();
    public ConfusionMatrix ConfusionMatrix { get; set; } = new();
    public int TotalSamples { get; set; }
    public int CorrectPredictions { get; set; }
    public int IncorrectPredictions { get; set; }
}

public class ModelMetadata
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TrainerType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TrainingData TrainingData { get; set; } = null!;
    public ModelEvaluation Evaluation { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

public class ModelInfo
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsed { get; set; }
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public ModelEvaluation Evaluation { get; set; } = new();
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ModelComparison
{
    public List<string> ModelVersions { get; set; } = new();
    public Dictionary<string, ModelEvaluation> Evaluations { get; set; } = new();
    public string BestModel { get; set; } = string.Empty;
    public float BestScore { get; set; }
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
}

public class ModelPerformance
{
    public string ModelVersion { get; set; } = string.Empty;
    public int TotalPredictions { get; set; }
    public int CorrectPredictions { get; set; }
    public float Accuracy { get; set; }
    public float AverageConfidence { get; set; }
    public float AveragePredictionTime { get; set; }
    public Dictionary<string, int> PredictionsByClass { get; set; } = new();
    public Dictionary<string, float> ConfidenceByClass { get; set; } = new();
    public DateTime LastUsed { get; set; }
    public DateTime PerformanceCalculatedAt { get; set; } = DateTime.UtcNow;
    public List<PerformanceMetric> Metrics { get; set; } = new();
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public float Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class TrainingProgress
{
    public string TrainingId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public float Progress { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public List<string> Logs { get; set; } = new();
}

public class TrainingJob
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public TrainingOptions Options { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public TrainingResult Result { get; set; } = new();
    public List<string> Logs { get; set; } = new();
}

public class AugmentationOptions
{
    public bool UseTextAugmentation { get; set; } = true;
    public bool UsePositionAugmentation { get; set; } = true;
    public bool UseNoiseAugmentation { get; set; } = true;
    public int AugmentationFactor { get; set; } = 2;
    public float NoiseLevel { get; set; } = 0.1f;
    public float PositionVariance { get; set; } = 0.05f;
    public bool UseSyntheticData { get; set; } = false;
    public int SyntheticSamples { get; set; } = 100;
}

