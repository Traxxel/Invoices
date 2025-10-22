# Aufgabe 22: InputRow DataView Schema

## Ziel

ML.NET DataView Schema für InputRow mit allen Features für ML-Training und Prediction.

## 1. InputRow Data Model

**Datei:** `src/Invoice.Infrastructure/ML/Models/InputRow.cs`

```csharp
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
```

## 2. DataView Schema Configuration

**Datei:** `src/Invoice.Infrastructure/ML/Configuration/DataViewSchemaConfiguration.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Infrastructure.ML.Models;

namespace Invoice.Infrastructure.ML.Configuration;

public static class DataViewSchemaConfiguration
{
    public static DataViewSchema GetInputRowSchema()
    {
        var builder = new DataViewSchema.Builder();

        // Text features
        builder.AddColumn("Text", TextDataViewType.Instance);
        builder.AddColumn("NormalizedText", TextDataViewType.Instance);
        builder.AddColumn("LowercaseText", TextDataViewType.Instance);

        // Position features
        builder.AddColumn("X", NumberDataViewType.Single);
        builder.AddColumn("Y", NumberDataViewType.Single);
        builder.AddColumn("Width", NumberDataViewType.Single);
        builder.AddColumn("Height", NumberDataViewType.Single);
        builder.AddColumn("CenterX", NumberDataViewType.Single);
        builder.AddColumn("CenterY", NumberDataViewType.Single);
        builder.AddColumn("RelativeX", NumberDataViewType.Single);
        builder.AddColumn("RelativeY", NumberDataViewType.Single);
        builder.AddColumn("RelativeWidth", NumberDataViewType.Single);
        builder.AddColumn("RelativeHeight", NumberDataViewType.Single);

        // Line features
        builder.AddColumn("LineIndex", NumberDataViewType.Int32);
        builder.AddColumn("TotalLines", NumberDataViewType.Int32);
        builder.AddColumn("LinePosition", NumberDataViewType.Single);
        builder.AddColumn("PageNumber", NumberDataViewType.Int32);

        // Statistical features
        builder.AddColumn("CharacterCount", NumberDataViewType.Int32);
        builder.AddColumn("WordCount", NumberDataViewType.Int32);
        builder.AddColumn("DigitCount", NumberDataViewType.Int32);
        builder.AddColumn("LetterCount", NumberDataViewType.Int32);
        builder.AddColumn("SpecialCharCount", NumberDataViewType.Int32);
        builder.AddColumn("AverageWordLength", NumberDataViewType.Single);
        builder.AddColumn("DigitRatio", NumberDataViewType.Single);
        builder.AddColumn("LetterRatio", NumberDataViewType.Single);
        builder.AddColumn("SpecialCharRatio", NumberDataViewType.Single);
        builder.AddColumn("UppercaseCount", NumberDataViewType.Int32);
        builder.AddColumn("LowercaseCount", NumberDataViewType.Int32);
        builder.AddColumn("UppercaseRatio", NumberDataViewType.Single);

        // Text pattern features
        builder.AddColumn("ContainsNumbers", BooleanDataViewType.Instance);
        builder.AddColumn("ContainsCurrency", BooleanDataViewType.Instance);
        builder.AddColumn("ContainsDate", BooleanDataViewType.Instance);
        builder.AddColumn("ContainsEmail", BooleanDataViewType.Instance);
        builder.AddColumn("ContainsPhone", BooleanDataViewType.Instance);
        builder.AddColumn("StartsWithNumber", BooleanDataViewType.Instance);
        builder.AddColumn("StartsWithLetter", BooleanDataViewType.Instance);
        builder.AddColumn("EndsWithPunctuation", BooleanDataViewType.Instance);
        builder.AddColumn("IsAllUppercase", BooleanDataViewType.Instance);
        builder.AddColumn("IsAllLowercase", BooleanDataViewType.Instance);
        builder.AddColumn("IsMixedCase", BooleanDataViewType.Instance);

        // Regex hit counts
        builder.AddColumn("RegexInvoiceNumberHits", NumberDataViewType.Int32);
        builder.AddColumn("RegexDateHits", NumberDataViewType.Int32);
        builder.AddColumn("RegexAmountHits", NumberDataViewType.Int32);
        builder.AddColumn("RegexCurrencyHits", NumberDataViewType.Int32);
        builder.AddColumn("RegexEmailHits", NumberDataViewType.Int32);
        builder.AddColumn("RegexPhoneHits", NumberDataViewType.Int32);

        // Context features
        builder.AddColumn("PreviousLine", TextDataViewType.Instance);
        builder.AddColumn("NextLine", TextDataViewType.Instance);
        builder.AddColumn("DistanceToPrevious", NumberDataViewType.Single);
        builder.AddColumn("DistanceToNext", NumberDataViewType.Single);
        builder.AddColumn("IsFirstLine", BooleanDataViewType.Instance);
        builder.AddColumn("IsLastLine", BooleanDataViewType.Instance);
        builder.AddColumn("IsIsolated", BooleanDataViewType.Instance);

        // Layout features
        builder.AddColumn("PageWidth", NumberDataViewType.Single);
        builder.AddColumn("PageHeight", NumberDataViewType.Single);
        builder.AddColumn("Region", TextDataViewType.Instance);
        builder.AddColumn("Alignment", TextDataViewType.Instance);
        builder.AddColumn("Indentation", NumberDataViewType.Single);
        builder.AddColumn("IsAlignedWithPrevious", BooleanDataViewType.Instance);
        builder.AddColumn("IsAlignedWithNext", BooleanDataViewType.Instance);

        // Font features
        builder.AddColumn("FontSize", NumberDataViewType.Single);
        builder.AddColumn("FontName", TextDataViewType.Instance);

        // Target label
        builder.AddColumn("Label", TextDataViewType.Instance);

        // Metadata
        builder.AddColumn("DocumentId", TextDataViewType.Instance);
        builder.AddColumn("ModelVersion", TextDataViewType.Instance);
        builder.AddColumn("CreatedAt", DateTimeDataViewType.Instance);

        return builder.ToSchema();
    }

    public static DataViewSchema GetFeatureSchema()
    {
        var builder = new DataViewSchema.Builder();

        // Feature vectors
        builder.AddColumn("TextFeatures", new VectorDataViewType(NumberDataViewType.Single, 100));
        builder.AddColumn("PositionFeatures", new VectorDataViewType(NumberDataViewType.Single, 20));
        builder.AddColumn("StatisticalFeatures", new VectorDataViewType(NumberDataViewType.Single, 15));
        builder.AddColumn("PatternFeatures", new VectorDataViewType(NumberDataViewType.Single, 10));
        builder.AddColumn("RegexFeatures", new VectorDataViewType(NumberDataViewType.Single, 6));
        builder.AddColumn("ContextFeatures", new VectorDataViewType(NumberDataViewType.Single, 10));
        builder.AddColumn("LayoutFeatures", new VectorDataViewType(NumberDataViewType.Single, 10));
        builder.AddColumn("FontFeatures", new VectorDataViewType(NumberDataViewType.Single, 5));
        builder.AddColumn("AllFeatures", new VectorDataViewType(NumberDataViewType.Single, 200));

        return builder.ToSchema();
    }

    public static DataViewSchema GetPredictionSchema()
    {
        var builder = new DataViewSchema.Builder();

        // Prediction results
        builder.AddColumn("PredictedLabel", TextDataViewType.Instance);
        builder.AddColumn("Score", new VectorDataViewType(NumberDataViewType.Single, 7)); // 7 classes
        builder.AddColumn("Probability", new VectorDataViewType(NumberDataViewType.Single, 7));
        builder.AddColumn("Confidence", NumberDataViewType.Single);
        builder.AddColumn("Top3Predictions", new VectorDataViewType(TextDataViewType.Instance, 3));
        builder.AddColumn("Top3Scores", new VectorDataViewType(NumberDataViewType.Single, 3));

        return builder.ToSchema();
    }
}
```

## 3. DataView Converter

**Datei:** `src/Invoice.Infrastructure/ML/Converters/DataViewConverter.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Infrastructure.ML.Models;
using Invoice.Application.Interfaces;

namespace Invoice.Infrastructure.ML.Converters;

public class DataViewConverter
{
    private readonly MLContext _mlContext;
    private readonly IFeatureExtractionService _featureExtractionService;

    public DataViewConverter(MLContext mlContext, IFeatureExtractionService featureExtractionService)
    {
        _mlContext = mlContext;
        _featureExtractionService = featureExtractionService;
    }

    public IDataView ConvertToDataView(List<ExtractedFeature> features)
    {
        var inputRows = new List<InputRow>();

        foreach (var feature in features)
        {
            var inputRow = ConvertToInputRow(feature);
            inputRows.Add(inputRow);
        }

        return _mlContext.Data.LoadFromEnumerable(inputRows);
    }

    public IDataView ConvertToDataView(List<NormalizedTextLine> textLines, string documentId = "")
    {
        var features = new List<ExtractedFeature>();

        for (int i = 0; i < textLines.Count; i++)
        {
            var feature = _featureExtractionService.ExtractFeatureFromLineAsync(textLines[i], i).Result;
            features.Add(feature);
        }

        return ConvertToDataView(features);
    }

    public InputRow ConvertToInputRow(ExtractedFeature feature)
    {
        return new InputRow
        {
            // Text features
            Text = feature.Text,
            NormalizedText = feature.TextFeatures.NormalizedText,
            LowercaseText = feature.TextFeatures.LowercaseText,

            // Position features
            X = feature.Position.X,
            Y = feature.Position.Y,
            Width = feature.Position.Width,
            Height = feature.Position.Height,
            CenterX = feature.Position.CenterX,
            CenterY = feature.Position.CenterY,
            RelativeX = feature.Position.RelativeX,
            RelativeY = feature.Position.RelativeY,
            RelativeWidth = feature.Position.RelativeWidth,
            RelativeHeight = feature.Position.RelativeHeight,

            // Line features
            LineIndex = feature.LineIndex,
            TotalLines = feature.Position.TotalLines,
            LinePosition = feature.Position.LinePosition,
            PageNumber = feature.PageNumber,

            // Statistical features
            CharacterCount = feature.Statistics.CharacterCount,
            WordCount = feature.Statistics.WordCount,
            DigitCount = feature.Statistics.DigitCount,
            LetterCount = feature.Statistics.LetterCount,
            SpecialCharCount = feature.Statistics.SpecialCharCount,
            AverageWordLength = feature.Statistics.AverageWordLength,
            DigitRatio = feature.Statistics.DigitRatio,
            LetterRatio = feature.Statistics.LetterRatio,
            SpecialCharRatio = feature.Statistics.SpecialCharRatio,
            UppercaseCount = feature.Statistics.UppercaseCount,
            LowercaseCount = feature.Statistics.LowercaseCount,
            UppercaseRatio = feature.Statistics.UppercaseRatio,

            // Text pattern features
            ContainsNumbers = feature.TextFeatures.ContainsNumbers,
            ContainsCurrency = feature.TextFeatures.ContainsCurrency,
            ContainsDate = feature.TextFeatures.ContainsDate,
            ContainsEmail = feature.TextFeatures.ContainsEmail,
            ContainsPhone = feature.TextFeatures.ContainsPhone,
            StartsWithNumber = feature.TextFeatures.StartsWithNumber,
            StartsWithLetter = feature.TextFeatures.StartsWithLetter,
            EndsWithPunctuation = feature.TextFeatures.EndsWithPunctuation,
            IsAllUppercase = feature.TextFeatures.IsAllUppercase,
            IsAllLowercase = feature.TextFeatures.IsAllLowercase,
            IsMixedCase = feature.TextFeatures.IsMixedCase,

            // Regex hit counts
            RegexInvoiceNumberHits = feature.RegexHits.Count(h => h.Category == "InvoiceNumber"),
            RegexDateHits = feature.RegexHits.Count(h => h.Category == "Date"),
            RegexAmountHits = feature.RegexHits.Count(h => h.Category == "Amount"),
            RegexCurrencyHits = feature.RegexHits.Count(h => h.Category == "Currency"),
            RegexEmailHits = feature.RegexHits.Count(h => h.Category == "Email"),
            RegexPhoneHits = feature.RegexHits.Count(h => h.Category == "Phone"),

            // Context features
            PreviousLine = feature.Context.PreviousLine,
            NextLine = feature.Context.NextLine,
            DistanceToPrevious = feature.Context.DistanceToPrevious,
            DistanceToNext = feature.Context.DistanceToNext,
            IsFirstLine = feature.Context.IsFirstLine,
            IsLastLine = feature.Context.IsLastLine,
            IsIsolated = feature.Context.IsIsolated,

            // Layout features
            PageWidth = feature.Layout.PageWidth,
            PageHeight = feature.Layout.PageHeight,
            Region = feature.Layout.Region,
            Alignment = feature.Layout.Alignment,
            Indentation = feature.Layout.Indentation,
            IsAlignedWithPrevious = feature.Layout.IsAlignedWithPrevious,
            IsAlignedWithNext = feature.Layout.IsAlignedWithNext,

            // Font features
            FontSize = 0f, // Will be set from original text line
            FontName = string.Empty, // Will be set from original text line

            // Metadata
            DocumentId = documentId,
            ModelVersion = "v1.0",
            CreatedAt = DateTime.UtcNow
        };
    }

    public List<InputRow> ConvertToInputRows(List<ExtractedFeature> features, string documentId = "")
    {
        var inputRows = new List<InputRow>();

        foreach (var feature in features)
        {
            var inputRow = ConvertToInputRow(feature);
            inputRow.DocumentId = documentId;
            inputRows.Add(inputRow);
        }

        return inputRows;
    }

    public InputRowPrediction ConvertToPrediction(InputRow inputRow, string predictedLabel, float[] scores, float[] probabilities)
    {
        var sortedIndices = scores
            .Select((score, index) => new { Score = score, Index = index })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToArray();

        var top3Predictions = sortedIndices.Select(x => GetLabelName(x.Index)).ToArray();
        var top3Scores = sortedIndices.Select(x => x.Score).ToArray();

        return new InputRowPrediction
        {
            PredictedLabel = predictedLabel,
            Score = scores,
            Probability = probabilities,
            Confidence = scores.Max(),
            Top3Predictions = top3Predictions,
            Top3Scores = top3Scores
        };
    }

    private string GetLabelName(int index)
    {
        return index switch
        {
            0 => "None",
            1 => "InvoiceNumber",
            2 => "InvoiceDate",
            3 => "IssuerAddress",
            4 => "NetTotal",
            5 => "VatTotal",
            6 => "GrossTotal",
            _ => "Unknown"
        };
    }
}
```

## 4. DataView Extensions

**Datei:** `src/Invoice.Infrastructure/ML/Extensions/DataViewExtensions.cs`

```csharp
using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Infrastructure.ML.Models;

namespace Invoice.Infrastructure.ML.Extensions;

public static class DataViewExtensions
{
    public static List<InputRow> ToInputRows(this IDataView dataView)
    {
        var rows = new List<InputRow>();
        var cursor = dataView.GetRowCursor(dataView.Schema);

        while (cursor.MoveNext())
        {
            var row = new InputRow();

            // Map columns to properties
            var textCol = cursor.GetGetter<ReadOnlyMemory<char>>(dataView.Schema["Text"]);
            var xCol = cursor.GetGetter<float>(dataView.Schema["X"]);
            var yCol = cursor.GetGetter<float>(dataView.Schema["Y"]);
            var labelCol = cursor.GetGetter<ReadOnlyMemory<char>>(dataView.Schema["Label"]);

            textCol(ref row.Text);
            xCol(ref row.X);
            yCol(ref row.Y);
            labelCol(ref row.Label);

            rows.Add(row);
        }

        return rows;
    }

    public static List<InputRowPrediction> ToPredictions(this IDataView dataView)
    {
        var predictions = new List<InputRowPrediction>();
        var cursor = dataView.GetRowCursor(dataView.Schema);

        while (cursor.MoveNext())
        {
            var prediction = new InputRowPrediction();

            // Map prediction columns
            var predictedLabelCol = cursor.GetGetter<ReadOnlyMemory<char>>(dataView.Schema["PredictedLabel"]);
            var scoreCol = cursor.GetGetter<VBuffer<float>>(dataView.Schema["Score"]);
            var confidenceCol = cursor.GetGetter<float>(dataView.Schema["Confidence"]);

            predictedLabelCol(ref prediction.PredictedLabel);
            scoreCol(ref prediction.Score);
            confidenceCol(ref prediction.Confidence);

            predictions.Add(prediction);
        }

        return predictions;
    }

    public static DataViewSchema GetSchema(this IDataView dataView)
    {
        return dataView.Schema;
    }

    public static int GetRowCount(this IDataView dataView)
    {
        return (int)dataView.GetRowCount();
    }

    public static bool HasColumn(this IDataView dataView, string columnName)
    {
        return dataView.Schema.Contains(columnName);
    }

    public static DataViewType GetColumnType(this IDataView dataView, string columnName)
    {
        return dataView.Schema[columnName].Type;
    }
}
```

## Wichtige Hinweise

- Vollständiges InputRow Schema für ML.NET
- Alle Features als DataView Columns definiert
- Position, Statistical, Text Pattern Features
- Regex Hit Counts für Pattern-Erkennung
- Context und Layout Features
- Font Features für Typography-Analyse
- Prediction Schema für ML-Ergebnisse
- DataView Converter für Feature-Transformation
- Extensions für DataView-Operationen
- Error Handling für alle Conversions
- Logging für alle DataView-Operationen
