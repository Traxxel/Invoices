namespace Invoice.Domain.Entities;

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

