# Aufgabe 19: PdfPig Integration und Text-Extraktion

## Ziel

PdfPig-basierte PDF-Parsing-Integration für Text-Extraktion und Layout-Informationen.

## 1. PDF Parser Interface

**Datei:** `src/InvoiceReader.Application/Interfaces/IPdfParserService.cs`

```csharp
namespace InvoiceReader.Application.Interfaces;

public interface IPdfParserService
{
    // Basic parsing
    Task<ParsedDocument> ParseDocumentAsync(string filePath);
    Task<ParsedDocument> ParseDocumentAsync(Stream fileStream);
    Task<ParsedDocument> ParseDocumentAsync(byte[] fileBytes);

    // Document information
    Task<DocumentInfo> GetDocumentInfoAsync(string filePath);
    Task<int> GetPageCountAsync(string filePath);
    Task<bool> IsValidPdfAsync(string filePath);

    // Text extraction
    Task<string> ExtractTextAsync(string filePath);
    Task<string> ExtractTextFromPageAsync(string filePath, int pageNumber);
    Task<List<TextLine>> ExtractTextLinesAsync(string filePath);
    Task<List<TextLine>> ExtractTextLinesFromPageAsync(string filePath, int pageNumber);

    // Layout extraction
    Task<List<TextBlock>> ExtractTextBlocksAsync(string filePath);
    Task<List<TextBlock>> ExtractTextBlocksFromPageAsync(string filePath, int pageNumber);
    Task<List<Word>> ExtractWordsAsync(string filePath);
    Task<List<Word>> ExtractWordsFromPageAsync(string filePath, int pageNumber);

    // Advanced parsing
    Task<List<Table>> ExtractTablesAsync(string filePath);
    Task<List<Table>> ExtractTablesFromPageAsync(string filePath, int pageNumber);
    Task<List<Image>> ExtractImagesAsync(string filePath);
    Task<List<Image>> ExtractImagesFromPageAsync(string filePath, int pageNumber);
}

public class ParsedDocument
{
    public string FilePath { get; set; } = string.Empty;
    public DocumentInfo Info { get; set; } = new();
    public List<Page> Pages { get; set; } = new();
    public string FullText { get; set; } = string.Empty;
    public List<TextLine> AllTextLines { get; set; } = new();
    public List<TextBlock> AllTextBlocks { get; set; } = new();
    public List<Word> AllWords { get; set; } = new();
    public List<Table> Tables { get; set; } = new();
    public List<Image> Images { get; set; } = new();
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentInfo
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
    public int PageCount { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsValid { get; set; }
}

public class Page
{
    public int PageNumber { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<TextLine> TextLines { get; set; } = new();
    public List<TextBlock> TextBlocks { get; set; } = new();
    public List<Word> Words { get; set; } = new();
    public List<Table> Tables { get; set; } = new();
    public List<Image> Images { get; set; } = new();
}

public class TextLine
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int LineIndex { get; set; }
    public List<Word> Words { get; set; } = new();
}

public class TextBlock
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public int PageNumber { get; set; }
    public List<TextLine> Lines { get; set; } = new();
}

public class Word
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public List<Letter> Letters { get; set; } = new();
}

public class Letter
{
    public char Character { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
}

public class Table
{
    public int PageNumber { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public List<TableRow> Rows { get; set; } = new();
    public int ColumnCount { get; set; }
    public int RowCount { get; set; }
}

public class TableRow
{
    public int RowIndex { get; set; }
    public List<TableCell> Cells { get; set; } = new();
}

public class TableCell
{
    public int ColumnIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class Image
{
    public int PageNumber { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
```

## 2. PDF Parser Implementation

**Datei:** `src/InvoiceReader.Infrastructure/Services/PdfPigParserService.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig.Writer;

namespace InvoiceReader.Infrastructure.Services;

public class PdfPigParserService : IPdfParserService
{
    private readonly ILogger<PdfPigParserService> _logger;

    public PdfPigParserService(ILogger<PdfPigParserService> logger)
    {
        _logger = logger;
    }

    public async Task<ParsedDocument> ParseDocumentAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Parsing PDF document: {FilePath}", filePath);

            using var document = PdfDocument.Open(filePath);
            return await ParseDocumentInternalAsync(document, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PDF document: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<ParsedDocument> ParseDocumentAsync(Stream fileStream)
    {
        try
        {
            _logger.LogInformation("Parsing PDF document from stream");

            using var document = PdfDocument.Open(fileStream);
            return await ParseDocumentInternalAsync(document, "stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PDF document from stream");
            throw;
        }
    }

    public async Task<ParsedDocument> ParseDocumentAsync(byte[] fileBytes)
    {
        try
        {
            _logger.LogInformation("Parsing PDF document from bytes");

            using var document = PdfDocument.Open(fileBytes);
            return await ParseDocumentInternalAsync(document, "bytes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PDF document from bytes");
            throw;
        }
    }

    private async Task<ParsedDocument> ParseDocumentInternalAsync(PdfDocument document, string source)
    {
        var parsedDocument = new ParsedDocument
        {
            FilePath = source,
            Info = ExtractDocumentInfo(document),
            Pages = new List<Page>()
        };

        // Extract text from all pages
        var allTextLines = new List<TextLine>();
        var allTextBlocks = new List<TextBlock>();
        var allWords = new List<Word>();
        var allTables = new List<Table>();
        var allImages = new List<Image>();

        for (int pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
        {
            var page = document.GetPage(pageNumber);
            var pageData = await ExtractPageDataAsync(page, pageNumber);

            parsedDocument.Pages.Add(pageData);
            allTextLines.AddRange(pageData.TextLines);
            allTextBlocks.AddRange(pageData.TextBlocks);
            allWords.AddRange(pageData.Words);
            allTables.AddRange(pageData.Tables);
            allImages.AddRange(pageData.Images);
        }

        parsedDocument.AllTextLines = allTextLines;
        parsedDocument.AllTextBlocks = allTextBlocks;
        parsedDocument.AllWords = allWords;
        parsedDocument.Tables = allTables;
        parsedDocument.Images = allImages;
        parsedDocument.FullText = string.Join(Environment.NewLine, allTextLines.Select(tl => tl.Text));

        _logger.LogInformation("Successfully parsed PDF document: {PageCount} pages, {TextLineCount} text lines",
            document.NumberOfPages, allTextLines.Count);

        return parsedDocument;
    }

    private DocumentInfo ExtractDocumentInfo(PdfDocument document)
    {
        var info = document.Information;

        return new DocumentInfo
        {
            Title = info.Title ?? string.Empty,
            Author = info.Author ?? string.Empty,
            Subject = info.Subject ?? string.Empty,
            Creator = info.Creator ?? string.Empty,
            Producer = info.Producer ?? string.Empty,
            CreationDate = info.CreationDate,
            ModificationDate = info.ModificationDate,
            PageCount = document.NumberOfPages,
            Version = document.Version.ToString(),
            IsEncrypted = document.IsEncrypted,
            IsValid = true
        };
    }

    private async Task<Page> ExtractPageDataAsync(PdfPage page, int pageNumber)
    {
        var pageData = new Page
        {
            PageNumber = pageNumber,
            Width = page.Width,
            Height = page.Height,
            TextLines = new List<TextLine>(),
            TextBlocks = new List<TextBlock>(),
            Words = new List<Word>(),
            Tables = new List<Table>(),
            Images = new List<Image>()
        };

        // Extract words
        var words = page.GetWords().ToList();
        var textLines = new List<TextLine>();
        var lineIndex = 0;

        // Group words into lines
        var currentLine = new List<Word>();
        var currentY = 0f;
        var lineHeight = 0f;

        foreach (var word in words.OrderBy(w => w.BoundingBox.Bottom).ThenBy(w => w.BoundingBox.Left))
        {
            if (currentLine.Count == 0)
            {
                currentY = word.BoundingBox.Bottom;
                lineHeight = word.BoundingBox.Height;
            }
            else if (Math.Abs(word.BoundingBox.Bottom - currentY) > lineHeight * 0.5f)
            {
                // New line
                if (currentLine.Count > 0)
                {
                    textLines.Add(CreateTextLineFromWords(currentLine, pageNumber, lineIndex++));
                }
                currentLine = new List<Word> { word };
                currentY = word.BoundingBox.Bottom;
                lineHeight = word.BoundingBox.Height;
            }
            else
            {
                currentLine.Add(word);
            }
        }

        // Add last line
        if (currentLine.Count > 0)
        {
            textLines.Add(CreateTextLineFromWords(currentLine, pageNumber, lineIndex));
        }

        pageData.TextLines = textLines;
        pageData.Words = words.Select(w => ConvertWord(w, pageNumber)).ToList();
        pageData.Text = string.Join(Environment.NewLine, textLines.Select(tl => tl.Text));

        // Extract tables (simplified implementation)
        pageData.Tables = await ExtractTablesFromPageAsync(page, pageNumber);

        // Extract images
        pageData.Images = await ExtractImagesFromPageAsync(page, pageNumber);

        return pageData;
    }

    private TextLine CreateTextLineFromWords(List<Word> words, int pageNumber, int lineIndex)
    {
        var text = string.Join(" ", words.Select(w => w.Text));
        var boundingBox = words.First().BoundingBox;

        foreach (var word in words.Skip(1))
        {
            boundingBox = boundingBox.Union(word.BoundingBox);
        }

        return new TextLine
        {
            Text = text,
            X = boundingBox.Left,
            Y = boundingBox.Bottom,
            Width = boundingBox.Width,
            Height = boundingBox.Height,
            FontSize = words.First().FontSize,
            FontName = words.First().FontName,
            PageNumber = pageNumber,
            LineIndex = lineIndex,
            Words = words.Select(w => ConvertWord(w, pageNumber)).ToList()
        };
    }

    private Word ConvertWord(UglyToad.PdfPig.Content.Word word, int pageNumber)
    {
        return new Word
        {
            Text = word.Text,
            X = word.BoundingBox.Left,
            Y = word.BoundingBox.Bottom,
            Width = word.BoundingBox.Width,
            Height = word.BoundingBox.Height,
            FontSize = word.FontSize,
            FontName = word.FontName,
            PageNumber = pageNumber,
            Letters = word.Letters.Select(l => new Letter
            {
                Character = l.Value,
                X = l.StartBaseLine.X,
                Y = l.StartBaseLine.Y,
                Width = l.Width,
                Height = l.GlyphRectangle.Height,
                FontSize = l.FontSize,
                FontName = l.FontName
            }).ToList()
        };
    }

    private async Task<List<Table>> ExtractTablesFromPageAsync(PdfPage page, int pageNumber)
    {
        // Simplified table extraction
        // In a real implementation, you would use more sophisticated table detection
        var tables = new List<Table>();

        try
        {
            // This is a placeholder - real table extraction would require more complex logic
            // using document layout analysis libraries
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract tables from page {PageNumber}", pageNumber);
        }

        return tables;
    }

    private async Task<List<Image>> ExtractImagesFromPageAsync(PdfPage page, int pageNumber)
    {
        var images = new List<Image>();

        try
        {
            var pageImages = page.GetImages();
            foreach (var image in pageImages)
            {
                images.Add(new Image
                {
                    PageNumber = pageNumber,
                    X = image.BoundsInPdfSpace.Left,
                    Y = image.BoundsInPdfSpace.Bottom,
                    Width = image.BoundsInPdfSpace.Width,
                    Height = image.BoundsInPdfSpace.Height,
                    Name = image.NameInFile,
                    Data = image.RawBytes.ToArray()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract images from page {PageNumber}", pageNumber);
        }

        return images;
    }

    public async Task<DocumentInfo> GetDocumentInfoAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            return ExtractDocumentInfo(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document info: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<int> GetPageCountAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            return document.NumberOfPages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get page count: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> IsValidPdfAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            return document != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var text = string.Join(Environment.NewLine,
                Enumerable.Range(1, document.NumberOfPages)
                    .Select(pageNumber => document.GetPage(pageNumber).Text));
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<string> ExtractTextFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var page = document.GetPage(pageNumber);
            return page.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }

    public async Task<List<TextLine>> ExtractTextLinesAsync(string filePath)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.AllTextLines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text lines: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<TextLine>> ExtractTextLinesFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.TextLines ?? new List<TextLine>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text lines from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }

    public async Task<List<TextBlock>> ExtractTextBlocksAsync(string filePath)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.AllTextBlocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text blocks: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<TextBlock>> ExtractTextBlocksFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.TextBlocks ?? new List<TextBlock>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text blocks from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }

    public async Task<List<Word>> ExtractWordsAsync(string filePath)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.AllWords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract words: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<Word>> ExtractWordsFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.Words ?? new List<Word>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract words from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }

    public async Task<List<Table>> ExtractTablesAsync(string filePath)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract tables: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<Table>> ExtractTablesFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.Tables ?? new List<Table>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract tables from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }

    public async Task<List<Image>> ExtractImagesAsync(string filePath)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Images;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract images: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<List<Image>> ExtractImagesFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.Images ?? new List<Image>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract images from page {PageNumber}: {FilePath}", pageNumber, filePath);
            throw;
        }
    }
}
```

## 3. PDF Parser Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/PdfParserExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class PdfParserExtensions
{
    public static IServiceCollection AddPdfParserServices(this IServiceCollection services)
    {
        services.AddScoped<IPdfParserService, PdfPigParserService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständige PdfPig-Integration für PDF-Parsing
- Text-Extraktion mit Layout-Informationen
- Zeilen-basierte Text-Extraktion für ML-Training
- Word- und Letter-Level-Details für präzise Positionierung
- Document Info-Extraktion für Metadaten
- Table und Image-Extraktion (vereinfacht)
- Error Handling für alle Parsing-Operationen
- Logging für alle PDF-Operationen
- Stream-basierte Parsing für verschiedene Input-Quellen
- Strukturierte Daten-Modelle für alle PDF-Elemente
