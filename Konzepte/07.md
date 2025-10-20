# Aufgabe 07: InvoiceRawBlock Entity

## Ziel

Entity für rohe Textblöcke aus PDF-Parsing zur Nachvollziehbarkeit der ML-Extraktion.

## 1. InvoiceRawBlock Entity

**Datei:** `src/InvoiceReader.Domain/Entities/InvoiceRawBlock.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceReader.Domain.Entities;

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
```

## 2. RawBlock Factory

**Datei:** `src/InvoiceReader.Domain/Entities/RawBlockFactory.cs`

```csharp
namespace InvoiceReader.Domain.Entities;

public static class RawBlockFactory
{
    public static List<InvoiceRawBlock> CreateFromTextLines(
        Guid invoiceId,
        int page,
        IEnumerable<TextLine> textLines)
    {
        var blocks = new List<InvoiceRawBlock>();
        int lineIndex = 0;

        foreach (var line in textLines)
        {
            var block = InvoiceRawBlock.Create(
                invoiceId,
                page,
                line.Text,
                line.X,
                line.Y,
                line.Width,
                line.Height,
                lineIndex++
            );

            blocks.Add(block);
        }

        return blocks;
    }

    public static List<InvoiceRawBlock> CreateFromWords(
        Guid invoiceId,
        int page,
        IEnumerable<Word> words)
    {
        var blocks = new List<InvoiceRawBlock>();
        var currentLine = new List<Word>();
        int lineIndex = 0;

        foreach (var word in words.OrderBy(w => w.Y).ThenBy(w => w.X))
        {
            if (currentLine.Count == 0 || IsSameLine(currentLine.Last(), word))
            {
                currentLine.Add(word);
            }
            else
            {
                // Create block from current line
                if (currentLine.Count > 0)
                {
                    var block = CreateBlockFromWords(invoiceId, page, currentLine, lineIndex++);
                    blocks.Add(block);
                }

                currentLine = new List<Word> { word };
            }
        }

        // Add last line
        if (currentLine.Count > 0)
        {
            var block = CreateBlockFromWords(invoiceId, page, currentLine, lineIndex);
            blocks.Add(block);
        }

        return blocks;
    }

    private static bool IsSameLine(Word word1, Word word2)
    {
        const float lineHeightTolerance = 5.0f;
        return Math.Abs(word1.Y - word2.Y) <= lineHeightTolerance;
    }

    private static InvoiceRawBlock CreateBlockFromWords(
        Guid invoiceId,
        int page,
        List<Word> words,
        int lineIndex)
    {
        var text = string.Join(" ", words.Select(w => w.Text));
        var x = words.Min(w => w.X);
        var y = words.Min(w => w.Y);
        var width = words.Max(w => w.X + w.Width) - x;
        var height = words.Max(w => w.Y + w.Height) - y;

        return InvoiceRawBlock.Create(invoiceId, page, text, x, y, width, height, lineIndex);
    }
}

// Helper Classes
public class TextLine
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class Word
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
```

## 3. RawBlock Specifications

**Datei:** `src/InvoiceReader.Domain/Entities/RawBlockSpecifications.cs`

```csharp
using System.Linq.Expressions;

namespace InvoiceReader.Domain.Entities;

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
```

## 4. RawBlock Comparer

**Datei:** `src/InvoiceReader.Domain/Entities/RawBlockComparer.cs`

```csharp
using System.Collections.Generic;

namespace InvoiceReader.Domain.Entities;

public class RawBlockComparer : IEqualityComparer<InvoiceRawBlock>
{
    public bool Equals(InvoiceRawBlock? x, InvoiceRawBlock? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.InvoiceId == y.InvoiceId &&
               x.Page == y.Page &&
               x.LineIndex == y.LineIndex &&
               x.Text == y.Text;
    }

    public int GetHashCode(InvoiceRawBlock obj)
    {
        return HashCode.Combine(obj.InvoiceId, obj.Page, obj.LineIndex, obj.Text);
    }
}

public class RawBlockByPositionComparer : IComparer<InvoiceRawBlock>
{
    public int Compare(InvoiceRawBlock? x, InvoiceRawBlock? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Sort by page first, then by Y position, then by X position
        var pageComparison = x.Page.CompareTo(y.Page);
        if (pageComparison != 0) return pageComparison;

        var yComparison = x.Y.CompareTo(y.Y);
        if (yComparison != 0) return yComparison;

        return x.X.CompareTo(y.X);
    }
}
```

## Wichtige Hinweise

- Alle Properties für PDF-Parsing und ML-Nachvollziehbarkeit
- Factory Methods für verschiedene Erstellungs-Szenarien
- Computed Properties für Position und Größe
- Specifications für komplexe Queries
- Comparer für Sortierung und Duplikat-Erkennung
- Validation für Datenintegrität
- Business Methods für ML-spezifische Logik
- Navigation Properties für EF Core
