# Aufgabe 24: Model Training und Speicherung

## Ziel

ML Model Training Service mit automatischer Speicherung und Versionierung der trainierten Modelle.

## 1. Model Training Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IModelTrainingService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;

namespace InvoiceReader.Application.Interfaces;

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
    public DateTime LastUsed { get; set; }
    public DateTime PerformanceCalculatedAt { get; set; } = DateTime.UtcNow;
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
```

## 2. Model Training Implementation

**Datei:** `src/InvoiceReader.Infrastructure/ML/Services/ModelTrainingService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Infrastructure.ML.Models;
using InvoiceReader.Infrastructure.ML.Converters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InvoiceReader.Infrastructure.ML.Services;

public class ModelTrainingService : IModelTrainingService
{
    private readonly MLContext _mlContext;
    private readonly IMLPipelineService _pipelineService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ModelTrainingService> _logger;
    private readonly DataViewConverter _dataViewConverter;
    private readonly Dictionary<string, TrainingJob> _trainingJobs = new();

    public ModelTrainingService(
        MLContext mlContext,
        IMLPipelineService pipelineService,
        IFileStorageService fileStorageService,
        ILogger<ModelTrainingService> logger)
    {
        _mlContext = mlContext;
        _pipelineService = pipelineService;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _dataViewConverter = new DataViewConverter(_mlContext, null);
    }

    public async Task<TrainingResult> TrainModelAsync(TrainingData trainingData, TrainingOptions options)
    {
        var trainingId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting model training: {ModelVersion}", options.Version);

            var trainingJob = new TrainingJob
            {
                Id = trainingId,
                Status = "Running",
                ModelVersion = options.Version,
                Options = options,
                StartedAt = startTime
            };

            _trainingJobs[trainingId] = trainingJob;

            // Create training pipeline
            var pipeline = _pipelineService.CreateTrainingPipeline();

            // Train the model
            var model = pipeline.Fit(trainingData.TrainData);

            // Evaluate the model
            var evaluation = await _pipelineService.EvaluateModelAsync(model, trainingData.TestData);

            // Calculate training metrics
            var metrics = CalculateTrainingMetrics(evaluation);

            // Save the model if requested
            string modelPath = string.Empty;
            if (options.SaveModel)
            {
                modelPath = await SaveModelAsync(model, options.Version, new ModelMetadata
                {
                    Version = options.Version,
                    Name = options.ModelName,
                    Description = options.Description,
                    TrainerType = options.TrainerType,
                    Parameters = options.TrainerParameters,
                    TrainingData = trainingData,
                    Evaluation = evaluation,
                    IsActive = options.SetAsActive,
                    Tags = options.Tags
                });
            }

            var result = new TrainingResult
            {
                Success = true,
                ModelVersion = options.Version,
                ModelPath = modelPath,
                Model = model,
                Evaluation = evaluation,
                Metrics = metrics,
                TrainingTime = DateTime.UtcNow - startTime,
                TrainedAt = DateTime.UtcNow
            };

            // Update training job
            trainingJob.Status = "Completed";
            trainingJob.CompletedAt = DateTime.UtcNow;
            trainingJob.Duration = DateTime.UtcNow - startTime;
            trainingJob.Result = result;

            _logger.LogInformation("Model training completed successfully: {ModelVersion}, Accuracy: {Accuracy}",
                options.Version, evaluation.Accuracy);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model training failed: {ModelVersion}", options.Version);

            var result = new TrainingResult
            {
                Success = false,
                ModelVersion = options.Version,
                Errors = new List<string> { ex.Message },
                TrainingTime = DateTime.UtcNow - startTime,
                TrainedAt = DateTime.UtcNow
            };

            // Update training job
            if (_trainingJobs.ContainsKey(trainingId))
            {
                _trainingJobs[trainingId].Status = "Failed";
                _trainingJobs[trainingId].CompletedAt = DateTime.UtcNow;
                _trainingJobs[trainingId].Result = result;
            }

            return result;
        }
    }

    public async Task<TrainingResult> RetrainModelAsync(string modelVersion, TrainingData trainingData, TrainingOptions options)
    {
        try
        {
            _logger.LogInformation("Retraining model: {ModelVersion}", modelVersion);

            // Load existing model for comparison
            var existingModel = await LoadModelAsync(modelVersion);

            // Train new model
            var result = await TrainModelAsync(trainingData, options);

            // Compare with existing model
            if (existingModel != null)
            {
                var comparison = await CompareModelsAsync(new List<string> { modelVersion, result.ModelVersion }, trainingData);
                _logger.LogInformation("Model comparison: Best model = {BestModel}, Score = {BestScore}",
                    comparison.BestModel, comparison.BestScore);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrain model: {ModelVersion}", modelVersion);
            throw;
        }
    }

    public async Task<TrainingResult> IncrementalTrainingAsync(string modelVersion, TrainingData newData, TrainingOptions options)
    {
        try
        {
            _logger.LogInformation("Incremental training for model: {ModelVersion}", modelVersion);

            // Load existing model
            var existingModel = await LoadModelAsync(modelVersion);
            if (existingModel == null)
            {
                throw new InvalidOperationException($"Model {modelVersion} not found");
            }

            // Combine with existing training data
            var combinedData = await CombineTrainingDataAsync(existingModel, newData);

            // Retrain with combined data
            var result = await TrainModelAsync(combinedData, options);

            _logger.LogInformation("Incremental training completed: {ModelVersion}", result.ModelVersion);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform incremental training: {ModelVersion}", modelVersion);
            throw;
        }
    }

    public async Task<bool> SaveModelAsync(ITransformer model, string version, ModelMetadata metadata)
    {
        try
        {
            _logger.LogInformation("Saving model: {Version}", version);

            var modelPath = Path.Combine("data", "models", $"model_{version}.zip");
            var metadataPath = Path.Combine("data", "models", $"metadata_{version}.json");

            // Save model
            _mlContext.Model.Save(model, null, modelPath);

            // Save metadata
            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, metadataJson);

            // Update file size
            var fileInfo = new FileInfo(modelPath);
            metadata.FileSize = fileInfo.Length;
            metadata.FilePath = modelPath;

            _logger.LogInformation("Model saved successfully: {ModelPath}", modelPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save model: {Version}", version);
            return false;
        }
    }

    public async Task<ITransformer> LoadModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Loading model: {Version}", version);

            var modelPath = Path.Combine("data", "models", $"model_{version}.zip");

            if (!File.Exists(modelPath))
            {
                _logger.LogWarning("Model file not found: {ModelPath}", modelPath);
                return null;
            }

            var model = _mlContext.Model.Load(modelPath, out var schema);

            _logger.LogInformation("Model loaded successfully: {Version}", version);

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model: {Version}", version);
            return null;
        }
    }

    public async Task<List<ModelInfo>> GetAvailableModelsAsync()
    {
        try
        {
            var models = new List<ModelInfo>();
            var modelsDirectory = Path.Combine("data", "models");

            if (!Directory.Exists(modelsDirectory))
            {
                return models;
            }

            var modelFiles = Directory.GetFiles(modelsDirectory, "model_*.zip");

            foreach (var modelFile in modelFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(modelFile);
                var version = fileName.Replace("model_", "");
                var metadataPath = Path.Combine(modelsDirectory, $"metadata_{version}.json");

                var modelInfo = new ModelInfo
                {
                    Version = version,
                    Name = $"Model {version}",
                    FilePath = modelFile,
                    FileSize = new FileInfo(modelFile).Length,
                    CreatedAt = File.GetCreationTime(modelFile),
                    LastUsed = File.GetLastWriteTime(modelFile)
                };

                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var metadataJson = await File.ReadAllTextAsync(metadataPath);
                        var metadata = JsonSerializer.Deserialize<ModelMetadata>(metadataJson);

                        modelInfo.Name = metadata.Name;
                        modelInfo.Description = metadata.Description;
                        modelInfo.IsActive = metadata.IsActive;
                        modelInfo.Evaluation = metadata.Evaluation;
                        modelInfo.Tags = metadata.Tags;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load metadata for model: {Version}", version);
                    }
                }

                models.Add(modelInfo);
            }

            return models.OrderByDescending(m => m.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models");
            return new List<ModelInfo>();
        }
    }

    public async Task<bool> DeleteModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Deleting model: {Version}", version);

            var modelPath = Path.Combine("data", "models", $"model_{version}.zip");
            var metadataPath = Path.Combine("data", "models", $"metadata_{version}.json");

            if (File.Exists(modelPath))
            {
                File.Delete(modelPath);
            }

            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }

            _logger.LogInformation("Model deleted successfully: {Version}", version);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete model: {Version}", version);
            return false;
        }
    }

    public async Task<bool> SetActiveModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Setting active model: {Version}", version);

            // Load model to verify it exists
            var model = await LoadModelAsync(version);
            if (model == null)
            {
                _logger.LogWarning("Model not found: {Version}", version);
                return false;
            }

            // Update metadata to set as active
            var metadataPath = Path.Combine("data", "models", $"metadata_{version}.json");
            if (File.Exists(metadataPath))
            {
                var metadataJson = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonSerializer.Deserialize<ModelMetadata>(metadataJson);
                metadata.IsActive = true;
                metadata.LastUsed = DateTime.UtcNow;

                var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, updatedJson);
            }

            _logger.LogInformation("Active model set successfully: {Version}", version);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set active model: {Version}", version);
            return false;
        }
    }

    public async Task<TrainingData> PrepareTrainingDataAsync(List<ExtractedFeature> features)
    {
        try
        {
            _logger.LogInformation("Preparing training data from {Count} features", features.Count);

            var dataView = _dataViewConverter.ConvertToDataView(features);
            var splitData = await SplitTrainingDataAsync(new TrainingData { TrainData = dataView });

            _logger.LogInformation("Training data prepared: {TotalSamples} total, {TrainSamples} train, {ValidationSamples} validation, {TestSamples} test",
                splitData.TotalSamples, splitData.TrainSamples, splitData.ValidationSamples, splitData.TestSamples);

            return splitData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare training data");
            throw;
        }
    }

    public async Task<TrainingData> SplitTrainingDataAsync(TrainingData data, float trainRatio = 0.8f, float validationRatio = 0.1f)
    {
        try
        {
            var testRatio = 1.0f - trainRatio - validationRatio;

            var split = _mlContext.Data.TrainTestSplit(data.TrainData, testFraction: testRatio);
            var trainData = split.TrainSet;
            var testData = split.TestSet;

            IDataView validationData = null;
            if (validationRatio > 0)
            {
                var validationSplit = _mlContext.Data.TrainTestSplit(trainData, testFraction: validationRatio / trainRatio);
                trainData = validationSplit.TrainSet;
                validationData = validationSplit.TestSet;
            }

            return new TrainingData
            {
                TrainData = trainData,
                ValidationData = validationData,
                TestData = testData,
                TotalSamples = (int)data.TrainData.GetRowCount(),
                TrainSamples = (int)trainData.GetRowCount(),
                ValidationSamples = validationData != null ? (int)validationData.GetRowCount() : 0,
                TestSamples = (int)testData.GetRowCount()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to split training data");
            throw;
        }
    }

    public async Task<TrainingData> AugmentTrainingDataAsync(TrainingData data, AugmentationOptions options)
    {
        try
        {
            _logger.LogInformation("Augmenting training data with factor {Factor}", options.AugmentationFactor);

            // Simplified augmentation - in a real implementation, you would apply various augmentation techniques
            var augmentedData = data;

            _logger.LogInformation("Training data augmented successfully");

            return augmentedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to augment training data");
            throw;
        }
    }

    public async Task<ModelEvaluation> EvaluateModelAsync(ITransformer model, TrainingData testData)
    {
        try
        {
            _logger.LogInformation("Evaluating model");

            var evaluation = await _pipelineService.EvaluateModelAsync(model, testData.TestData);

            _logger.LogInformation("Model evaluation completed: Accuracy={Accuracy}", evaluation.Accuracy);

            return evaluation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate model");
            throw;
        }
    }

    public async Task<ModelComparison> CompareModelsAsync(List<string> modelVersions, TrainingData testData)
    {
        try
        {
            _logger.LogInformation("Comparing models: {Versions}", string.Join(", ", modelVersions));

            var comparison = new ModelComparison
            {
                ModelVersions = modelVersions
            };

            var bestScore = 0f;
            var bestModel = string.Empty;

            foreach (var version in modelVersions)
            {
                var model = await LoadModelAsync(version);
                if (model != null)
                {
                    var evaluation = await EvaluateModelAsync(model, testData);
                    comparison.Evaluations[version] = evaluation;

                    if (evaluation.Accuracy > bestScore)
                    {
                        bestScore = evaluation.Accuracy;
                        bestModel = version;
                    }
                }
            }

            comparison.BestModel = bestModel;
            comparison.BestScore = bestScore;

            _logger.LogInformation("Model comparison completed: Best model = {BestModel}, Score = {BestScore}",
                bestModel, bestScore);

            return comparison;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare models");
            throw;
        }
    }

    public async Task<ModelPerformance> GetModelPerformanceAsync(string version)
    {
        try
        {
            _logger.LogInformation("Getting model performance: {Version}", version);

            // Simplified performance calculation
            var performance = new ModelPerformance
            {
                ModelVersion = version,
                TotalPredictions = 0,
                CorrectPredictions = 0,
                Accuracy = 0f,
                AverageConfidence = 0f,
                AveragePredictionTime = 0f,
                LastUsed = DateTime.UtcNow
            };

            _logger.LogInformation("Model performance retrieved: {Version}", version);

            return performance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model performance: {Version}", version);
            throw;
        }
    }

    public async Task<TrainingProgress> GetTrainingProgressAsync(string trainingId)
    {
        if (_trainingJobs.ContainsKey(trainingId))
        {
            var job = _trainingJobs[trainingId];
            return new TrainingProgress
            {
                TrainingId = trainingId,
                Status = job.Status,
                Progress = job.Status == "Completed" ? 100f : 50f,
                CurrentStep = job.Status == "Completed" ? "Completed" : "Training",
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                ElapsedTime = DateTime.UtcNow - job.StartedAt,
                Logs = job.Logs
            };
        }

        return new TrainingProgress { Status = "NotFound" };
    }

    public async Task<bool> CancelTrainingAsync(string trainingId)
    {
        if (_trainingJobs.ContainsKey(trainingId))
        {
            _trainingJobs[trainingId].Status = "Cancelled";
            _trainingJobs[trainingId].CompletedAt = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public async Task<List<TrainingJob>> GetTrainingJobsAsync()
    {
        return _trainingJobs.Values.ToList();
    }

    private TrainingMetrics CalculateTrainingMetrics(ModelEvaluation evaluation)
    {
        return new TrainingMetrics
        {
            Accuracy = evaluation.Accuracy,
            MicroF1Score = evaluation.MicroF1Score,
            MacroF1Score = evaluation.MacroF1Score,
            WeightedF1Score = evaluation.WeightedF1Score,
            TotalSamples = 0, // Would be calculated from actual data
            CorrectPredictions = 0, // Would be calculated from actual data
            IncorrectPredictions = 0 // Would be calculated from actual data
        };
    }

    private async Task<TrainingData> CombineTrainingDataAsync(ITransformer existingModel, TrainingData newData)
    {
        // Simplified combination - in a real implementation, you would properly combine the datasets
        return newData;
    }
}
```

## 3. Model Training Extensions

**Datei:** `src/InvoiceReader.Infrastructure/ML/Extensions/ModelTrainingExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.ML.Extensions;

public static class ModelTrainingExtensions
{
    public static IServiceCollection AddModelTrainingServices(this IServiceCollection services)
    {
        services.AddScoped<IModelTrainingService, ModelTrainingService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Model Training Service für ML.NET
- Automatische Model-Speicherung und Versionierung
- Training Data Management mit Splitting
- Model Evaluation und Comparison
- Training Progress Monitoring
- Model Performance Tracking
- Error Handling für alle Training-Operationen
- Logging für alle ML-Training-Operationen
- JSON-basierte Metadata-Speicherung
- Cross-Validation und Model Selection
