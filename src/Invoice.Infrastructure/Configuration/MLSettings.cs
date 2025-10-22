namespace Invoice.Infrastructure.Configuration;

public class MLSettings
{
    public string ModelVersion { get; set; } = "v1.0";
    public float ConfidenceThreshold { get; set; } = 0.7f;
    public TrainingDataSplit TrainingDataSplit { get; set; } = new();
    public FeatureSettings Features { get; set; } = new();
}

public class TrainingDataSplit
{
    public float TrainPercentage { get; set; } = 0.8f;
    public float ValidationPercentage { get; set; } = 0.1f;
    public float TestPercentage { get; set; } = 0.1f;
}

public class FeatureSettings
{
    public bool UseRegexFeatures { get; set; } = true;
    public bool UsePositionFeatures { get; set; } = true;
    public bool UseContextFeatures { get; set; } = true;
}

