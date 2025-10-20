# Aufgabe 21: Feature-Extraction (Bounding Boxes, Regex-Hits)

## Ziel

Feature-Extraction Service für ML-Training mit Bounding Boxes, Regex-Hits und Kontext-Informationen.

## 1. Feature Extraction Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IFeatureExtractionService.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

public interface IFeatureExtractionService
{
    // Feature extraction
    Task<List<ExtractedFeature>> ExtractFeaturesAsync(List<NormalizedTextLine> textLines);
    Task<List<ExtractedFeature>> ExtractFeaturesFromDocumentAsync(ParsedDocument document);
    Task<ExtractedFeature> ExtractFeatureFromLineAsync(NormalizedTextLine textLine, int lineIndex);

    // Regex-based features
    Task<List<RegexHit>> ExtractRegexHitsAsync(string text);
    Task<List<RegexHit>> ExtractRegexHitsAsync(List<NormalizedTextLine> textLines);
    Task<Dictionary<string, int>> GetRegexHitCountsAsync(string text);

    // Position-based features
    Task<PositionFeatures> ExtractPositionFeaturesAsync(NormalizedTextLine textLine, int lineIndex, int totalLines);
    Task<BoundingBoxFeatures> ExtractBoundingBoxFeaturesAsync(NormalizedTextLine textLine);
    Task<LayoutFeatures> ExtractLayoutFeaturesAsync(List<NormalizedTextLine> textLines, int lineIndex);

    // Context features
    Task<ContextFeatures> ExtractContextFeaturesAsync(List<NormalizedTextLine> textLines, int lineIndex);
    Task<List<string>> ExtractNeighboringTextAsync(List<NormalizedTextLine> textLines, int lineIndex, int contextSize);

    // Statistical features
    Task<StatisticalFeatures> ExtractStatisticalFeaturesAsync(NormalizedTextLine textLine);
    Task<TextFeatures> ExtractTextFeaturesAsync(NormalizedTextLine textLine);

    // Combined feature extraction
    Task<MLFeatureVector> ExtractMLFeaturesAsync(NormalizedTextLine textLine, int lineIndex, List<NormalizedTextLine> allLines);
}

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
```

## 2. Feature Extraction Implementation

**Datei:** `src/InvoiceReader.Infrastructure/Services/FeatureExtractionService.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace InvoiceReader.Infrastructure.Services;

public class FeatureExtractionService : IFeatureExtractionService
{
    private readonly ILogger<FeatureExtractionService> _logger;
    private readonly Dictionary<string, Regex> _regexPatterns;

    public FeatureExtractionService(ILogger<FeatureExtractionService> logger)
    {
        _logger = logger;
        _regexPatterns = InitializeRegexPatterns();
    }

    public async Task<List<ExtractedFeature>> ExtractFeaturesAsync(List<NormalizedTextLine> textLines)
    {
        var features = new List<ExtractedFeature>();

        for (int i = 0; i < textLines.Count; i++)
        {
            try
            {
                var feature = await ExtractFeatureFromLineAsync(textLines[i], i);
                features.Add(feature);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract features from line {LineIndex}", i);
            }
        }

        return features;
    }

    public async Task<List<ExtractedFeature>> ExtractFeaturesFromDocumentAsync(ParsedDocument document)
    {
        var allTextLines = new List<NormalizedTextLine>();

        foreach (var page in document.Pages)
        {
            foreach (var line in page.TextLines)
            {
                allTextLines.Add(new NormalizedTextLine
                {
                    Text = line.Text,
                    X = line.X,
                    Y = line.Y,
                    Width = line.Width,
                    Height = line.Height,
                    PageNumber = line.PageNumber,
                    LineIndex = line.LineIndex,
                    FontSize = line.FontSize,
                    FontName = line.FontName
                });
            }
        }

        return await ExtractFeaturesAsync(allTextLines);
    }

    public async Task<ExtractedFeature> ExtractFeatureFromLineAsync(NormalizedTextLine textLine, int lineIndex)
    {
        var feature = new ExtractedFeature
        {
            Text = textLine.Text,
            LineIndex = lineIndex,
            PageNumber = textLine.PageNumber
        };

        try
        {
            // Extract position features
            feature.Position = await ExtractPositionFeaturesAsync(textLine, lineIndex, 0);

            // Extract bounding box features
            feature.BoundingBox = await ExtractBoundingBoxFeaturesAsync(textLine);

            // Extract layout features
            feature.Layout = await ExtractLayoutFeaturesAsync(new List<NormalizedTextLine> { textLine }, lineIndex);

            // Extract context features
            feature.Context = await ExtractContextFeaturesAsync(new List<NormalizedTextLine> { textLine }, lineIndex);

            // Extract statistical features
            feature.Statistics = await ExtractStatisticalFeaturesAsync(textLine);

            // Extract text features
            feature.TextFeatures = await ExtractTextFeaturesAsync(textLine);

            // Extract regex hits
            feature.RegexHits = await ExtractRegexHitsAsync(textLine.Text);

            // Extract ML features
            feature.MLFeatures = await ExtractMLFeaturesAsync(textLine, lineIndex, new List<NormalizedTextLine> { textLine });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract features from line: {Text}", textLine.Text);
        }

        return feature;
    }

    public async Task<List<RegexHit>> ExtractRegexHitsAsync(string text)
    {
        var hits = new List<RegexHit>();

        foreach (var pattern in _regexPatterns)
        {
            try
            {
                var matches = pattern.Value.Matches(text);
                foreach (Match match in matches)
                {
                    hits.Add(new RegexHit
                    {
                        Pattern = pattern.Key,
                        Match = match.Value,
                        StartIndex = match.Index,
                        EndIndex = match.Index + match.Length,
                        Category = GetRegexCategory(pattern.Key),
                        Confidence = CalculateRegexConfidence(match.Value, pattern.Key)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract regex hits for pattern: {Pattern}", pattern.Key);
            }
        }

        return hits;
    }

    public async Task<List<RegexHit>> ExtractRegexHitsAsync(List<NormalizedTextLine> textLines)
    {
        var allHits = new List<RegexHit>();

        foreach (var line in textLines)
        {
            var hits = await ExtractRegexHitsAsync(line.Text);
            allHits.AddRange(hits);
        }

        return allHits;
    }

    public async Task<Dictionary<string, int>> GetRegexHitCountsAsync(string text)
    {
        var counts = new Dictionary<string, int>();

        foreach (var pattern in _regexPatterns)
        {
            try
            {
                var matches = pattern.Value.Matches(text);
                counts[pattern.Key] = matches.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to count regex hits for pattern: {Pattern}", pattern.Key);
                counts[pattern.Key] = 0;
            }
        }

        return counts;
    }

    public async Task<PositionFeatures> ExtractPositionFeaturesAsync(NormalizedTextLine textLine, int lineIndex, int totalLines)
    {
        return new PositionFeatures
        {
            X = textLine.X,
            Y = textLine.Y,
            Width = textLine.Width,
            Height = textLine.Height,
            CenterX = textLine.X + (textLine.Width / 2),
            CenterY = textLine.Y + (textLine.Height / 2),
            RelativeX = textLine.X / 1000f, // Normalized to page width
            RelativeY = textLine.Y / 1000f, // Normalized to page height
            RelativeWidth = textLine.Width / 1000f,
            RelativeHeight = textLine.Height / 1000f,
            LineIndex = lineIndex,
            TotalLines = totalLines,
            LinePosition = totalLines > 0 ? (float)lineIndex / totalLines : 0f
        };
    }

    public async Task<BoundingBoxFeatures> ExtractBoundingBoxFeaturesAsync(NormalizedTextLine textLine)
    {
        var right = textLine.X + textLine.Width;
        var top = textLine.Y + textLine.Height;
        var area = textLine.Width * textLine.Height;
        var aspectRatio = textLine.Height > 0 ? textLine.Width / textLine.Height : 0f;

        return new BoundingBoxFeatures
        {
            Left = textLine.X,
            Top = top,
            Right = right,
            Bottom = textLine.Y,
            Width = textLine.Width,
            Height = textLine.Height,
            Area = area,
            AspectRatio = aspectRatio,
            IsValid = textLine.Width > 0 && textLine.Height > 0
        };
    }

    public async Task<LayoutFeatures> ExtractLayoutFeaturesAsync(List<NormalizedTextLine> textLines, int lineIndex)
    {
        var currentLine = textLines[lineIndex];
        var pageWidth = 1000f; // Default page width
        var pageHeight = 1000f; // Default page height

        return new LayoutFeatures
        {
            PageWidth = pageWidth,
            PageHeight = pageHeight,
            RelativeX = currentLine.X / pageWidth,
            RelativeY = currentLine.Y / pageHeight,
            RelativeWidth = currentLine.Width / pageWidth,
            RelativeHeight = currentLine.Height / pageHeight,
            Region = DetermineRegion(currentLine.Y, pageHeight),
            Alignment = DetermineAlignment(currentLine.X, currentLine.Width, pageWidth),
            Indentation = currentLine.X,
            IsAlignedWithPrevious = lineIndex > 0 && IsAlignedWith(currentLine, textLines[lineIndex - 1]),
            IsAlignedWithNext = lineIndex < textLines.Count - 1 && IsAlignedWith(currentLine, textLines[lineIndex + 1])
        };
    }

    public async Task<ContextFeatures> ExtractContextFeaturesAsync(List<NormalizedTextLine> textLines, int lineIndex)
    {
        var currentLine = textLines[lineIndex];
        var previousLine = lineIndex > 0 ? textLines[lineIndex - 1] : null;
        var nextLine = lineIndex < textLines.Count - 1 ? textLines[lineIndex + 1] : null;

        return new ContextFeatures
        {
            PreviousLine = previousLine?.Text ?? string.Empty,
            NextLine = nextLine?.Text ?? string.Empty,
            PreviousWord = previousLine?.Text.Split(' ').LastOrDefault() ?? string.Empty,
            NextWord = nextLine?.Text.Split(' ').FirstOrDefault() ?? string.Empty,
            NeighboringLines = ExtractNeighboringText(textLines, lineIndex, 2),
            DistanceToPrevious = previousLine != null ? Math.Abs(currentLine.Y - previousLine.Y) : 0f,
            DistanceToNext = nextLine != null ? Math.Abs(currentLine.Y - nextLine.Y) : 0f,
            IsFirstLine = lineIndex == 0,
            IsLastLine = lineIndex == textLines.Count - 1,
            IsIsolated = (previousLine == null || Math.Abs(currentLine.Y - previousLine.Y) > currentLine.Height * 2) &&
                        (nextLine == null || Math.Abs(currentLine.Y - nextLine.Y) > currentLine.Height * 2)
        };
    }

    public async Task<List<string>> ExtractNeighboringTextAsync(List<NormalizedTextLine> textLines, int lineIndex, int contextSize)
    {
        var neighbors = new List<string>();
        var start = Math.Max(0, lineIndex - contextSize);
        var end = Math.Min(textLines.Count - 1, lineIndex + contextSize);

        for (int i = start; i <= end; i++)
        {
            if (i != lineIndex)
            {
                neighbors.Add(textLines[i].Text);
            }
        }

        return neighbors;
    }

    public async Task<StatisticalFeatures> ExtractStatisticalFeaturesAsync(NormalizedTextLine textLine)
    {
        var text = textLine.Text;
        var characters = text.ToCharArray();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var digitCount = characters.Count(char.IsDigit);
        var letterCount = characters.Count(char.IsLetter);
        var specialCharCount = characters.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        var uppercaseCount = characters.Count(char.IsUpper);
        var lowercaseCount = characters.Count(char.IsLower);

        return new StatisticalFeatures
        {
            CharacterCount = characters.Length,
            WordCount = words.Length,
            DigitCount = digitCount,
            LetterCount = letterCount,
            SpecialCharCount = specialCharCount,
            AverageWordLength = words.Length > 0 ? (float)characters.Length / words.Length : 0f,
            DigitRatio = characters.Length > 0 ? (float)digitCount / characters.Length : 0f,
            LetterRatio = characters.Length > 0 ? (float)letterCount / characters.Length : 0f,
            SpecialCharRatio = characters.Length > 0 ? (float)specialCharCount / characters.Length : 0f,
            UppercaseCount = uppercaseCount,
            LowercaseCount = lowercaseCount,
            UppercaseRatio = characters.Length > 0 ? (float)uppercaseCount / characters.Length : 0f
        };
    }

    public async Task<TextFeatures> ExtractTextFeaturesAsync(NormalizedTextLine textLine)
    {
        var text = textLine.Text;
        var normalizedText = textLine.NormalizedText ?? text;

        return new TextFeatures
        {
            Text = text,
            NormalizedText = normalizedText,
            LowercaseText = text.ToLowerInvariant(),
            UppercaseText = text.ToUpperInvariant(),
            ContainsNumbers = text.Any(char.IsDigit),
            ContainsCurrency = ContainsCurrency(text),
            ContainsDate = ContainsDate(text),
            ContainsEmail = ContainsEmail(text),
            ContainsPhone = ContainsPhone(text),
            StartsWithNumber = text.Length > 0 && char.IsDigit(text[0]),
            StartsWithLetter = text.Length > 0 && char.IsLetter(text[0]),
            EndsWithPunctuation = text.Length > 0 && char.IsPunctuation(text[^1]),
            IsAllUppercase = text.All(c => !char.IsLetter(c) || char.IsUpper(c)),
            IsAllLowercase = text.All(c => !char.IsLetter(c) || char.IsLower(c)),
            IsMixedCase = text.Any(char.IsUpper) && text.Any(char.IsLower)
        };
    }

    public async Task<MLFeatureVector> ExtractMLFeaturesAsync(NormalizedTextLine textLine, int lineIndex, List<NormalizedTextLine> allLines)
    {
        var features = new List<float>();
        var featureNames = new List<string>();
        var namedFeatures = new Dictionary<string, float>();

        try
        {
            // Position features
            var position = await ExtractPositionFeaturesAsync(textLine, lineIndex, allLines.Count);
            features.AddRange(new[] { position.X, position.Y, position.Width, position.Height, position.CenterX, position.CenterY });
            featureNames.AddRange(new[] { "X", "Y", "Width", "Height", "CenterX", "CenterY" });

            // Statistical features
            var stats = await ExtractStatisticalFeaturesAsync(textLine);
            features.AddRange(new[] { stats.CharacterCount, stats.WordCount, stats.DigitCount, stats.LetterCount, stats.SpecialCharCount });
            featureNames.AddRange(new[] { "CharCount", "WordCount", "DigitCount", "LetterCount", "SpecialCharCount" });

            // Text features
            var textFeatures = await ExtractTextFeaturesAsync(textLine);
            features.AddRange(new[] {
                textFeatures.ContainsNumbers ? 1f : 0f,
                textFeatures.ContainsCurrency ? 1f : 0f,
                textFeatures.ContainsDate ? 1f : 0f,
                textFeatures.StartsWithNumber ? 1f : 0f,
                textFeatures.StartsWithLetter ? 1f : 0f
            });
            featureNames.AddRange(new[] { "HasNumbers", "HasCurrency", "HasDate", "StartsWithNumber", "StartsWithLetter" });

            // Regex hits
            var regexHits = await ExtractRegexHitsAsync(textLine.Text);
            var regexCounts = new Dictionary<string, int>();
            foreach (var hit in regexHits)
            {
                if (!regexCounts.ContainsKey(hit.Category))
                    regexCounts[hit.Category] = 0;
                regexCounts[hit.Category]++;
            }

            var regexCategories = new[] { "InvoiceNumber", "Date", "Amount", "Currency", "Email", "Phone" };
            foreach (var category in regexCategories)
            {
                features.Add(regexCounts.GetValueOrDefault(category, 0));
                featureNames.Add($"Regex_{category}");
            }

            // Context features
            var context = await ExtractContextFeaturesAsync(allLines, lineIndex);
            features.AddRange(new[] {
                context.DistanceToPrevious,
                context.DistanceToNext,
                context.IsFirstLine ? 1f : 0f,
                context.IsLastLine ? 1f : 0f,
                context.IsIsolated ? 1f : 0f
            });
            featureNames.AddRange(new[] { "DistToPrev", "DistToNext", "IsFirst", "IsLast", "IsIsolated" });

            return new MLFeatureVector
            {
                Features = features.ToArray(),
                FeatureNames = featureNames.ToArray(),
                NamedFeatures = namedFeatures,
                FeatureCount = features.Count,
                IsValid = features.Count > 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract ML features from line: {Text}", textLine.Text);
            return new MLFeatureVector { IsValid = false };
        }
    }

    private Dictionary<string, Regex> InitializeRegexPatterns()
    {
        return new Dictionary<string, Regex>
        {
            { "InvoiceNumber", new Regex(@"(?i)\b(RE|INV|RG|RNR|Rechnungs(?:-)?Nr\.?)\s*[:\-]?\s*([A-Z0-9\-\/\.]{4,})", RegexOptions.Compiled) },
            { "Date", new Regex(@"\b(0?[1-9]|[12][0-9]|3[01])\.(0?[1-9]|1[0-2])\.(19|20)\d\d\b", RegexOptions.Compiled) },
            { "Amount", new Regex(@"[-+]?\d{1,3}(\.\d{3})*,\d{2}", RegexOptions.Compiled) },
            { "Currency", new Regex(@"€|EUR|USD|\$", RegexOptions.Compiled) },
            { "Email", new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled) },
            { "Phone", new Regex(@"\b[\+]?[0-9\s\-\(\)]{10,}\b", RegexOptions.Compiled) }
        };
    }

    private string GetRegexCategory(string patternName)
    {
        return patternName switch
        {
            "InvoiceNumber" => "InvoiceNumber",
            "Date" => "Date",
            "Amount" => "Amount",
            "Currency" => "Currency",
            "Email" => "Email",
            "Phone" => "Phone",
            _ => "Other"
        };
    }

    private float CalculateRegexConfidence(string match, string patternName)
    {
        // Simple confidence calculation based on match length and pattern
        var baseConfidence = 0.5f;

        if (patternName == "InvoiceNumber" && match.Length >= 4)
            baseConfidence = 0.8f;
        else if (patternName == "Date" && match.Length >= 8)
            baseConfidence = 0.9f;
        else if (patternName == "Amount" && match.Contains(','))
            baseConfidence = 0.7f;

        return Math.Min(1.0f, baseConfidence);
    }

    private string DetermineRegion(float y, float pageHeight)
    {
        var relativeY = y / pageHeight;
        return relativeY switch
        {
            < 0.2f => "header",
            > 0.8f => "footer",
            _ => "body"
        };
    }

    private string DetermineAlignment(float x, float width, float pageWidth)
    {
        var centerX = pageWidth / 2;
        var lineCenter = x + (width / 2);

        if (lineCenter < centerX - 50)
            return "left";
        else if (lineCenter > centerX + 50)
            return "right";
        else
            return "center";
    }

    private bool IsAlignedWith(NormalizedTextLine line1, NormalizedTextLine line2)
    {
        var tolerance = 10f;
        return Math.Abs(line1.X - line2.X) < tolerance;
    }

    private List<string> ExtractNeighboringText(List<NormalizedTextLine> textLines, int lineIndex, int contextSize)
    {
        var neighbors = new List<string>();
        var start = Math.Max(0, lineIndex - contextSize);
        var end = Math.Min(textLines.Count - 1, lineIndex + contextSize);

        for (int i = start; i <= end; i++)
        {
            if (i != lineIndex)
            {
                neighbors.Add(textLines[i].Text);
            }
        }

        return neighbors;
    }

    private bool ContainsCurrency(string text)
    {
        return text.Contains("€") || text.Contains("EUR") || text.Contains("$") || text.Contains("USD");
    }

    private bool ContainsDate(string text)
    {
        return _regexPatterns["Date"].IsMatch(text);
    }

    private bool ContainsEmail(string text)
    {
        return _regexPatterns["Email"].IsMatch(text);
    }

    private bool ContainsPhone(string text)
    {
        return _regexPatterns["Phone"].IsMatch(text);
    }
}
```

## 3. Feature Extraction Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/FeatureExtractionExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class FeatureExtractionExtensions
{
    public static IServiceCollection AddFeatureExtractionServices(this IServiceCollection services)
    {
        services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Umfassende Feature-Extraktion für ML-Training
- Position-basierte Features für Layout-Erkennung
- Regex-basierte Features für Pattern-Erkennung
- Kontext-Features für bessere Klassifikation
- Statistical Features für Text-Analyse
- ML Feature Vector für Machine Learning
- Bounding Box Features für geometrische Analyse
- Layout Features für Dokument-Struktur
- Error Handling für alle Feature-Extraktionen
- Logging für alle Feature-Operationen
- Strukturierte Feature-Daten für ML-Pipeline
