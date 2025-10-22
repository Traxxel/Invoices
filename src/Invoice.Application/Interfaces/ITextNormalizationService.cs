using Invoice.Application.Models;

namespace Invoice.Application.Interfaces;

public interface ITextNormalizationService
{
    // Text normalization
    Task<string> NormalizeTextAsync(string text);
    Task<List<TextLine>> NormalizeTextLinesAsync(List<TextLine> textLines);
    Task<List<Word>> NormalizeWordsAsync(List<Word> words);

    // Line formation
    Task<List<NormalizedTextLine>> FormLinesAsync(List<Word> words);
    Task<List<NormalizedTextLine>> FormLinesFromTextAsync(string text);
    Task<List<NormalizedTextLine>> MergeBrokenLinesAsync(List<NormalizedTextLine> lines);

    // Text cleaning
    Task<string> CleanTextAsync(string text);
    Task<string> RemoveSpecialCharactersAsync(string text);
    Task<string> NormalizeWhitespaceAsync(string text);
    Task<string> NormalizeNumbersAsync(string text);
    Task<string> NormalizeCurrencyAsync(string text);
    Task<string> NormalizeDatesAsync(string text);

    // Character normalization
    Task<string> NormalizeUnicodeAsync(string text);
    Task<string> ResolveLigaturesAsync(string text);
    Task<string> NormalizeQuotesAsync(string text);
    Task<string> NormalizeDashesAsync(string text);

    // Language-specific normalization
    Task<string> NormalizeGermanTextAsync(string text);
    Task<string> NormalizeDecimalSeparatorsAsync(string text);
    Task<string> NormalizeDateFormatsAsync(string text);
}

