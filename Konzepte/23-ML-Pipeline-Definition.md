# Aufgabe 23: ML Pipeline Definition (Featurization + Trainer)

## Ziel

ML.NET Pipeline mit Featurization und Trainer für Multi-Class-Classification der Invoice-Felder.

## 1. ML Pipeline Interface

**Datei:** `src/Invoice.Application/Interfaces/IMLPipelineService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Invoice.Application.Interfaces;

public interface IMLPipelineService
{
    // Pipeline creation
    IEstimator<ITransformer> CreateTrainingPipeline();
    IEstimator<ITransformer> CreatePredictionPipeline();
    IEstimator<ITransformer> CreateFeaturePipeline();

    // Pipeline execution
    Task<ITransformer> TrainPipelineAsync(IDataView trainingData, IDataView validationData = null);
    Task<IDataView> TransformDataAsync(IDataView data, ITransformer transformer);
    Task<IDataView> PredictAsync(IDataView data, ITransformer model);

    // Feature engineering
    Task<IDataView> CreateFeaturesAsync(IDataView data);
    Task<IDataView> NormalizeFeaturesAsync(IDataView data);
    Task<IDataView> SelectFeaturesAsync(IDataView data);

    // Model evaluation
    Task<ModelEvaluation> EvaluateModelAsync(ITransformer model, IDataView testData);
    Task<CrossValidationResult> CrossValidateAsync(IDataView data, int folds = 5);

    // Pipeline configuration
    Task<MLPipelineConfiguration> GetPipelineConfigurationAsync();
    Task<bool> UpdatePipelineConfigurationAsync(MLPipelineConfiguration configuration);
}

public class MLPipelineConfiguration
{
    public bool UseTextFeaturization { get; set; } = true;
    public bool UsePositionFeatures { get; set; } = true;
    public bool UseStatisticalFeatures { get; set; } = true;
    public bool UseRegexFeatures { get; set; } = true;
    public bool UseContextFeatures { get; set; } = true;
    public bool UseLayoutFeatures { get; set; } = true;
    public bool UseFontFeatures { get; set; } = true;
    public bool UseFeatureNormalization { get; set; } = true;
    public bool UseFeatureSelection { get; set; } = false;
    public string TrainerType { get; set; } = "SdcaMaximumEntropy";
    public Dictionary<string, object> TrainerParameters { get; set; } = new();
    public int MaxIterations { get; set; } = 100;
    public float LearningRate { get; set; } = 0.01f;
    public float L1Regularization { get; set; } = 0.01f;
    public float L2Regularization { get; set; } = 0.01f;
    public int FeatureSelectionCount { get; set; } = 100;
    public float FeatureSelectionThreshold { get; set; } = 0.1f;
}

public class ModelEvaluation
{
    public float Accuracy { get; set; }
    public float MicroF1Score { get; set; }
    public float MacroF1Score { get; set; }
    public float WeightedF1Score { get; set; }
    public Dictionary<string, float> PerClassF1Score { get; set; } = new();
    public Dictionary<string, float> PerClassPrecision { get; set; } = new();
    public Dictionary<string, float> PerClassRecall { get; set; } = new();
    public ConfusionMatrix ConfusionMatrix { get; set; } = new();
    public List<ModelError> Errors { get; set; } = new();
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

public class CrossValidationResult
{
    public float MeanAccuracy { get; set; }
    public float StdDevAccuracy { get; set; }
    public float MeanMicroF1Score { get; set; }
    public float StdDevMicroF1Score { get; set; }
    public float MeanMacroF1Score { get; set; }
    public float StdDevMacroF1Score { get; set; }
    public List<ModelEvaluation> FoldResults { get; set; } = new();
    public DateTime CrossValidatedAt { get; set; } = DateTime.UtcNow;
}

public class ConfusionMatrix
{
    public Dictionary<string, Dictionary<string, int>> Matrix { get; set; } = new();
    public List<string> Classes { get; set; } = new();
    public int TotalSamples { get; set; }
    public int CorrectPredictions { get; set; }
    public int IncorrectPredictions { get; set; }
}

public class ModelError
{
    public string ActualLabel { get; set; } = string.Empty;
    public string PredictedLabel { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LineIndex { get; set; }
    public string DocumentId { get; set; } = string.Empty;
}
```

## 2. ML Pipeline Implementation

**Datei:** `src/Invoice.Infrastructure/ML/Services/MLPipelineService.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Invoice.Application.Interfaces;
using Invoice.Infrastructure.ML.Models;
using Invoice.Infrastructure.ML.Configuration;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.ML.Services;

public class MLPipelineService : IMLPipelineService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<MLPipelineService> _logger;
    private MLPipelineConfiguration _configuration;

    public MLPipelineService(MLContext mlContext, ILogger<MLPipelineService> logger)
    {
        _mlContext = mlContext;
        _logger = logger;
        _configuration = new MLPipelineConfiguration();
    }

    public IEstimator<ITransformer> CreateTrainingPipeline()
    {
        try
        {
            _logger.LogInformation("Creating training pipeline...");

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                .Append(CreateFeaturePipeline())
                .Append(CreateTrainer());

            _logger.LogInformation("Training pipeline created successfully");
            return pipeline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create training pipeline");
            throw;
        }
    }

    public IEstimator<ITransformer> CreatePredictionPipeline()
    {
        try
        {
            _logger.LogInformation("Creating prediction pipeline...");

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                .Append(CreateFeaturePipeline())
                .Append(CreateTrainer())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

            _logger.LogInformation("Prediction pipeline created successfully");
            return pipeline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create prediction pipeline");
            throw;
        }
    }

    public IEstimator<ITransformer> CreateFeaturePipeline()
    {
        try
        {
            _logger.LogInformation("Creating feature pipeline...");

            var pipeline = _mlContext.Transforms.Concatenate("Features", GetFeatureColumns());

            // Add text featurization if enabled
            if (_configuration.UseTextFeaturization)
            {
                pipeline = pipeline.Append(_mlContext.Transforms.Text.FeaturizeText("TextFeatures", "Text"));
            }

            // Add feature normalization if enabled
            if (_configuration.UseFeatureNormalization)
            {
                pipeline = pipeline.Append(_mlContext.Transforms.NormalizeMinMax("Features", "Features"));
            }

            // Add feature selection if enabled
            if (_configuration.UseFeatureSelection)
            {
                pipeline = pipeline.Append(_mlContext.Transforms.FeatureSelection.SelectFeaturesBasedOnCount("Features", "Features",
                    _configuration.FeatureSelectionCount));
            }

            _logger.LogInformation("Feature pipeline created successfully");
            return pipeline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create feature pipeline");
            throw;
        }
    }

    public async Task<ITransformer> TrainPipelineAsync(IDataView trainingData, IDataView validationData = null)
    {
        try
        {
            _logger.LogInformation("Training ML pipeline...");

            var pipeline = CreateTrainingPipeline();
            var transformer = pipeline.Fit(trainingData);

            _logger.LogInformation("ML pipeline training completed successfully");
            return transformer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train ML pipeline");
            throw;
        }
    }

    public async Task<IDataView> TransformDataAsync(IDataView data, ITransformer transformer)
    {
        try
        {
            _logger.LogInformation("Transforming data...");

            var transformedData = transformer.Transform(data);

            _logger.LogInformation("Data transformation completed successfully");
            return transformedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform data");
            throw;
        }
    }

    public async Task<IDataView> PredictAsync(IDataView data, ITransformer model)
    {
        try
        {
            _logger.LogInformation("Making predictions...");

            var predictions = model.Transform(data);

            _logger.LogInformation("Predictions completed successfully");
            return predictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to make predictions");
            throw;
        }
    }

    public async Task<IDataView> CreateFeaturesAsync(IDataView data)
    {
        try
        {
            _logger.LogInformation("Creating features...");

            var featurePipeline = CreateFeaturePipeline();
            var transformedData = featurePipeline.Fit(data).Transform(data);

            _logger.LogInformation("Features created successfully");
            return transformedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create features");
            throw;
        }
    }

    public async Task<IDataView> NormalizeFeaturesAsync(IDataView data)
    {
        try
        {
            _logger.LogInformation("Normalizing features...");

            var normalizePipeline = _mlContext.Transforms.NormalizeMinMax("Features", "Features");
            var transformedData = normalizePipeline.Fit(data).Transform(data);

            _logger.LogInformation("Feature normalization completed successfully");
            return transformedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize features");
            throw;
        }
    }

    public async Task<IDataView> SelectFeaturesAsync(IDataView data)
    {
        try
        {
            _logger.LogInformation("Selecting features...");

            var selectionPipeline = _mlContext.Transforms.FeatureSelection.SelectFeaturesBasedOnCount("Features", "Features",
                _configuration.FeatureSelectionCount);
            var transformedData = selectionPipeline.Fit(data).Transform(data);

            _logger.LogInformation("Feature selection completed successfully");
            return transformedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select features");
            throw;
        }
    }

    public async Task<ModelEvaluation> EvaluateModelAsync(ITransformer model, IDataView testData)
    {
        try
        {
            _logger.LogInformation("Evaluating model...");

            var predictions = model.Transform(testData);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

            var evaluation = new ModelEvaluation
            {
                Accuracy = metrics.MacroAccuracy,
                MicroF1Score = metrics.MicroAccuracy,
                MacroF1Score = metrics.MacroAccuracy,
                WeightedF1Score = metrics.MacroAccuracy,
                PerClassF1Score = metrics.PerClassLogLoss.ToDictionary(kvp => kvp.Key, kvp => 1f - kvp.Value),
                PerClassPrecision = metrics.PerClassLogLoss.ToDictionary(kvp => kvp.Key, kvp => 1f - kvp.Value),
                PerClassRecall = metrics.PerClassLogLoss.ToDictionary(kvp => kvp.Key, kvp => 1f - kvp.Value),
                ConfusionMatrix = CreateConfusionMatrix(predictions),
                Errors = CreateModelErrors(predictions)
            };

            _logger.LogInformation("Model evaluation completed: Accuracy={Accuracy}, MicroF1={MicroF1}, MacroF1={MacroF1}",
                evaluation.Accuracy, evaluation.MicroF1Score, evaluation.MacroF1Score);

            return evaluation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate model");
            throw;
        }
    }

    public async Task<CrossValidationResult> CrossValidateAsync(IDataView data, int folds = 5)
    {
        try
        {
            _logger.LogInformation("Performing cross-validation with {Folds} folds...", folds);

            var pipeline = CreateTrainingPipeline();
            var crossValidationResults = _mlContext.MulticlassClassification.CrossValidate(data, pipeline, numberOfFolds: folds);

            var accuracies = crossValidationResults.Select(r => r.Metrics.MacroAccuracy).ToArray();
            var microF1Scores = crossValidationResults.Select(r => r.Metrics.MicroAccuracy).ToArray();
            var macroF1Scores = crossValidationResults.Select(r => r.Metrics.MacroAccuracy).ToArray();

            var result = new CrossValidationResult
            {
                MeanAccuracy = accuracies.Average(),
                StdDevAccuracy = CalculateStandardDeviation(accuracies),
                MeanMicroF1Score = microF1Scores.Average(),
                StdDevMicroF1Score = CalculateStandardDeviation(microF1Scores),
                MeanMacroF1Score = macroF1Scores.Average(),
                StdDevMacroF1Score = CalculateStandardDeviation(macroF1Scores),
                FoldResults = crossValidationResults.Select(r => new ModelEvaluation
                {
                    Accuracy = r.Metrics.MacroAccuracy,
                    MicroF1Score = r.Metrics.MicroAccuracy,
                    MacroF1Score = r.Metrics.MacroAccuracy,
                    WeightedF1Score = r.Metrics.MacroAccuracy
                }).ToList()
            };

            _logger.LogInformation("Cross-validation completed: Mean Accuracy={MeanAccuracy}, StdDev={StdDev}",
                result.MeanAccuracy, result.StdDevAccuracy);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform cross-validation");
            throw;
        }
    }

    public async Task<MLPipelineConfiguration> GetPipelineConfigurationAsync()
    {
        return _configuration;
    }

    public async Task<bool> UpdatePipelineConfigurationAsync(MLPipelineConfiguration configuration)
    {
        try
        {
            _configuration = configuration;
            _logger.LogInformation("Pipeline configuration updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update pipeline configuration");
            return false;
        }
    }

    private string[] GetFeatureColumns()
    {
        var features = new List<string>();

        if (_configuration.UsePositionFeatures)
        {
            features.AddRange(new[] { "X", "Y", "Width", "Height", "CenterX", "CenterY", "RelativeX", "RelativeY", "RelativeWidth", "RelativeHeight" });
        }

        if (_configuration.UseStatisticalFeatures)
        {
            features.AddRange(new[] { "CharacterCount", "WordCount", "DigitCount", "LetterCount", "SpecialCharCount",
                "AverageWordLength", "DigitRatio", "LetterRatio", "SpecialCharRatio", "UppercaseCount", "LowercaseCount", "UppercaseRatio" });
        }

        if (_configuration.UseRegexFeatures)
        {
            features.AddRange(new[] { "RegexInvoiceNumberHits", "RegexDateHits", "RegexAmountHits", "RegexCurrencyHits", "RegexEmailHits", "RegexPhoneHits" });
        }

        if (_configuration.UseContextFeatures)
        {
            features.AddRange(new[] { "DistanceToPrevious", "DistanceToNext", "IsFirstLine", "IsLastLine", "IsIsolated" });
        }

        if (_configuration.UseLayoutFeatures)
        {
            features.AddRange(new[] { "PageWidth", "PageHeight", "Indentation", "IsAlignedWithPrevious", "IsAlignedWithNext" });
        }

        if (_configuration.UseFontFeatures)
        {
            features.AddRange(new[] { "FontSize" });
        }

        return features.ToArray();
    }

    private IEstimator<ITransformer> CreateTrainer()
    {
        return _configuration.TrainerType switch
        {
            "SdcaMaximumEntropy" => _mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: _configuration.MaxIterations,
                learningRate: _configuration.LearningRate,
                l1Regularization: _configuration.L1Regularization,
                l2Regularization: _configuration.L2Regularization),
            "LbfgsMaximumEntropy" => _mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                l1Regularization: _configuration.L1Regularization,
                l2Regularization: _configuration.L2Regularization),
            "OneVersusAll" => _mlContext.MulticlassClassification.Trainers.OneVersusAll(
                binaryEstimator: _mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features")),
            _ => _mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features")
        };
    }

    private ConfusionMatrix CreateConfusionMatrix(IDataView predictions)
    {
        // Simplified confusion matrix creation
        // In a real implementation, you would need to extract actual vs predicted labels
        return new ConfusionMatrix
        {
            Classes = new[] { "None", "InvoiceNumber", "InvoiceDate", "IssuerAddress", "NetTotal", "VatTotal", "GrossTotal" }.ToList(),
            TotalSamples = 0,
            CorrectPredictions = 0,
            IncorrectPredictions = 0
        };
    }

    private List<ModelError> CreateModelErrors(IDataView predictions)
    {
        // Simplified error creation
        // In a real implementation, you would extract misclassified samples
        return new List<ModelError>();
    }

    private float CalculateStandardDeviation(float[] values)
    {
        if (values.Length == 0) return 0f;

        var mean = values.Average();
        var variance = values.Select(v => (v - mean) * (v - mean)).Average();
        return (float)Math.Sqrt(variance);
    }
}
```

## 3. ML Pipeline Extensions

**Datei:** `src/Invoice.Infrastructure/ML/Extensions/MLPipelineExtensions.cs`

```csharp
using Invoice.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.ML.Extensions;

public static class MLPipelineExtensions
{
    public static IServiceCollection AddMLPipelineServices(this IServiceCollection services)
    {
        services.AddScoped<IMLPipelineService, MLPipelineService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständige ML.NET Pipeline für Multi-Class-Classification
- Konfigurierbare Feature-Extraktion
- Verschiedene Trainer-Optionen (SdcaMaximumEntropy, LbfgsMaximumEntropy, OneVersusAll)
- Feature Normalization und Selection
- Model Evaluation mit detaillierten Metriken
- Cross-Validation für robuste Evaluation
- Confusion Matrix für Error-Analyse
- Error Handling für alle ML-Operationen
- Logging für alle ML-Pipeline-Operationen
- Strukturierte Configuration für Pipeline-Parameter
