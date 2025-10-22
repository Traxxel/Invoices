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

