using Invoice.Application.Interfaces;
using Invoice.Application.Models;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Invoice.Infrastructure.Services;

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
            Pages = new List<Application.Models.Page>()
        };

        // Extract text from all pages
        var allTextLines = new List<Application.Models.TextLine>();
        var allTextBlocks = new List<Application.Models.TextBlock>();
        var allWords = new List<Application.Models.Word>();
        var allTables = new List<Application.Models.Table>();
        var allImages = new List<Application.Models.Image>();

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
            CreationDate = TryParseDate(info.CreationDate),
            ModificationDate = TryParseDate(info.ModifiedDate),
            PageCount = document.NumberOfPages,
            Version = document.Version.ToString(),
            IsEncrypted = document.IsEncrypted,
            IsValid = true
        };
    }

    private async Task<Application.Models.Page> ExtractPageDataAsync(UglyToad.PdfPig.Content.Page page, int pageNumber)
    {
        var pageData = new Application.Models.Page
        {
            PageNumber = pageNumber,
            Width = (float)page.Width,
            Height = (float)page.Height,
            TextLines = new List<Application.Models.TextLine>(),
            TextBlocks = new List<Application.Models.TextBlock>(),
            Words = new List<Application.Models.Word>(),
            Tables = new List<Application.Models.Table>(),
            Images = new List<Application.Models.Image>()
        };

        // Extract words
        var words = page.GetWords().ToList();
        var textLines = new List<Application.Models.TextLine>();
        var lineIndex = 0;

        // Group words into lines
        var currentLine = new List<UglyToad.PdfPig.Content.Word>();
        var currentY = 0.0;
        var lineHeight = 0.0;

        foreach (var word in words.OrderBy(w => w.BoundingBox.Bottom).ThenBy(w => w.BoundingBox.Left))
        {
            if (currentLine.Count == 0)
            {
                currentY = word.BoundingBox.Bottom;
                lineHeight = word.BoundingBox.Height;
            }
            else if (Math.Abs(word.BoundingBox.Bottom - currentY) > lineHeight * 0.5)
            {
                // New line
                if (currentLine.Count > 0)
                {
                    textLines.Add(CreateTextLineFromWords(currentLine, pageNumber, lineIndex++));
                }
                currentLine = new List<UglyToad.PdfPig.Content.Word> { word };
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

        return await Task.FromResult(pageData);
    }

    private TextLine CreateTextLineFromWords(List<UglyToad.PdfPig.Content.Word> words, int pageNumber, int lineIndex)
    {
        var text = string.Join(" ", words.Select(w => w.Text));
        var boundingBox = words.First().BoundingBox;

        foreach (var word in words.Skip(1))
        {
            // Union of bounding boxes
            var left = Math.Min(boundingBox.Left, word.BoundingBox.Left);
            var bottom = Math.Min(boundingBox.Bottom, word.BoundingBox.Bottom);
            var right = Math.Max(boundingBox.Right, word.BoundingBox.Right);
            var top = Math.Max(boundingBox.Top, word.BoundingBox.Top);
            
            boundingBox = new UglyToad.PdfPig.Core.PdfRectangle(left, bottom, right, top);
        }

        return new TextLine
        {
            Text = text,
            X = (float)boundingBox.Left,
            Y = (float)boundingBox.Bottom,
            Width = (float)boundingBox.Width,
            Height = (float)boundingBox.Height,
            FontSize = words.First().Letters.Any() ? (float)words.First().Letters.First().FontSize : 0f,
            FontName = words.First().Letters.Any() ? words.First().Letters.First().FontName : string.Empty,
            PageNumber = pageNumber,
            LineIndex = lineIndex,
            Words = words.Select(w => ConvertWord(w, pageNumber)).ToList()
        };
    }

    private Application.Models.Word ConvertWord(UglyToad.PdfPig.Content.Word word, int pageNumber)
    {
        return new Application.Models.Word
        {
            Text = word.Text,
            X = (float)word.BoundingBox.Left,
            Y = (float)word.BoundingBox.Bottom,
            Width = (float)word.BoundingBox.Width,
            Height = (float)word.BoundingBox.Height,
            FontSize = (float)(word.Letters.FirstOrDefault()?.FontSize ?? 0),
            FontName = word.Letters.FirstOrDefault()?.FontName ?? string.Empty,
            PageNumber = pageNumber,
            Letters = word.Letters.Select(l => new Application.Models.Letter
            {
                Character = l.Value.Length > 0 ? l.Value[0] : ' ',
                X = (float)l.Location.X,
                Y = (float)l.Location.Y,
                Width = (float)l.Width,
                Height = (float)l.GlyphRectangle.Height,
                FontSize = (float)l.FontSize,
                FontName = l.FontName
            }).ToList()
        };
    }

    private async Task<List<Table>> ExtractTablesFromPageAsync(UglyToad.PdfPig.Content.Page page, int pageNumber)
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

        return await Task.FromResult(tables);
    }

    private async Task<List<Image>> ExtractImagesFromPageAsync(UglyToad.PdfPig.Content.Page page, int pageNumber)
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
                    X = (float)image.Bounds.Left,
                    Y = (float)image.Bounds.Bottom,
                    Width = (float)image.Bounds.Width,
                    Height = (float)image.Bounds.Height,
                    Name = $"Image_{pageNumber}_{images.Count}",
                    Data = image.RawBytes.ToArray()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract images from page {PageNumber}", pageNumber);
        }

        return await Task.FromResult(images);
    }

    public async Task<DocumentInfo> GetDocumentInfoAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            return await Task.FromResult(ExtractDocumentInfo(document));
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
            return await Task.FromResult(document.NumberOfPages);
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
            return await Task.FromResult(document != null);
        }
        catch
        {
            return await Task.FromResult(false);
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
            return await Task.FromResult(text);
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
            return await Task.FromResult(page.Text);
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

    public async Task<List<Application.Models.Word>> ExtractWordsAsync(string filePath)
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

    public async Task<List<Application.Models.Word>> ExtractWordsFromPageAsync(string filePath, int pageNumber)
    {
        try
        {
            var parsedDocument = await ParseDocumentAsync(filePath);
            return parsedDocument.Pages.FirstOrDefault(p => p.PageNumber == pageNumber)?.Words ?? new List<Application.Models.Word>();
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

    private DateTime? TryParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // PDF date format: D:YYYYMMDDHHmmSSOHH'mm'
        try
        {
            if (dateString.StartsWith("D:"))
            {
                dateString = dateString.Substring(2);
                
                // Extract year, month, day, hour, minute, second
                if (dateString.Length >= 14)
                {
                    int year = int.Parse(dateString.Substring(0, 4));
                    int month = int.Parse(dateString.Substring(4, 2));
                    int day = int.Parse(dateString.Substring(6, 2));
                    int hour = int.Parse(dateString.Substring(8, 2));
                    int minute = int.Parse(dateString.Substring(10, 2));
                    int second = int.Parse(dateString.Substring(12, 2));

                    return new DateTime(year, month, day, hour, minute, second);
                }
            }

            // Try standard DateTime parsing as fallback
            if (DateTime.TryParse(dateString, out var result))
                return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse PDF date string: {DateString}", dateString);
        }

        return null;
    }
}

