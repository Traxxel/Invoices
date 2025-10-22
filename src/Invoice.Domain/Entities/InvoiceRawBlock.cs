using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice.Domain.Entities;

public class InvoiceRawBlock
{
    public Guid Id { get; set; }

    [Required]
    public Guid InvoiceId { get; set; }

    [Required]
    public int Page { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public float X { get; set; }

    [Required]
    public float Y { get; set; }

    [Required]
    public float Width { get; set; }

    [Required]
    public float Height { get; set; }

    public int LineIndex { get; set; }

    [MaxLength(50)]
    public string? PredictedLabel { get; set; }

    public float? PredictionConfidence { get; set; }

    [MaxLength(50)]
    public string? ActualLabel { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    [ForeignKey("InvoiceId")]
    public virtual Invoice Invoice { get; set; } = null!;

    // Computed Properties
    public float Bottom => Y + Height;
    public float Right => X + Width;
    public float CenterX => X + (Width / 2);
    public float CenterY => Y + (Height / 2);
    public float Area => Width * Height;

    // Constructor
    public InvoiceRawBlock()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Methods
    public static InvoiceRawBlock Create(
        Guid invoiceId,
        int page,
        string text,
        float x,
        float y,
        float width,
        float height,
        int lineIndex = 0)
    {
        return new InvoiceRawBlock
        {
            InvoiceId = invoiceId,
            Page = page,
            Text = text,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            LineIndex = lineIndex
        };
    }

    public static InvoiceRawBlock CreateWithPrediction(
        Guid invoiceId,
        int page,
        string text,
        float x,
        float y,
        float width,
        float height,
        int lineIndex,
        string predictedLabel,
        float predictionConfidence)
    {
        return new InvoiceRawBlock
        {
            InvoiceId = invoiceId,
            Page = page,
            Text = text,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            LineIndex = lineIndex,
            PredictedLabel = predictedLabel,
            PredictionConfidence = predictionConfidence
        };
    }

    // Business Methods
    public void UpdatePrediction(string label, float confidence)
    {
        PredictedLabel = label;
        PredictionConfidence = confidence;
    }

    public void UpdateActualLabel(string label)
    {
        ActualLabel = label;
    }

    public bool IsCorrectlyPredicted()
    {
        return !string.IsNullOrEmpty(PredictedLabel) &&
               !string.IsNullOrEmpty(ActualLabel) &&
               PredictedLabel == ActualLabel;
    }

    public bool IsHighConfidence(float threshold = 0.8f)
    {
        return PredictionConfidence.HasValue && PredictionConfidence.Value >= threshold;
    }

    public bool IsLowConfidence(float threshold = 0.3f)
    {
        return PredictionConfidence.HasValue && PredictionConfidence.Value <= threshold;
    }

    public bool IsLabeled()
    {
        return !string.IsNullOrEmpty(ActualLabel);
    }

    public bool IsPredicted()
    {
        return !string.IsNullOrEmpty(PredictedLabel);
    }

    // Validation
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Text) &&
               Width > 0 &&
               Height > 0 &&
               Page > 0;
    }

    public override string ToString()
    {
        return $"Block {LineIndex} on Page {Page}: '{Text}' at ({X:F2},{Y:F2})";
    }
}

