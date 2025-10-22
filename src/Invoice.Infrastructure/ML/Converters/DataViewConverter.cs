using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Infrastructure.ML.Models;
using Invoice.Application.Interfaces;
using Invoice.Application.Models;

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

    public IDataView ConvertToDataView(List<ExtractedFeature> features, string documentId = "")
    {
        var inputRows = ConvertToInputRows(features, documentId);
        return _mlContext.Data.LoadFromEnumerable(inputRows);
    }

    public async Task<IDataView> ConvertToDataViewAsync(List<NormalizedTextLine> textLines, string documentId = "")
    {
        var features = await _featureExtractionService.ExtractFeaturesAsync(textLines);
        return ConvertToDataView(features, documentId);
    }

    public InputRow ConvertToInputRow(ExtractedFeature feature, string documentId = "")
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

            // Font features (will be set from original text line)
            FontSize = 0f,
            FontName = string.Empty,

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
            var inputRow = ConvertToInputRow(feature, documentId);
            inputRows.Add(inputRow);
        }

        return inputRows;
    }

    public InputRowPrediction ConvertToPrediction(string predictedLabel, float[] scores, float[] probabilities)
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

