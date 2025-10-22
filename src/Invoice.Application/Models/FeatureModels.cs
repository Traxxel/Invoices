namespace Invoice.Application.Models;

public class ExtractedFeature
{
    public string Text { get; set; } = string.Empty;
    public int LineIndex { get; set; }
    public int PageNumber { get; set; }
    public PositionFeatures Position { get; set; } = new();
    public BoundingBoxFeatures BoundingBox { get; set; } = new();
    public LayoutFeatures Layout { get; set; } = new();
    public ContextFeatures Context { get; set; } = new();
    public StatisticalFeatures Statistics { get; set; } = new();
    public TextFeatures TextFeatures { get; set; } = new();
    public List<RegexHit> RegexHits { get; set; } = new();
    public MLFeatureVector MLFeatures { get; set; } = new();
}

public class RegexHit
{
    public string Pattern { get; set; } = string.Empty;
    public string Match { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string Category { get; set; } = string.Empty;
    public float Confidence { get; set; }
}

public class PositionFeatures
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float CenterX { get; set; }
    public float CenterY { get; set; }
    public float RelativeX { get; set; }
    public float RelativeY { get; set; }
    public float RelativeWidth { get; set; }
    public float RelativeHeight { get; set; }
    public int LineIndex { get; set; }
    public int TotalLines { get; set; }
    public float LinePosition { get; set; }
}

public class BoundingBoxFeatures
{
    public float Left { get; set; }
    public float Top { get; set; }
    public float Right { get; set; }
    public float Bottom { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Area { get; set; }
    public float AspectRatio { get; set; }
    public bool IsValid { get; set; }
}

public class LayoutFeatures
{
    public float PageWidth { get; set; }
    public float PageHeight { get; set; }
    public float RelativeX { get; set; }
    public float RelativeY { get; set; }
    public float RelativeWidth { get; set; }
    public float RelativeHeight { get; set; }
    public string Region { get; set; } = string.Empty; // "header", "body", "footer"
    public string Alignment { get; set; } = string.Empty; // "left", "center", "right"
    public float Indentation { get; set; }
    public bool IsAlignedWithPrevious { get; set; }
    public bool IsAlignedWithNext { get; set; }
}

public class ContextFeatures
{
    public string PreviousLine { get; set; } = string.Empty;
    public string NextLine { get; set; } = string.Empty;
    public string PreviousWord { get; set; } = string.Empty;
    public string NextWord { get; set; } = string.Empty;
    public List<string> NeighboringLines { get; set; } = new();
    public float DistanceToPrevious { get; set; }
    public float DistanceToNext { get; set; }
    public bool IsFirstLine { get; set; }
    public bool IsLastLine { get; set; }
    public bool IsIsolated { get; set; }
}

public class StatisticalFeatures
{
    public int CharacterCount { get; set; }
    public int WordCount { get; set; }
    public int DigitCount { get; set; }
    public int LetterCount { get; set; }
    public int SpecialCharCount { get; set; }
    public float AverageWordLength { get; set; }
    public float DigitRatio { get; set; }
    public float LetterRatio { get; set; }
    public float SpecialCharRatio { get; set; }
    public int UppercaseCount { get; set; }
    public int LowercaseCount { get; set; }
    public float UppercaseRatio { get; set; }
}

public class TextFeatures
{
    public string Text { get; set; } = string.Empty;
    public string NormalizedText { get; set; } = string.Empty;
    public string LowercaseText { get; set; } = string.Empty;
    public string UppercaseText { get; set; } = string.Empty;
    public bool ContainsNumbers { get; set; }
    public bool ContainsCurrency { get; set; }
    public bool ContainsDate { get; set; }
    public bool ContainsEmail { get; set; }
    public bool ContainsPhone { get; set; }
    public bool StartsWithNumber { get; set; }
    public bool StartsWithLetter { get; set; }
    public bool EndsWithPunctuation { get; set; }
    public bool IsAllUppercase { get; set; }
    public bool IsAllLowercase { get; set; }
    public bool IsMixedCase { get; set; }
}

public class MLFeatureVector
{
    public float[] Features { get; set; } = Array.Empty<float>();
    public string[] FeatureNames { get; set; } = Array.Empty<string>();
    public Dictionary<string, float> NamedFeatures { get; set; } = new();
    public int FeatureCount { get; set; }
    public bool IsValid { get; set; }
}

