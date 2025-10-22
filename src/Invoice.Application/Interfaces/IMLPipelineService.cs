using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Application.Models;

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

