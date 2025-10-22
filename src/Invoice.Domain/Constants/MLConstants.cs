namespace Invoice.Domain.Constants;

public static class MLConstants
{
    // Model Versions
    public const string DefaultModelVersion = "v1.0";
    public const string ManualModelVersion = "manual";
    public const string UnknownModelVersion = "unknown";

    // Training Data
    public const int MinTrainingSamples = 100;
    public const int RecommendedTrainingSamples = 1000;
    public const int MaxTrainingSamples = 10000;

    // Data Splits
    public const float DefaultTrainSplit = 0.8f;
    public const float DefaultValidationSplit = 0.1f;
    public const float DefaultTestSplit = 0.1f;

    // Performance Thresholds
    public const float MinAccuracyThreshold = 0.85f;
    public const float MinPrecisionThreshold = 0.80f;
    public const float MinRecallThreshold = 0.80f;
    public const float MinF1ScoreThreshold = 0.80f;

    // Feature Limits
    public const int MaxTextLength = 4000;
    public const int MaxContextLines = 3;
    public const int MaxRegexHits = 10;

    // Model Storage
    public const string ModelFileExtension = ".zip";
    public const string TrainingDataExtension = ".jsonl";
    public const string LabeledDataExtension = ".tsv";
}

