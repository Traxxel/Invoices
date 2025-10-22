using Microsoft.ML;
using Microsoft.ML.Data;

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

