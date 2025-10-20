# Aufgabe 25: Prediction Engine und Model Loading

## Ziel

Prediction Engine Service für ML-Model Loading und Real-time Predictions mit Confidence-Scores.

## 1. Prediction Engine Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IPredictionEngineService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;

namespace InvoiceReader.Application.Interfaces;

public interface IPredictionEngineService
{
    // Model loading
    Task<bool> LoadModelAsync(string version);
    Task<bool> LoadActiveModelAsync();
    Task<bool> ReloadModelAsync();
    Task<List<string>> GetAvailableModelVersionsAsync();
    Task<string> GetActiveModelVersionAsync();

    // Prediction operations
    Task<PredictionResult> PredictAsync(ExtractedFeature feature);
    Task<List<PredictionResult>> PredictAsync(List<ExtractedFeature> features);
    Task<PredictionResult> PredictAsync(NormalizedTextLine textLine, int lineIndex, List<NormalizedTextLine> allLines);
    Task<List<PredictionResult>> PredictAsync(List<NormalizedTextLine> textLines);

    // Batch prediction
    Task<BatchPredictionResult> PredictBatchAsync(List<ExtractedFeature> features);
    Task<BatchPredictionResult> PredictBatchAsync(List<NormalizedTextLine> textLines);

    // Model information
    Task<ModelInfo> GetModelInfoAsync(string version = null);
    Task<ModelPerformance> GetModelPerformanceAsync();
    Task<bool> IsModelLoadedAsync();
    Task<DateTime> GetModelLoadTimeAsync();

    // Confidence and scoring
    Task<ConfidenceAnalysis> AnalyzeConfidenceAsync(PredictionResult prediction);
    Task<List<PredictionResult>> GetTopKPredictionsAsync(ExtractedFeature feature, int k = 3);
    Task<PredictionResult> GetBestPredictionAsync(ExtractedFeature feature);

    // Model switching
    Task<bool> SwitchModelAsync(string version);
    Task<bool> SetActiveModelAsync(string version);
    Task<bool> WarmupModelAsync();
}

public class PredictionResult
{
    public string PredictedLabel { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public float[] Scores { get; set; } = Array.Empty<float>();
    public float[] Probabilities { get; set; } = Array.Empty<float>();
    public List<ClassScore> ClassScores { get; set; } = new();
    public List<ClassScore> Top3Scores { get; set; } = new();
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan PredictionTime { get; set; }
    public bool IsHighConfidence { get; set; }
    public bool IsLowConfidence { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty;
    public ExtractedFeature SourceFeature { get; set; } = null!;
}

public class ClassScore
{
    public string ClassName { get; set; } = string.Empty;
    public float Score { get; set; }
    public float Probability { get; set; }
    public int Rank { get; set; }
}

public class BatchPredictionResult
{
    public List<PredictionResult> Predictions { get; set; } = new();
    public int TotalPredictions { get; set; }
    public int HighConfidencePredictions { get; set; }
    public int LowConfidencePredictions { get; set; }
    public float AverageConfidence { get; set; }
    public float MinConfidence { get; set; }
    public float MaxConfidence { get; set; }
    public TimeSpan TotalPredictionTime { get; set; }
    public float AveragePredictionTime { get; set; }
    public Dictionary<string, int> PredictionsByClass { get; set; } = new();
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
}

public class ConfidenceAnalysis
{
    public float Confidence { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty;
    public bool IsHighConfidence { get; set; }
    public bool IsLowConfidence { get; set; }
    public bool IsMediumConfidence { get; set; }
    public float ConfidenceRank { get; set; }
    public List<ClassScore> AlternativeClasses { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public bool RequiresManualReview { get; set; }
}

public class ModelInfo
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TrainerType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public ModelEvaluation Evaluation { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsed { get; set; }
    public bool IsActive { get; set; }
    public bool IsLoaded { get; set; }
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
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
```

## 2. Prediction Engine Implementation

**Datei:** `src/InvoiceReader.Infrastructure/ML/Services/PredictionEngineService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Infrastructure.ML.Models;
using InvoiceReader.Infrastructure.ML.Converters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InvoiceReader.Infrastructure.ML.Services;

public class PredictionEngineService : IPredictionEngineService
{
    private readonly MLContext _mlContext;
    private readonly IFeatureExtractionService _featureExtractionService;
    private readonly ILogger<PredictionEngineService> _logger;
    private readonly DataViewConverter _dataViewConverter;

    private ITransformer _currentModel;
    private string _currentModelVersion;
    private DateTime _modelLoadTime;
    private bool _isModelLoaded;
    private readonly Dictionary<string, ModelPerformance> _modelPerformance = new();

    public PredictionEngineService(
        MLContext mlContext,
        IFeatureExtractionService featureExtractionService,
        ILogger<PredictionEngineService> logger)
    {
        _mlContext = mlContext;
        _featureExtractionService = featureExtractionService;
        _logger = logger;
        _dataViewConverter = new DataViewConverter(_mlContext, _featureExtractionService);
    }

    public async Task<bool> LoadModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Loading model: {Version}", version);

            var modelPath = Path.Combine("data", "models", $"model_{version}.zip");

            if (!File.Exists(modelPath))
            {
                _logger.LogError("Model file not found: {ModelPath}", modelPath);
                return false;
            }

            _currentModel = _mlContext.Model.Load(modelPath, out var schema);
            _currentModelVersion = version;
            _modelLoadTime = DateTime.UtcNow;
            _isModelLoaded = true;

            _logger.LogInformation("Model loaded successfully: {Version}", version);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model: {Version}", version);
            return false;
        }
    }

    public async Task<bool> LoadActiveModelAsync()
    {
        try
        {
            _logger.LogInformation("Loading active model");

            var modelsDirectory = Path.Combine("data", "models");
            if (!Directory.Exists(modelsDirectory))
            {
                _logger.LogWarning("Models directory not found: {ModelsDirectory}", modelsDirectory);
                return false;
            }

            var metadataFiles = Directory.GetFiles(modelsDirectory, "metadata_*.json");
            string activeVersion = null;

            foreach (var metadataFile in metadataFiles)
            {
                try
                {
                    var metadataJson = await File.ReadAllTextAsync(metadataFile);
                    var metadata = JsonSerializer.Deserialize<ModelMetadata>(metadataJson);

                    if (metadata.IsActive)
                    {
                        activeVersion = metadata.Version;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read metadata file: {MetadataFile}", metadataFile);
                }
            }

            if (string.IsNullOrEmpty(activeVersion))
            {
                _logger.LogWarning("No active model found");
                return false;
            }

            return await LoadModelAsync(activeVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active model");
            return false;
        }
    }

    public async Task<bool> ReloadModelAsync()
    {
        try
        {
            _logger.LogInformation("Reloading current model: {Version}", _currentModelVersion);

            if (string.IsNullOrEmpty(_currentModelVersion))
            {
                _logger.LogWarning("No current model to reload");
                return false;
            }

            return await LoadModelAsync(_currentModelVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload model");
            return false;
        }
    }

    public async Task<List<string>> GetAvailableModelVersionsAsync()
    {
        try
        {
            var versions = new List<string>();
            var modelsDirectory = Path.Combine("data", "models");

            if (!Directory.Exists(modelsDirectory))
            {
                return versions;
            }

            var modelFiles = Directory.GetFiles(modelsDirectory, "model_*.zip");

            foreach (var modelFile in modelFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(modelFile);
                var version = fileName.Replace("model_", "");
                versions.Add(version);
            }

            return versions.OrderByDescending(v => v).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available model versions");
            return new List<string>();
        }
    }

    public async Task<string> GetActiveModelVersionAsync()
    {
        return _currentModelVersion ?? string.Empty;
    }

    public async Task<PredictionResult> PredictAsync(ExtractedFeature feature)
    {
        try
        {
            if (!_isModelLoaded)
            {
                throw new InvalidOperationException("No model loaded");
            }

            var startTime = DateTime.UtcNow;

            // Convert feature to InputRow
            var inputRow = _dataViewConverter.ConvertToInputRow(feature);

            // Create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<InputRow, InputRowPrediction>(_currentModel);

            // Make prediction
            var prediction = predictionEngine.Predict(inputRow);

            var predictionTime = DateTime.UtcNow - startTime;

            // Convert to PredictionResult
            var result = ConvertToPredictionResult(prediction, feature, predictionTime);

            // Update performance metrics
            UpdateModelPerformance(result);

            _logger.LogDebug("Prediction completed: {PredictedLabel}, Confidence: {Confidence}, Time: {PredictionTime}ms",
                result.PredictedLabel, result.Confidence, predictionTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make prediction");
            throw;
        }
    }

    public async Task<List<PredictionResult>> PredictAsync(List<ExtractedFeature> features)
    {
        try
        {
            _logger.LogInformation("Making batch predictions for {Count} features", features.Count);

            var results = new List<PredictionResult>();
            var startTime = DateTime.UtcNow;

            foreach (var feature in features)
            {
                try
                {
                    var result = await PredictAsync(feature);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to predict feature: {Text}", feature.Text);
                }
            }

            var totalTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Batch predictions completed: {Count} predictions in {TotalTime}ms",
                results.Count, totalTime.TotalMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make batch predictions");
            throw;
        }
    }

    public async Task<PredictionResult> PredictAsync(NormalizedTextLine textLine, int lineIndex, List<NormalizedTextLine> allLines)
    {
        try
        {
            // Extract features from text line
            var feature = await _featureExtractionService.ExtractFeatureFromLineAsync(textLine, lineIndex);

            // Make prediction
            return await PredictAsync(feature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict text line: {Text}", textLine.Text);
            throw;
        }
    }

    public async Task<List<PredictionResult>> PredictAsync(List<NormalizedTextLine> textLines)
    {
        try
        {
            var results = new List<PredictionResult>();

            for (int i = 0; i < textLines.Count; i++)
            {
                try
                {
                    var result = await PredictAsync(textLines[i], i, textLines);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to predict text line {Index}: {Text}", i, textLines[i].Text);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make predictions for text lines");
            throw;
        }
    }

    public async Task<BatchPredictionResult> PredictBatchAsync(List<ExtractedFeature> features)
    {
        try
        {
            _logger.LogInformation("Making batch predictions for {Count} features", features.Count);

            var startTime = DateTime.UtcNow;
            var predictions = await PredictAsync(features);
            var totalTime = DateTime.UtcNow - startTime;

            var result = new BatchPredictionResult
            {
                Predictions = predictions,
                TotalPredictions = predictions.Count,
                HighConfidencePredictions = predictions.Count(p => p.IsHighConfidence),
                LowConfidencePredictions = predictions.Count(p => p.IsLowConfidence),
                AverageConfidence = predictions.Average(p => p.Confidence),
                MinConfidence = predictions.Min(p => p.Confidence),
                MaxConfidence = predictions.Max(p => p.Confidence),
                TotalPredictionTime = totalTime,
                AveragePredictionTime = (float)totalTime.TotalMilliseconds / predictions.Count,
                ModelVersion = _currentModelVersion,
                PredictedAt = DateTime.UtcNow
            };

            // Calculate predictions by class
            foreach (var prediction in predictions)
            {
                if (!result.PredictionsByClass.ContainsKey(prediction.PredictedLabel))
                {
                    result.PredictionsByClass[prediction.PredictedLabel] = 0;
                }
                result.PredictionsByClass[prediction.PredictedLabel]++;
            }

            _logger.LogInformation("Batch predictions completed: {TotalPredictions} predictions, {HighConfidence} high confidence, {LowConfidence} low confidence",
                result.TotalPredictions, result.HighConfidencePredictions, result.LowConfidencePredictions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make batch predictions");
            throw;
        }
    }

    public async Task<BatchPredictionResult> PredictBatchAsync(List<NormalizedTextLine> textLines)
    {
        try
        {
            var features = new List<ExtractedFeature>();

            for (int i = 0; i < textLines.Count; i++)
            {
                var feature = await _featureExtractionService.ExtractFeatureFromLineAsync(textLines[i], i);
                features.Add(feature);
            }

            return await PredictBatchAsync(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make batch predictions for text lines");
            throw;
        }
    }

    public async Task<ModelInfo> GetModelInfoAsync(string version = null)
    {
        try
        {
            var targetVersion = version ?? _currentModelVersion;

            if (string.IsNullOrEmpty(targetVersion))
            {
                return new ModelInfo();
            }

            var metadataPath = Path.Combine("data", "models", $"metadata_{targetVersion}.json");

            if (!File.Exists(metadataPath))
            {
                _logger.LogWarning("Metadata file not found: {MetadataPath}", metadataPath);
                return new ModelInfo { Version = targetVersion };
            }

            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<ModelMetadata>(metadataJson);

            return new ModelInfo
            {
                Version = metadata.Version,
                Name = metadata.Name,
                Description = metadata.Description,
                TrainerType = metadata.TrainerType,
                Parameters = metadata.Parameters,
                Evaluation = metadata.Evaluation,
                CreatedAt = metadata.CreatedAt,
                LastUsed = metadata.LastUsed,
                IsActive = metadata.IsActive,
                IsLoaded = _isModelLoaded && _currentModelVersion == targetVersion,
                FileSize = metadata.FileSize,
                FilePath = metadata.FilePath,
                Tags = metadata.Tags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model info: {Version}", version);
            return new ModelInfo();
        }
    }

    public async Task<ModelPerformance> GetModelPerformanceAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentModelVersion))
            {
                return new ModelPerformance();
            }

            if (_modelPerformance.ContainsKey(_currentModelVersion))
            {
                return _modelPerformance[_currentModelVersion];
            }

            return new ModelPerformance
            {
                ModelVersion = _currentModelVersion,
                LastUsed = _modelLoadTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model performance");
            return new ModelPerformance();
        }
    }

    public async Task<bool> IsModelLoadedAsync()
    {
        return _isModelLoaded;
    }

    public async Task<DateTime> GetModelLoadTimeAsync()
    {
        return _modelLoadTime;
    }

    public async Task<ConfidenceAnalysis> AnalyzeConfidenceAsync(PredictionResult prediction)
    {
        try
        {
            var analysis = new ConfidenceAnalysis
            {
                Confidence = prediction.Confidence,
                ConfidenceLevel = prediction.ConfidenceLevel,
                IsHighConfidence = prediction.IsHighConfidence,
                IsLowConfidence = prediction.IsLowConfidence,
                IsMediumConfidence = !prediction.IsHighConfidence && !prediction.IsLowConfidence,
                ConfidenceRank = CalculateConfidenceRank(prediction),
                AlternativeClasses = prediction.ClassScores.Where(cs => cs.ClassName != prediction.PredictedLabel).Take(3).ToList(),
                RequiresManualReview = prediction.IsLowConfidence || prediction.Confidence < 0.5f
            };

            // Generate recommendation
            analysis.Recommendation = GenerateRecommendation(analysis);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze confidence");
            return new ConfidenceAnalysis();
        }
    }

    public async Task<List<PredictionResult>> GetTopKPredictionsAsync(ExtractedFeature feature, int k = 3)
    {
        try
        {
            var prediction = await PredictAsync(feature);

            // Return top K predictions based on scores
            var topKPredictions = prediction.ClassScores
                .OrderByDescending(cs => cs.Score)
                .Take(k)
                .Select(cs => new PredictionResult
                {
                    PredictedLabel = cs.ClassName,
                    Confidence = cs.Score,
                    Scores = prediction.Scores,
                    Probabilities = prediction.Probabilities,
                    ClassScores = prediction.ClassScores,
                    Top3Scores = prediction.Top3Scores,
                    ModelVersion = prediction.ModelVersion,
                    PredictedAt = prediction.PredictedAt,
                    PredictionTime = prediction.PredictionTime,
                    IsHighConfidence = cs.Score > 0.8f,
                    IsLowConfidence = cs.Score < 0.3f,
                    ConfidenceLevel = GetConfidenceLevel(cs.Score),
                    SourceFeature = prediction.SourceFeature
                })
                .ToList();

            return topKPredictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top K predictions");
            return new List<PredictionResult>();
        }
    }

    public async Task<PredictionResult> GetBestPredictionAsync(ExtractedFeature feature)
    {
        try
        {
            var prediction = await PredictAsync(feature);

            // Return the best prediction (highest confidence)
            var bestClass = prediction.ClassScores
                .OrderByDescending(cs => cs.Score)
                .FirstOrDefault();

            if (bestClass != null)
            {
                prediction.PredictedLabel = bestClass.ClassName;
                prediction.Confidence = bestClass.Score;
            }

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get best prediction");
            throw;
        }
    }

    public async Task<bool> SwitchModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Switching to model: {Version}", version);

            var success = await LoadModelAsync(version);

            if (success)
            {
                _logger.LogInformation("Model switched successfully: {Version}", version);
            }
            else
            {
                _logger.LogError("Failed to switch to model: {Version}", version);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch model: {Version}", version);
            return false;
        }
    }

    public async Task<bool> SetActiveModelAsync(string version)
    {
        try
        {
            _logger.LogInformation("Setting active model: {Version}", version);

            // Load the model first
            var success = await LoadModelAsync(version);

            if (success)
            {
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
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set active model: {Version}", version);
            return false;
        }
    }

    public async Task<bool> WarmupModelAsync()
    {
        try
        {
            _logger.LogInformation("Warming up model: {Version}", _currentModelVersion);

            if (!_isModelLoaded)
            {
                _logger.LogWarning("No model loaded for warmup");
                return false;
            }

            // Create a dummy feature for warmup
            var dummyFeature = new ExtractedFeature
            {
                Text = "Warmup",
                LineIndex = 0,
                PageNumber = 1
            };

            // Make a prediction to warm up the model
            await PredictAsync(dummyFeature);

            _logger.LogInformation("Model warmup completed: {Version}", _currentModelVersion);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to warmup model");
            return false;
        }
    }

    private PredictionResult ConvertToPredictionResult(InputRowPrediction prediction, ExtractedFeature feature, TimeSpan predictionTime)
    {
        var classScores = new List<ClassScore>();
        var classNames = new[] { "None", "InvoiceNumber", "InvoiceDate", "IssuerAddress", "NetTotal", "VatTotal", "GrossTotal" };

        for (int i = 0; i < prediction.Score.Length && i < classNames.Length; i++)
        {
            classScores.Add(new ClassScore
            {
                ClassName = classNames[i],
                Score = prediction.Score[i],
                Probability = prediction.Probability[i],
                Rank = i + 1
            });
        }

        var top3Scores = classScores.OrderByDescending(cs => cs.Score).Take(3).ToList();

        return new PredictionResult
        {
            PredictedLabel = prediction.PredictedLabel,
            Confidence = prediction.Confidence,
            Scores = prediction.Score,
            Probabilities = prediction.Probability,
            ClassScores = classScores,
            Top3Scores = top3Scores,
            ModelVersion = _currentModelVersion,
            PredictedAt = DateTime.UtcNow,
            PredictionTime = predictionTime,
            IsHighConfidence = prediction.Confidence > 0.8f,
            IsLowConfidence = prediction.Confidence < 0.3f,
            ConfidenceLevel = GetConfidenceLevel(prediction.Confidence),
            SourceFeature = feature
        };
    }

    private string GetConfidenceLevel(float confidence)
    {
        return confidence switch
        {
            >= 0.8f => "High",
            >= 0.5f => "Medium",
            _ => "Low"
        };
    }

    private float CalculateConfidenceRank(PredictionResult prediction)
    {
        // Calculate confidence rank based on all class scores
        var sortedScores = prediction.ClassScores.OrderByDescending(cs => cs.Score).ToList();
        var predictedIndex = sortedScores.FindIndex(cs => cs.ClassName == prediction.PredictedLabel);
        return predictedIndex >= 0 ? (float)(sortedScores.Count - predictedIndex) / sortedScores.Count : 0f;
    }

    private string GenerateRecommendation(ConfidenceAnalysis analysis)
    {
        if (analysis.IsHighConfidence)
        {
            return "High confidence prediction - can be used directly";
        }
        else if (analysis.IsMediumConfidence)
        {
            return "Medium confidence prediction - review recommended";
        }
        else
        {
            return "Low confidence prediction - manual review required";
        }
    }

    private void UpdateModelPerformance(PredictionResult prediction)
    {
        if (!_modelPerformance.ContainsKey(_currentModelVersion))
        {
            _modelPerformance[_currentModelVersion] = new ModelPerformance
            {
                ModelVersion = _currentModelVersion,
                LastUsed = DateTime.UtcNow
            };
        }

        var performance = _modelPerformance[_currentModelVersion];
        performance.TotalPredictions++;
        performance.LastUsed = DateTime.UtcNow;

        if (!performance.PredictionsByClass.ContainsKey(prediction.PredictedLabel))
        {
            performance.PredictionsByClass[prediction.PredictedLabel] = 0;
        }
        performance.PredictionsByClass[prediction.PredictedLabel]++;

        // Update average confidence
        performance.AverageConfidence = (performance.AverageConfidence * (performance.TotalPredictions - 1) + prediction.Confidence) / performance.TotalPredictions;
    }
}
```

## 3. Prediction Engine Extensions

**Datei:** `src/InvoiceReader.Infrastructure/ML/Extensions/PredictionEngineExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.ML.Extensions;

public static class PredictionEngineExtensions
{
    public static IServiceCollection AddPredictionEngineServices(this IServiceCollection services)
    {
        services.AddScoped<IPredictionEngineService, PredictionEngineService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Prediction Engine Service für ML.NET
- Real-time Predictions mit Confidence-Scores
- Model Loading und Switching
- Batch Prediction für Performance
- Confidence Analysis für Quality Control
- Model Performance Tracking
- Error Handling für alle Prediction-Operationen
- Logging für alle ML-Prediction-Operationen
- Top-K Predictions für Alternative-Klassen
- Model Warmup für Performance-Optimierung
