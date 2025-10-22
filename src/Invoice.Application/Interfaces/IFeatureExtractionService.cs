using Invoice.Application.Models;

namespace Invoice.Application.Interfaces;

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

