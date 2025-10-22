using Microsoft.ML.Data;

namespace Invoice.Infrastructure.ML.Models;

public class InputRow
{
    // Text features
    [LoadColumn(0)]
    public string Text { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string NormalizedText { get; set; } = string.Empty;

    [LoadColumn(2)]
    public string LowercaseText { get; set; } = string.Empty;

    // Position features
    [LoadColumn(3)]
    public float X { get; set; }

    [LoadColumn(4)]
    public float Y { get; set; }

    [LoadColumn(5)]
    public float Width { get; set; }

    [LoadColumn(6)]
    public float Height { get; set; }

    [LoadColumn(7)]
    public float CenterX { get; set; }

    [LoadColumn(8)]
    public float CenterY { get; set; }

    [LoadColumn(9)]
    public float RelativeX { get; set; }

    [LoadColumn(10)]
    public float RelativeY { get; set; }

    [LoadColumn(11)]
    public float RelativeWidth { get; set; }

    [LoadColumn(12)]
    public float RelativeHeight { get; set; }

    // Line features
    [LoadColumn(13)]
    public int LineIndex { get; set; }

    [LoadColumn(14)]
    public int TotalLines { get; set; }

    [LoadColumn(15)]
    public float LinePosition { get; set; }

    [LoadColumn(16)]
    public int PageNumber { get; set; }

    // Statistical features
    [LoadColumn(17)]
    public int CharacterCount { get; set; }

    [LoadColumn(18)]
    public int WordCount { get; set; }

    [LoadColumn(19)]
    public int DigitCount { get; set; }

    [LoadColumn(20)]
    public int LetterCount { get; set; }

    [LoadColumn(21)]
    public int SpecialCharCount { get; set; }

    [LoadColumn(22)]
    public float AverageWordLength { get; set; }

    [LoadColumn(23)]
    public float DigitRatio { get; set; }

    [LoadColumn(24)]
    public float LetterRatio { get; set; }

    [LoadColumn(25)]
    public float SpecialCharRatio { get; set; }

    [LoadColumn(26)]
    public int UppercaseCount { get; set; }

    [LoadColumn(27)]
    public int LowercaseCount { get; set; }

    [LoadColumn(28)]
    public float UppercaseRatio { get; set; }

    // Text pattern features
    [LoadColumn(29)]
    public bool ContainsNumbers { get; set; }

    [LoadColumn(30)]
    public bool ContainsCurrency { get; set; }

    [LoadColumn(31)]
    public bool ContainsDate { get; set; }

    [LoadColumn(32)]
    public bool ContainsEmail { get; set; }

    [LoadColumn(33)]
    public bool ContainsPhone { get; set; }

    [LoadColumn(34)]
    public bool StartsWithNumber { get; set; }

    [LoadColumn(35)]
    public bool StartsWithLetter { get; set; }

    [LoadColumn(36)]
    public bool EndsWithPunctuation { get; set; }

    [LoadColumn(37)]
    public bool IsAllUppercase { get; set; }

    [LoadColumn(38)]
    public bool IsAllLowercase { get; set; }

    [LoadColumn(39)]
    public bool IsMixedCase { get; set; }

    // Regex hit counts
    [LoadColumn(40)]
    public int RegexInvoiceNumberHits { get; set; }

    [LoadColumn(41)]
    public int RegexDateHits { get; set; }

    [LoadColumn(42)]
    public int RegexAmountHits { get; set; }

    [LoadColumn(43)]
    public int RegexCurrencyHits { get; set; }

    [LoadColumn(44)]
    public int RegexEmailHits { get; set; }

    [LoadColumn(45)]
    public int RegexPhoneHits { get; set; }

    // Context features
    [LoadColumn(46)]
    public string PreviousLine { get; set; } = string.Empty;

    [LoadColumn(47)]
    public string NextLine { get; set; } = string.Empty;

    [LoadColumn(48)]
    public float DistanceToPrevious { get; set; }

    [LoadColumn(49)]
    public float DistanceToNext { get; set; }

    [LoadColumn(50)]
    public bool IsFirstLine { get; set; }

    [LoadColumn(51)]
    public bool IsLastLine { get; set; }

    [LoadColumn(52)]
    public bool IsIsolated { get; set; }

    // Layout features
    [LoadColumn(53)]
    public float PageWidth { get; set; }

    [LoadColumn(54)]
    public float PageHeight { get; set; }

    [LoadColumn(55)]
    public string Region { get; set; } = string.Empty;

    [LoadColumn(56)]
    public string Alignment { get; set; } = string.Empty;

    [LoadColumn(57)]
    public float Indentation { get; set; }

    [LoadColumn(58)]
    public bool IsAlignedWithPrevious { get; set; }

    [LoadColumn(59)]
    public bool IsAlignedWithNext { get; set; }

    // Font features
    [LoadColumn(60)]
    public float FontSize { get; set; }

    [LoadColumn(61)]
    public string FontName { get; set; } = string.Empty;

    // Target label
    [LoadColumn(62)]
    public string Label { get; set; } = string.Empty;

    // Metadata
    [LoadColumn(63)]
    public string DocumentId { get; set; } = string.Empty;

    [LoadColumn(64)]
    public string ModelVersion { get; set; } = string.Empty;

    [LoadColumn(65)]
    public DateTime CreatedAt { get; set; }
}

public class InputRowPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();

    [ColumnName("Probability")]
    public float[] Probability { get; set; } = Array.Empty<float>();

    [ColumnName("Confidence")]
    public float Confidence { get; set; }

    [ColumnName("Top3Predictions")]
    public string[] Top3Predictions { get; set; } = Array.Empty<string>();

    [ColumnName("Top3Scores")]
    public float[] Top3Scores { get; set; } = Array.Empty<float>();
}

public class InputRowFeatures
{
    [VectorType(100)]
    public float[] TextFeatures { get; set; } = Array.Empty<float>();

    [VectorType(20)]
    public float[] PositionFeatures { get; set; } = Array.Empty<float>();

    [VectorType(15)]
    public float[] StatisticalFeatures { get; set; } = Array.Empty<float>();

    [VectorType(10)]
    public float[] PatternFeatures { get; set; } = Array.Empty<float>();

    [VectorType(6)]
    public float[] RegexFeatures { get; set; } = Array.Empty<float>();

    [VectorType(10)]
    public float[] ContextFeatures { get; set; } = Array.Empty<float>();

    [VectorType(10)]
    public float[] LayoutFeatures { get; set; } = Array.Empty<float>();

    [VectorType(5)]
    public float[] FontFeatures { get; set; } = Array.Empty<float>();

    [VectorType(200)]
    public float[] AllFeatures { get; set; } = Array.Empty<float>();
}

