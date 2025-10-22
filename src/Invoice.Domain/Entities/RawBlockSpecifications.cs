using System.Linq.Expressions;

namespace Invoice.Domain.Entities;

public static class RawBlockSpecifications
{
    public static Expression<Func<InvoiceRawBlock, bool>> ByInvoiceId(Guid invoiceId)
    {
        return x => x.InvoiceId == invoiceId;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByPage(int page)
    {
        return x => x.Page == page;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByPredictedLabel(string label)
    {
        return x => x.PredictedLabel == label;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByActualLabel(string label)
    {
        return x => x.ActualLabel == label;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByConfidenceRange(float minConfidence, float maxConfidence)
    {
        return x => x.PredictionConfidence.HasValue &&
                    x.PredictionConfidence.Value >= minConfidence &&
                    x.PredictionConfidence.Value <= maxConfidence;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> HighConfidence(float threshold = 0.8f)
    {
        return x => x.PredictionConfidence.HasValue && x.PredictionConfidence.Value >= threshold;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> LowConfidence(float threshold = 0.3f)
    {
        return x => x.PredictionConfidence.HasValue && x.PredictionConfidence.Value <= threshold;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> CorrectlyPredicted()
    {
        return x => !string.IsNullOrEmpty(x.PredictedLabel) &&
                    !string.IsNullOrEmpty(x.ActualLabel) &&
                    x.PredictedLabel == x.ActualLabel;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> Misclassified()
    {
        return x => !string.IsNullOrEmpty(x.PredictedLabel) &&
                    !string.IsNullOrEmpty(x.ActualLabel) &&
                    x.PredictedLabel != x.ActualLabel;
    }

    public static Expression<Func<InvoiceRawBlock, bool>> Unlabeled()
    {
        return x => string.IsNullOrEmpty(x.ActualLabel);
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByTextContains(string searchText)
    {
        return x => x.Text.Contains(searchText);
    }

    public static Expression<Func<InvoiceRawBlock, bool>> ByPosition(float minX, float maxX, float minY, float maxY)
    {
        return x => x.X >= minX && x.X <= maxX && x.Y >= minY && x.Y <= maxY;
    }
}

