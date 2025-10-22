# Aufgabe 31: Regex-Patterns für InvoiceNumber, Date, Amount

## Ziel

Regex-Patterns für die Extraktion und Validierung von Rechnungsnummern, Datumsangaben und Beträgen.

## 1. Regex Pattern Service Interface

**Datei:** `src/Invoice.Application/Interfaces/IRegexPatternService.cs`

```csharp
namespace Invoice.Application.Interfaces;

public interface IRegexPatternService
{
    // Pattern retrieval
    Task<RegexPattern> GetPatternAsync(string patternName);
    Task<List<RegexPattern>> GetPatternsAsync(string category);
    Task<List<RegexPattern>> GetAllPatternsAsync();
    Task<List<RegexPattern>> GetPatternsByLanguageAsync(string language);

    // Pattern validation
    Task<PatternValidationResult> ValidatePatternAsync(string patternName, string testText);
    Task<PatternValidationResult> ValidatePatternAsync(RegexPattern pattern, string testText);
    Task<List<PatternValidationResult>> ValidateAllPatternsAsync(string testText);

    // Pattern matching
    Task<PatternMatchResult> MatchPatternAsync(string patternName, string text);
    Task<PatternMatchResult> MatchPatternAsync(RegexPattern pattern, string text);
    Task<List<PatternMatchResult>> MatchAllPatternsAsync(string text);
    Task<List<PatternMatchResult>> MatchPatternsAsync(List<string> patternNames, string text);

    // Pattern compilation
    Task<CompiledPattern> CompilePatternAsync(string patternName);
    Task<CompiledPattern> CompilePatternAsync(RegexPattern pattern);
    Task<List<CompiledPattern>> CompilePatternsAsync(List<string> patternNames);
    Task<List<CompiledPattern>> CompileAllPatternsAsync();

    // Pattern management
    Task<bool> AddPatternAsync(RegexPattern pattern);
    Task<bool> UpdatePatternAsync(RegexPattern pattern);
    Task<bool> DeletePatternAsync(string patternName);
    Task<bool> EnablePatternAsync(string patternName);
    Task<bool> DisablePatternAsync(string patternName);

    // Pattern statistics
    Task<PatternStatistics> GetPatternStatisticsAsync(string patternName);
    Task<PatternStatistics> GetPatternStatisticsAsync(RegexPattern pattern);
    Task<List<PatternStatistics>> GetAllPatternStatisticsAsync();
    Task<PatternUsageStatistics> GetPatternUsageStatisticsAsync();
}

public record RegexPattern(
    string Name,
    string Description,
    string Pattern,
    string Category,
    string Language,
    int Priority,
    bool IsEnabled,
    Dictionary<string, object> Parameters,
    List<string> Examples,
    List<string> TestCases,
    DateTime CreatedAt,
    DateTime LastModified
);

public record PatternValidationResult(
    bool IsValid,
    string Message,
    string PatternName,
    string TestText,
    List<PatternMatch> Matches,
    List<PatternWarning> Warnings,
    List<PatternError> Errors
);

public record PatternMatch(
    string Value,
    int StartIndex,
    int Length,
    string GroupName,
    Dictionary<string, string> Groups,
    float Confidence,
    string Source
);

public record PatternWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record PatternError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record PatternMatchResult(
    bool Success,
    string Message,
    string PatternName,
    string Text,
    List<PatternMatch> Matches,
    List<PatternWarning> Warnings,
    List<PatternError> Errors,
    TimeSpan MatchTime
);

public record CompiledPattern(
    string Name,
    System.Text.RegularExpressions.Regex Regex,
    RegexPattern Pattern,
    DateTime CompiledAt,
    Dictionary<string, object> Metadata
);

public record PatternStatistics(
    string PatternName,
    int TotalMatches,
    int SuccessfulMatches,
    int FailedMatches,
    float SuccessRate,
    TimeSpan AverageMatchTime,
    TimeSpan TotalMatchTime,
    Dictionary<string, int> MatchesByCategory,
    Dictionary<string, float> ConfidenceByCategory,
    DateTime LastUsed,
    int UsageCount
);

public record PatternUsageStatistics(
    int TotalPatterns,
    int EnabledPatterns,
    int DisabledPatterns,
    int TotalMatches,
    int SuccessfulMatches,
    int FailedMatches,
    float OverallSuccessRate,
    TimeSpan TotalMatchTime,
    Dictionary<string, int> UsageByCategory,
    Dictionary<string, int> UsageByLanguage,
    DateTime LastUpdated
);
```

## 2. Regex Pattern Service Implementation

**Datei:** `src/Invoice.Application/Services/RegexPatternService.cs`

```csharp
using Invoice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Invoice.Application.Services;

public class RegexPatternService : IRegexPatternService
{
    private readonly ILogger<RegexPatternService> _logger;
    private readonly Dictionary<string, RegexPattern> _patterns;
    private readonly Dictionary<string, CompiledPattern> _compiledPatterns;

    public RegexPatternService(ILogger<RegexPatternService> logger)
    {
        _logger = logger;
        _patterns = new Dictionary<string, RegexPattern>();
        _compiledPatterns = new Dictionary<string, CompiledPattern>();

        InitializePatterns();
    }

    public async Task<RegexPattern> GetPatternAsync(string patternName)
    {
        try
        {
            if (_patterns.TryGetValue(patternName, out var pattern))
            {
                return pattern;
            }

            _logger.LogWarning("Pattern not found: {PatternName}", patternName);
            return new RegexPattern("", "", "", "", "", 0, false, new Dictionary<string, object>(), new List<string>(), new List<string>(), DateTime.MinValue, DateTime.MinValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pattern: {PatternName}", patternName);
            throw;
        }
    }

    public async Task<List<RegexPattern>> GetPatternsAsync(string category)
    {
        try
        {
            return _patterns.Values
                .Where(p => p.Category == category && p.IsEnabled)
                .OrderBy(p => p.Priority)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patterns for category: {Category}", category);
            return new List<RegexPattern>();
        }
    }

    public async Task<List<RegexPattern>> GetAllPatternsAsync()
    {
        try
        {
            return _patterns.Values
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Priority)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all patterns");
            return new List<RegexPattern>();
        }
    }

    public async Task<List<RegexPattern>> GetPatternsByLanguageAsync(string language)
    {
        try
        {
            return _patterns.Values
                .Where(p => p.Language == language && p.IsEnabled)
                .OrderBy(p => p.Priority)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patterns for language: {Language}", language);
            return new List<RegexPattern>();
        }
    }

    public async Task<PatternValidationResult> ValidatePatternAsync(string patternName, string testText)
    {
        try
        {
            var pattern = await GetPatternAsync(patternName);
            return await ValidatePatternAsync(pattern, testText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate pattern: {PatternName}", patternName);
            return new PatternValidationResult(false, "Pattern validation failed", patternName, testText, new List<PatternMatch>(), new List<PatternWarning>(), new List<PatternError> { new PatternError("VALIDATION_FAILED", ex.Message, "Pattern", patternName, ex) });
        }
    }

    public async Task<PatternValidationResult> ValidatePatternAsync(RegexPattern pattern, string testText)
    {
        try
        {
            var matches = new List<PatternMatch>();
            var warnings = new List<PatternWarning>();
            var errors = new List<PatternError>();

            if (string.IsNullOrWhiteSpace(pattern.Pattern))
            {
                errors.Add(new PatternError("EMPTY_PATTERN", "Pattern is empty", "Pattern", pattern.Pattern, null));
                return new PatternValidationResult(false, "Pattern validation failed", pattern.Name, testText, matches, warnings, errors);
            }

            try
            {
                var regex = new Regex(pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var regexMatches = regex.Matches(testText);

                foreach (Match match in regexMatches)
                {
                    var groups = new Dictionary<string, string>();
                    foreach (Group group in match.Groups)
                    {
                        if (!string.IsNullOrEmpty(group.Name) && group.Name != "0")
                        {
                            groups[group.Name] = group.Value;
                        }
                    }

                    matches.Add(new PatternMatch(
                        match.Value,
                        match.Index,
                        match.Length,
                        match.Groups.Count > 1 ? match.Groups[1].Name : "Value",
                        groups,
                        1.0f,
                        "Regex"
                    ));
                }

                return new PatternValidationResult(true, "Pattern validation successful", pattern.Name, testText, matches, warnings, errors);
            }
            catch (ArgumentException ex)
            {
                errors.Add(new PatternError("INVALID_PATTERN", "Invalid regex pattern", "Pattern", pattern.Pattern, ex));
                return new PatternValidationResult(false, "Pattern validation failed", pattern.Name, testText, matches, warnings, errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate pattern: {PatternName}", pattern.Name);
            return new PatternValidationResult(false, "Pattern validation failed", pattern.Name, testText, new List<PatternMatch>(), new List<PatternWarning>(), new List<PatternError> { new PatternError("VALIDATION_FAILED", ex.Message, "Pattern", pattern.Name, ex) });
        }
    }

    public async Task<List<PatternValidationResult>> ValidateAllPatternsAsync(string testText)
    {
        try
        {
            var results = new List<PatternValidationResult>();

            foreach (var pattern in _patterns.Values.Where(p => p.IsEnabled))
            {
                var result = await ValidatePatternAsync(pattern, testText);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate all patterns");
            return new List<PatternValidationResult>();
        }
    }

    public async Task<PatternMatchResult> MatchPatternAsync(string patternName, string text)
    {
        try
        {
            var pattern = await GetPatternAsync(patternName);
            return await MatchPatternAsync(pattern, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match pattern: {PatternName}", patternName);
            return new PatternMatchResult(false, "Pattern matching failed", patternName, text, new List<PatternMatch>(), new List<PatternWarning>(), new List<PatternError> { new PatternError("MATCH_FAILED", ex.Message, "Pattern", patternName, ex) }, TimeSpan.Zero);
        }
    }

    public async Task<PatternMatchResult> MatchPatternAsync(RegexPattern pattern, string text)
    {
        var startTime = DateTime.UtcNow;
        var matches = new List<PatternMatch>();
        var warnings = new List<PatternWarning>();
        var errors = new List<PatternError>();

        try
        {
            if (string.IsNullOrWhiteSpace(pattern.Pattern))
            {
                errors.Add(new PatternError("EMPTY_PATTERN", "Pattern is empty", "Pattern", pattern.Pattern, null));
                return new PatternMatchResult(false, "Pattern matching failed", pattern.Name, text, matches, warnings, errors, DateTime.UtcNow - startTime);
            }

            var regex = new Regex(pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var regexMatches = regex.Matches(text);

            foreach (Match match in regexMatches)
            {
                var groups = new Dictionary<string, string>();
                foreach (Group group in match.Groups)
                {
                    if (!string.IsNullOrEmpty(group.Name) && group.Name != "0")
                    {
                        groups[group.Name] = group.Value;
                    }
                }

                matches.Add(new PatternMatch(
                    match.Value,
                    match.Index,
                    match.Length,
                    match.Groups.Count > 1 ? match.Groups[1].Name : "Value",
                    groups,
                    1.0f,
                    "Regex"
                ));
            }

            return new PatternMatchResult(true, "Pattern matching successful", pattern.Name, text, matches, warnings, errors, DateTime.UtcNow - startTime);
        }
        catch (ArgumentException ex)
        {
            errors.Add(new PatternError("INVALID_PATTERN", "Invalid regex pattern", "Pattern", pattern.Pattern, ex));
            return new PatternMatchResult(false, "Pattern matching failed", pattern.Name, text, matches, warnings, errors, DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match pattern: {PatternName}", pattern.Name);
            return new PatternMatchResult(false, "Pattern matching failed", pattern.Name, text, matches, warnings, errors, DateTime.UtcNow - startTime);
        }
    }

    public async Task<List<PatternMatchResult>> MatchAllPatternsAsync(string text)
    {
        try
        {
            var results = new List<PatternMatchResult>();

            foreach (var pattern in _patterns.Values.Where(p => p.IsEnabled))
            {
                var result = await MatchPatternAsync(pattern, text);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match all patterns");
            return new List<PatternMatchResult>();
        }
    }

    public async Task<List<PatternMatchResult>> MatchPatternsAsync(List<string> patternNames, string text)
    {
        try
        {
            var results = new List<PatternMatchResult>();

            foreach (var patternName in patternNames)
            {
                var result = await MatchPatternAsync(patternName, text);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match patterns: {PatternNames}", string.Join(", ", patternNames));
            return new List<PatternMatchResult>();
        }
    }

    public async Task<CompiledPattern> CompilePatternAsync(string patternName)
    {
        try
        {
            if (_compiledPatterns.TryGetValue(patternName, out var compiledPattern))
            {
                return compiledPattern;
            }

            var pattern = await GetPatternAsync(patternName);
            return await CompilePatternAsync(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile pattern: {PatternName}", patternName);
            throw;
        }
    }

    public async Task<CompiledPattern> CompilePatternAsync(RegexPattern pattern)
    {
        try
        {
            var regex = new Regex(pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var compiledPattern = new CompiledPattern(
                pattern.Name,
                regex,
                pattern,
                DateTime.UtcNow,
                new Dictionary<string, object>()
            );

            _compiledPatterns[pattern.Name] = compiledPattern;
            return compiledPattern;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile pattern: {PatternName}", pattern.Name);
            throw;
        }
    }

    public async Task<List<CompiledPattern>> CompilePatternsAsync(List<string> patternNames)
    {
        try
        {
            var compiledPatterns = new List<CompiledPattern>();

            foreach (var patternName in patternNames)
            {
                var compiledPattern = await CompilePatternAsync(patternName);
                compiledPatterns.Add(compiledPattern);
            }

            return compiledPatterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile patterns: {PatternNames}", string.Join(", ", patternNames));
            return new List<CompiledPattern>();
        }
    }

    public async Task<List<CompiledPattern>> CompileAllPatternsAsync()
    {
        try
        {
            var compiledPatterns = new List<CompiledPattern>();

            foreach (var pattern in _patterns.Values.Where(p => p.IsEnabled))
            {
                var compiledPattern = await CompilePatternAsync(pattern);
                compiledPatterns.Add(compiledPattern);
            }

            return compiledPatterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile all patterns");
            return new List<CompiledPattern>();
        }
    }

    public async Task<bool> AddPatternAsync(RegexPattern pattern)
    {
        try
        {
            _patterns[pattern.Name] = pattern;
            _logger.LogInformation("Pattern added: {PatternName}", pattern.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add pattern: {PatternName}", pattern.Name);
            return false;
        }
    }

    public async Task<bool> UpdatePatternAsync(RegexPattern pattern)
    {
        try
        {
            _patterns[pattern.Name] = pattern;
            _compiledPatterns.Remove(pattern.Name); // Remove compiled version to force recompilation
            _logger.LogInformation("Pattern updated: {PatternName}", pattern.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update pattern: {PatternName}", pattern.Name);
            return false;
        }
    }

    public async Task<bool> DeletePatternAsync(string patternName)
    {
        try
        {
            _patterns.Remove(patternName);
            _compiledPatterns.Remove(patternName);
            _logger.LogInformation("Pattern deleted: {PatternName}", patternName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete pattern: {PatternName}", patternName);
            return false;
        }
    }

    public async Task<bool> EnablePatternAsync(string patternName)
    {
        try
        {
            if (_patterns.TryGetValue(patternName, out var pattern))
            {
                var updatedPattern = pattern with { IsEnabled = true };
                _patterns[patternName] = updatedPattern;
                _logger.LogInformation("Pattern enabled: {PatternName}", patternName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable pattern: {PatternName}", patternName);
            return false;
        }
    }

    public async Task<bool> DisablePatternAsync(string patternName)
    {
        try
        {
            if (_patterns.TryGetValue(patternName, out var pattern))
            {
                var updatedPattern = pattern with { IsEnabled = false };
                _patterns[patternName] = updatedPattern;
                _logger.LogInformation("Pattern disabled: {PatternName}", patternName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable pattern: {PatternName}", patternName);
            return false;
        }
    }

    public async Task<PatternStatistics> GetPatternStatisticsAsync(string patternName)
    {
        try
        {
            // This would typically come from a database or statistics service
            return new PatternStatistics(
                patternName,
                0, // TotalMatches
                0, // SuccessfulMatches
                0, // FailedMatches
                0f, // SuccessRate
                TimeSpan.Zero, // AverageMatchTime
                TimeSpan.Zero, // TotalMatchTime
                new Dictionary<string, int>(), // MatchesByCategory
                new Dictionary<string, float>(), // ConfidenceByCategory
                DateTime.MinValue, // LastUsed
                0 // UsageCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pattern statistics: {PatternName}", patternName);
            return new PatternStatistics("", 0, 0, 0, 0f, TimeSpan.Zero, TimeSpan.Zero, new Dictionary<string, int>(), new Dictionary<string, float>(), DateTime.MinValue, 0);
        }
    }

    public async Task<PatternStatistics> GetPatternStatisticsAsync(RegexPattern pattern)
    {
        return await GetPatternStatisticsAsync(pattern.Name);
    }

    public async Task<List<PatternStatistics>> GetAllPatternStatisticsAsync()
    {
        try
        {
            var statistics = new List<PatternStatistics>();

            foreach (var pattern in _patterns.Values)
            {
                var stats = await GetPatternStatisticsAsync(pattern);
                statistics.Add(stats);
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all pattern statistics");
            return new List<PatternStatistics>();
        }
    }

    public async Task<PatternUsageStatistics> GetPatternUsageStatisticsAsync()
    {
        try
        {
            // This would typically come from a database or statistics service
            return new PatternUsageStatistics(
                _patterns.Count, // TotalPatterns
                _patterns.Values.Count(p => p.IsEnabled), // EnabledPatterns
                _patterns.Values.Count(p => !p.IsEnabled), // DisabledPatterns
                0, // TotalMatches
                0, // SuccessfulMatches
                0, // FailedMatches
                0f, // OverallSuccessRate
                TimeSpan.Zero, // TotalMatchTime
                new Dictionary<string, int>(), // UsageByCategory
                new Dictionary<string, int>(), // UsageByLanguage
                DateTime.UtcNow // LastUpdated
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pattern usage statistics");
            return new PatternUsageStatistics(0, 0, 0, 0, 0, 0, 0f, TimeSpan.Zero, new Dictionary<string, int>(), new Dictionary<string, int>(), DateTime.MinValue);
        }
    }

    private void InitializePatterns()
    {
        // Invoice Number Patterns
        AddInvoiceNumberPatterns();

        // Date Patterns
        AddDatePatterns();

        // Amount Patterns
        AddAmountPatterns();

        // Address Patterns
        AddAddressPatterns();

        // VAT Patterns
        AddVatPatterns();
    }

    private void AddInvoiceNumberPatterns()
    {
        // German invoice number patterns
        var germanInvoicePattern = new RegexPattern(
            "GermanInvoiceNumber",
            "German invoice number pattern",
            @"(?:Rechnung|Rechn\.|Rg\.|Nr\.|No\.)\s*:?\s*([A-Z0-9\-/]+)",
            "InvoiceNumber",
            "de",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "Rechnung: 2023-001", "Rg. 2023/001", "Nr. 2023-001" },
            new List<string> { "Rechnung: 2023-001", "Rg. 2023/001", "Nr. 2023-001" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GermanInvoiceNumber"] = germanInvoicePattern;

        // English invoice number patterns
        var englishInvoicePattern = new RegexPattern(
            "EnglishInvoiceNumber",
            "English invoice number pattern",
            @"(?:Invoice|Inv\.|No\.|Number)\s*:?\s*([A-Z0-9\-/]+)",
            "InvoiceNumber",
            "en",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "Invoice: 2023-001", "Inv. 2023/001", "No. 2023-001" },
            new List<string> { "Invoice: 2023-001", "Inv. 2023/001", "No. 2023-001" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["EnglishInvoiceNumber"] = englishInvoicePattern;

        // Generic invoice number pattern
        var genericInvoicePattern = new RegexPattern(
            "GenericInvoiceNumber",
            "Generic invoice number pattern",
            @"(?:[A-Z]{2,4}[-/]?\d{4,8})|(?:\d{4,8}[-/]?[A-Z]{2,4})",
            "InvoiceNumber",
            "generic",
            2,
            true,
            new Dictionary<string, object>(),
            new List<string> { "INV-2023-001", "2023-001-INV", "INV2023001" },
            new List<string> { "INV-2023-001", "2023-001-INV", "INV2023001" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GenericInvoiceNumber"] = genericInvoicePattern;
    }

    private void AddDatePatterns()
    {
        // German date patterns
        var germanDatePattern = new RegexPattern(
            "GermanDate",
            "German date pattern (DD.MM.YYYY)",
            @"(\d{1,2}\.\d{1,2}\.\d{4})",
            "Date",
            "de",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "15.03.2023", "1.1.2023", "31.12.2023" },
            new List<string> { "15.03.2023", "1.1.2023", "31.12.2023" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GermanDate"] = germanDatePattern;

        // English date patterns
        var englishDatePattern = new RegexPattern(
            "EnglishDate",
            "English date pattern (MM/DD/YYYY or YYYY-MM-DD)",
            @"(\d{1,2}/\d{1,2}/\d{4}|\d{4}-\d{1,2}-\d{1,2})",
            "Date",
            "en",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "03/15/2023", "2023-03-15", "3/15/2023" },
            new List<string> { "03/15/2023", "2023-03-15", "3/15/2023" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["EnglishDate"] = englishDatePattern;

        // ISO date pattern
        var isoDatePattern = new RegexPattern(
            "ISODate",
            "ISO date pattern (YYYY-MM-DD)",
            @"(\d{4}-\d{2}-\d{2})",
            "Date",
            "iso",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "2023-03-15", "2023-12-31" },
            new List<string> { "2023-03-15", "2023-12-31" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["ISODate"] = isoDatePattern;
    }

    private void AddAmountPatterns()
    {
        // German amount patterns
        var germanAmountPattern = new RegexPattern(
            "GermanAmount",
            "German amount pattern (€ 1.234,56)",
            @"(?:€\s*)?(\d{1,3}(?:\.\d{3})*(?:,\d{2})?)",
            "Amount",
            "de",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "€ 1.234,56", "1234,56", "1.234,56 €" },
            new List<string> { "€ 1.234,56", "1234,56", "1.234,56 €" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GermanAmount"] = germanAmountPattern;

        // English amount patterns
        var englishAmountPattern = new RegexPattern(
            "EnglishAmount",
            "English amount pattern ($ 1,234.56)",
            @"(?:\$\s*)?(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)",
            "Amount",
            "en",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "$ 1,234.56", "1234.56", "1,234.56 $" },
            new List<string> { "$ 1,234.56", "1234.56", "1,234.56 $" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["EnglishAmount"] = englishAmountPattern;

        // Generic amount pattern
        var genericAmountPattern = new RegexPattern(
            "GenericAmount",
            "Generic amount pattern",
            @"(?:[€$]\s*)?(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2})?)",
            "Amount",
            "generic",
            2,
            true,
            new Dictionary<string, object>(),
            new List<string> { "€ 1.234,56", "$ 1,234.56", "1234.56" },
            new List<string> { "€ 1.234,56", "$ 1,234.56", "1234.56" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GenericAmount"] = genericAmountPattern;
    }

    private void AddAddressPatterns()
    {
        // German address pattern
        var germanAddressPattern = new RegexPattern(
            "GermanAddress",
            "German address pattern",
            @"([A-Za-zäöüßÄÖÜ\s]+)\s*(\d{5})\s*([A-Za-zäöüßÄÖÜ\s]+)",
            "Address",
            "de",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "Musterstraße 123, 12345 Musterstadt" },
            new List<string> { "Musterstraße 123, 12345 Musterstadt" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GermanAddress"] = germanAddressPattern;

        // English address pattern
        var englishAddressPattern = new RegexPattern(
            "EnglishAddress",
            "English address pattern",
            @"([A-Za-z\s]+)\s*(\d{5}(?:-\d{4})?)\s*([A-Za-z\s]+)",
            "Address",
            "en",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "123 Main Street, 12345 City" },
            new List<string> { "123 Main Street, 12345 City" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["EnglishAddress"] = englishAddressPattern;
    }

    private void AddVatPatterns()
    {
        // German VAT pattern
        var germanVatPattern = new RegexPattern(
            "GermanVAT",
            "German VAT pattern",
            @"(?:MwSt\.|USt\.|Umsatzsteuer)\s*:?\s*(\d{1,2}(?:,\d{2})?%)",
            "VAT",
            "de",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "MwSt. 19%", "USt. 19%", "Umsatzsteuer 19%" },
            new List<string> { "MwSt. 19%", "USt. 19%", "Umsatzsteuer 19%" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["GermanVAT"] = germanVatPattern;

        // English VAT pattern
        var englishVatPattern = new RegexPattern(
            "EnglishVAT",
            "English VAT pattern",
            @"(?:VAT|Tax)\s*:?\s*(\d{1,2}(?:\.\d{2})?%)",
            "VAT",
            "en",
            1,
            true,
            new Dictionary<string, object>(),
            new List<string> { "VAT 20%", "Tax 20%", "VAT: 20%" },
            new List<string> { "VAT 20%", "Tax 20%", "VAT: 20%" },
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _patterns["EnglishVAT"] = englishVatPattern;
    }
}
```

## 3. Regex Pattern Service Extensions

**Datei:** `src/Invoice.Application/Extensions/RegexPatternExtensions.cs`

```csharp
using Invoice.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Application.Extensions;

public static class RegexPatternExtensions
{
    public static IServiceCollection AddRegexPatternServices(this IServiceCollection services)
    {
        services.AddSingleton<IRegexPatternService, RegexPatternService>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständiger Regex Pattern Service für Pattern-Management
- Pattern für InvoiceNumber, Date, Amount, Address, VAT
- Deutsche und englische Pattern-Varianten
- Pattern Validation und Matching
- Pattern Compilation für Performance
- Pattern Statistics für Monitoring
- Error Handling für alle Pattern-Operationen
- Logging für alle Pattern-Operationen
- Pattern Categories und Languages
- Pattern Priority für Reihenfolge
- Pattern Examples und Test Cases
