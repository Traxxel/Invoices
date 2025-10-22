using Invoice.Application.Interfaces;
using Invoice.Application.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Invoice.Infrastructure.Services;

public class TextNormalizationService : ITextNormalizationService
{
    private readonly ILogger<TextNormalizationService> _logger;
    private readonly CultureInfo _germanCulture = new("de-DE");

    public TextNormalizationService(ILogger<TextNormalizationService> logger)
    {
        _logger = logger;
    }

    public async Task<string> NormalizeTextAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text;
            var changes = new List<string>();

            // Unicode normalization
            var temp = await NormalizeUnicodeAsync(normalized);
            if (temp != normalized) { changes.Add("Unicode normalization"); normalized = temp; }

            // Ligature resolution
            temp = await ResolveLigaturesAsync(normalized);
            if (temp != normalized) { changes.Add("Ligature resolution"); normalized = temp; }

            // Quote normalization
            temp = await NormalizeQuotesAsync(normalized);
            if (temp != normalized) { changes.Add("Quote normalization"); normalized = temp; }

            // Dash normalization
            temp = await NormalizeDashesAsync(normalized);
            if (temp != normalized) { changes.Add("Dash normalization"); normalized = temp; }

            // German-specific normalization
            temp = await NormalizeGermanTextAsync(normalized);
            if (temp != normalized) { changes.Add("German text normalization"); normalized = temp; }

            // Decimal separator normalization
            temp = await NormalizeDecimalSeparatorsAsync(normalized);
            if (temp != normalized) { changes.Add("Decimal separator normalization"); normalized = temp; }

            // Date format normalization
            temp = await NormalizeDateFormatsAsync(normalized);
            if (temp != normalized) { changes.Add("Date format normalization"); normalized = temp; }

            // Whitespace normalization
            temp = await NormalizeWhitespaceAsync(normalized);
            if (temp != normalized) { changes.Add("Whitespace normalization"); normalized = temp; }

            if (changes.Any())
            {
                _logger.LogDebug("Text normalized with changes: {Changes}", string.Join(", ", changes));
            }

            return normalized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize text");
            return text;
        }
    }

    public async Task<List<TextLine>> NormalizeTextLinesAsync(List<TextLine> textLines)
    {
        var normalizedLines = new List<TextLine>();

        foreach (var line in textLines)
        {
            try
            {
                var normalizedText = await NormalizeTextAsync(line.Text);
                var normalizedLine = new TextLine
                {
                    Text = normalizedText,
                    X = line.X,
                    Y = line.Y,
                    Width = line.Width,
                    Height = line.Height,
                    FontSize = line.FontSize,
                    FontName = line.FontName,
                    PageNumber = line.PageNumber,
                    LineIndex = line.LineIndex,
                    Words = line.Words
                };

                normalizedLines.Add(normalizedLine);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to normalize text line: {Text}", line.Text);
                normalizedLines.Add(line);
            }
        }

        return normalizedLines;
    }

    public async Task<List<Word>> NormalizeWordsAsync(List<Word> words)
    {
        var normalizedWords = new List<Word>();

        foreach (var word in words)
        {
            try
            {
                var normalizedText = await NormalizeTextAsync(word.Text);
                var normalizedWord = new Word
                {
                    Text = normalizedText,
                    X = word.X,
                    Y = word.Y,
                    Width = word.Width,
                    Height = word.Height,
                    FontSize = word.FontSize,
                    FontName = word.FontName,
                    PageNumber = word.PageNumber,
                    Letters = word.Letters
                };

                normalizedWords.Add(normalizedWord);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to normalize word: {Text}", word.Text);
                normalizedWords.Add(word);
            }
        }

        return normalizedWords;
    }

    public async Task<List<NormalizedTextLine>> FormLinesAsync(List<Word> words)
    {
        try
        {
            var lines = new List<NormalizedTextLine>();
            var currentLine = new List<Word>();
            var lineIndex = 0;
            var currentY = 0f;
            var lineHeight = 0f;

            foreach (var word in words.OrderBy(w => w.Y).ThenBy(w => w.X))
            {
                if (currentLine.Count == 0)
                {
                    currentY = word.Y;
                    lineHeight = word.Height;
                }
                else if (Math.Abs(word.Y - currentY) > lineHeight * 0.5f)
                {
                    // New line
                    if (currentLine.Count > 0)
                    {
                        var line = await CreateNormalizedLineFromWords(currentLine, lineIndex++);
                        lines.Add(line);
                    }
                    currentLine = new List<Word> { word };
                    currentY = word.Y;
                    lineHeight = word.Height;
                }
                else
                {
                    currentLine.Add(word);
                }
            }

            // Add last line
            if (currentLine.Count > 0)
            {
                var line = await CreateNormalizedLineFromWords(currentLine, lineIndex);
                lines.Add(line);
            }

            return lines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to form lines from words");
            return new List<NormalizedTextLine>();
        }
    }

    public async Task<List<NormalizedTextLine>> FormLinesFromTextAsync(string text)
    {
        try
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var normalizedLines = new List<NormalizedTextLine>();

            for (int i = 0; i < lines.Length; i++)
            {
                var normalizedText = await NormalizeTextAsync(lines[i]);
                var line = new NormalizedTextLine
                {
                    Text = normalizedText,
                    OriginalText = lines[i],
                    LineIndex = i,
                    IsNormalized = normalizedText != lines[i]
                };

                if (line.IsNormalized)
                {
                    line.NormalizationChanges.Add("Text normalization");
                }

                normalizedLines.Add(line);
            }

            return normalizedLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to form lines from text");
            return new List<NormalizedTextLine>();
        }
    }

    public async Task<List<NormalizedTextLine>> MergeBrokenLinesAsync(List<NormalizedTextLine> lines)
    {
        try
        {
            var mergedLines = new List<NormalizedTextLine>();
            NormalizedTextLine? currentLine = null;

            foreach (var line in lines.OrderBy(l => l.Y).ThenBy(l => l.X))
            {
                if (currentLine == null)
                {
                    currentLine = line;
                }
                else if (ShouldMergeLines(currentLine, line))
                {
                    // Merge lines
                    currentLine.Text += " " + line.Text;
                    currentLine.OriginalText += " " + line.OriginalText;
                    currentLine.Width = Math.Max(currentLine.Width, line.X + line.Width - currentLine.X);
                    currentLine.Height = Math.Max(currentLine.Height, line.Height);
                    currentLine.NormalizationChanges.Add("Line merging");
                }
                else
                {
                    // Add current line and start new one
                    mergedLines.Add(currentLine);
                    currentLine = line;
                }
            }

            // Add last line
            if (currentLine != null)
            {
                mergedLines.Add(currentLine);
            }

            return mergedLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to merge broken lines");
            return lines;
        }
    }

    public async Task<string> CleanTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove control characters
        text = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

        // Remove excessive whitespace
        text = Regex.Replace(text, @"\s+", " ");

        return await Task.FromResult(text.Trim());
    }

    public async Task<string> RemoveSpecialCharactersAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Keep only letters, digits, spaces, and common punctuation
        text = Regex.Replace(text, @"[^\w\s.,;:!?()\-/€$%]", "");

        return await Task.FromResult(text.Trim());
    }

    public async Task<string> NormalizeWhitespaceAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize all whitespace to single spaces
        text = Regex.Replace(text, @"\s+", " ");

        return await Task.FromResult(text.Trim());
    }

    public async Task<string> NormalizeNumbersAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize number formats
        text = Regex.Replace(text, @"(\d+)\s*,\s*(\d+)", "$1,$2"); // Remove spaces around commas in numbers
        text = Regex.Replace(text, @"(\d+)\s*\.\s*(\d+)", "$1.$2"); // Remove spaces around dots in numbers

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeCurrencyAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize currency symbols
        text = text.Replace("€", "EUR");
        text = text.Replace("$", "USD");
        text = text.Replace("£", "GBP");

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeDatesAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize German date formats
        text = Regex.Replace(text, @"(\d{1,2})\.(\d{1,2})\.(\d{4})", "$1.$2.$3");
        text = Regex.Replace(text, @"(\d{1,2})/(\d{1,2})/(\d{4})", "$1.$2.$3");
        text = Regex.Replace(text, @"(\d{1,2})-(\d{1,2})-(\d{4})", "$1.$2.$3");

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeUnicodeAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Unicode normalization
        text = text.Normalize(NormalizationForm.FormC);

        return await Task.FromResult(text);
    }

    public async Task<string> ResolveLigaturesAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Common ligatures
        text = text.Replace("ﬀ", "ff");
        text = text.Replace("ﬁ", "fi");
        text = text.Replace("ﬂ", "fl");
        text = text.Replace("ﬃ", "ffi");
        text = text.Replace("ﬄ", "ffl");
        text = text.Replace("ﬆ", "st");

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeQuotesAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize quotes
        text = text.Replace("\u201C", "\"");
        text = text.Replace("\u201D", "\"");
        text = text.Replace("\u2018", "'");
        text = text.Replace("\u2019", "'");

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeDashesAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize dashes
        text = text.Replace("–", "-");
        text = text.Replace("—", "-");
        text = text.Replace("―", "-");

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeGermanTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // German-specific normalizations (optional - may not be desired for search)
        // Keeping original text is often better for invoices
        // This is here for ML training purposes if needed

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeDecimalSeparatorsAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize decimal separators for German format
        text = Regex.Replace(text, @"(\d+),(\d{2})\b", "$1.$2"); // Convert comma to dot for decimal separator
        text = Regex.Replace(text, @"(\d+)\.(\d{3})\.(\d{3})", "$1$2$3"); // Remove thousand separators

        return await Task.FromResult(text);
    }

    public async Task<string> NormalizeDateFormatsAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize German date formats (DD.MM.YYYY)
        text = Regex.Replace(text, @"(\d{1,2})\.(\d{1,2})\.(\d{4})", "$1.$2.$3");

        return await Task.FromResult(text);
    }

    private async Task<NormalizedTextLine> CreateNormalizedLineFromWords(List<Word> words, int lineIndex)
    {
        var text = string.Join(" ", words.Select(w => w.Text));
        var normalizedText = await NormalizeTextAsync(text);

        var minX = words.Min(w => w.X);
        var minY = words.Min(w => w.Y);
        var maxX = words.Max(w => w.X + w.Width);
        var maxY = words.Max(w => w.Y + w.Height);

        return new NormalizedTextLine
        {
            Text = normalizedText,
            OriginalText = text,
            X = minX,
            Y = minY,
            Width = maxX - minX,
            Height = maxY - minY,
            PageNumber = words.First().PageNumber,
            LineIndex = lineIndex,
            FontSize = words.First().FontSize,
            FontName = words.First().FontName,
            Words = words.Select(w => new NormalizedWord
            {
                Text = w.Text,
                OriginalText = w.Text,
                X = w.X,
                Y = w.Y,
                Width = w.Width,
                Height = w.Height,
                FontSize = w.FontSize,
                FontName = w.FontName,
                IsNormalized = false
            }).ToList(),
            IsNormalized = normalizedText != text,
            NormalizationChanges = normalizedText != text ? new List<string> { "Text normalization" } : new List<string>()
        };
    }

    private bool ShouldMergeLines(NormalizedTextLine line1, NormalizedTextLine line2)
    {
        // Check if lines should be merged based on position and content
        var verticalDistance = Math.Abs(line1.Y - line2.Y);
        var horizontalOverlap = !(line1.X + line1.Width < line2.X || line2.X + line2.Width < line1.X);

        // Merge if lines are close vertically and have horizontal overlap
        return verticalDistance < line1.Height * 0.5f && horizontalOverlap;
    }
}

