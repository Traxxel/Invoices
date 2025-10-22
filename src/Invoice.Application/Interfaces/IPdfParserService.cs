using Invoice.Application.Models;

namespace Invoice.Application.Interfaces;

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

