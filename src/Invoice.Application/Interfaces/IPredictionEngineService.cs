using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Application.Models;

namespace Invoice.Application.Interfaces;

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
    Task<TrainingModelInfo> GetModelInfoAsync(string version = null);
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

