namespace Invoice.Application.Interfaces;

public interface IParserHelperService
{
    // Date parsing
    Task<DateParseResult> ParseDateAsync(string dateString);
    Task<DateParseResult> ParseDateAsync(string dateString, string format);
    Task<DateParseResult> ParseDateAsync(string dateString, List<string> formats);
    Task<DateParseResult> ParseDateAsync(string dateString, DateParseOptions options);
    Task<List<DateParseResult>> ParseDatesAsync(List<string> dateStrings);
    Task<List<DateParseResult>> ParseDatesAsync(List<string> dateStrings, DateParseOptions options);

    // Date validation
    Task<DateValidationResult> ValidateDateAsync(string dateString);
    Task<DateValidationResult> ValidateDateAsync(DateOnly date);
    Task<DateValidationResult> ValidateDateAsync(DateOnly date, DateValidationOptions options);
    Task<List<DateValidationResult>> ValidateDatesAsync(List<string> dateStrings);
    Task<List<DateValidationResult>> ValidateDatesAsync(List<DateOnly> dates);

    // Date formatting
    Task<string> FormatDateAsync(DateOnly date, string format);
    Task<string> FormatDateAsync(DateOnly date, DateFormatOptions options);
    Task<List<string>> FormatDatesAsync(List<DateOnly> dates, string format);
    Task<List<string>> FormatDatesAsync(List<DateOnly> dates, DateFormatOptions options);

    // Decimal parsing
    Task<DecimalParseResult> ParseDecimalAsync(string decimalString);
    Task<DecimalParseResult> ParseDecimalAsync(string decimalString, string culture);
    Task<DecimalParseResult> ParseDecimalAsync(string decimalString, DecimalParseOptions options);
    Task<List<DecimalParseResult>> ParseDecimalsAsync(List<string> decimalStrings);
    Task<List<DecimalParseResult>> ParseDecimalsAsync(List<string> decimalStrings, DecimalParseOptions options);

    // Decimal validation
    Task<DecimalValidationResult> ValidateDecimalAsync(string decimalString);
    Task<DecimalValidationResult> ValidateDecimalAsync(decimal value);
    Task<DecimalValidationResult> ValidateDecimalAsync(decimal value, DecimalValidationOptions options);
    Task<List<DecimalValidationResult>> ValidateDecimalsAsync(List<string> decimalStrings);
    Task<List<DecimalValidationResult>> ValidateDecimalsAsync(List<decimal> values);

    // Decimal formatting
    Task<string> FormatDecimalAsync(decimal value, string format);
    Task<string> FormatDecimalAsync(decimal value, DecimalFormatOptions options);
    Task<List<string>> FormatDecimalsAsync(List<decimal> values, string format);
    Task<List<string>> FormatDecimalsAsync(List<decimal> values, DecimalFormatOptions options);

    // Address parsing
    Task<AddressParseResult> ParseAddressAsync(string addressString);
    Task<AddressParseResult> ParseAddressAsync(string addressString, string country);
    Task<AddressParseResult> ParseAddressAsync(string addressString, AddressParseOptions options);
    Task<List<AddressParseResult>> ParseAddressesAsync(List<string> addressStrings);
    Task<List<AddressParseResult>> ParseAddressesAsync(List<string> addressStrings, AddressParseOptions options);

    // Address validation
    Task<AddressValidationResult> ValidateAddressAsync(string addressString);
    Task<AddressValidationResult> ValidateAddressAsync(AddressBlock address);
    Task<AddressValidationResult> ValidateAddressAsync(AddressBlock address, AddressValidationOptions options);
    Task<List<AddressValidationResult>> ValidateAddressesAsync(List<string> addressStrings);
    Task<List<AddressValidationResult>> ValidateAddressesAsync(List<AddressBlock> addresses);

    // Address formatting
    Task<string> FormatAddressAsync(AddressBlock address);
    Task<string> FormatAddressAsync(AddressBlock address, AddressFormatOptions options);
    Task<List<string>> FormatAddressesAsync(List<AddressBlock> addresses);
    Task<List<string>> FormatAddressesAsync(List<AddressBlock> addresses, AddressFormatOptions options);

    // Text normalization
    Task<string> NormalizeTextAsync(string text);
    Task<string> NormalizeTextAsync(string text, TextNormalizationOptions options);
    Task<List<string>> NormalizeTextsAsync(List<string> texts);
    Task<List<string>> NormalizeTextsAsync(List<string> texts, TextNormalizationOptions options);

    // Text cleaning
    Task<string> CleanTextAsync(string text);
    Task<string> CleanTextAsync(string text, TextCleaningOptions options);
    Task<List<string>> CleanTextsAsync(List<string> texts);
    Task<List<string>> CleanTextsAsync(List<string> texts, TextCleaningOptions options);

    // Text extraction
    Task<TextExtractionResult> ExtractTextAsync(string text, string pattern);
    Task<TextExtractionResult> ExtractTextAsync(string text, List<string> patterns);
    Task<TextExtractionResult> ExtractTextAsync(string text, TextExtractionOptions options);
    Task<List<TextExtractionResult>> ExtractTextsAsync(List<string> texts, string pattern);
    Task<List<TextExtractionResult>> ExtractTextsAsync(List<string> texts, List<string> patterns);

    // Text validation
    Task<TextValidationResult> ValidateTextAsync(string text);
    Task<TextValidationResult> ValidateTextAsync(string text, TextValidationOptions options);
    Task<List<TextValidationResult>> ValidateTextsAsync(List<string> texts);
    Task<List<TextValidationResult>> ValidateTextsAsync(List<string> texts, TextValidationOptions options);
}

public record DateParseResult(
    bool Success,
    string Message,
    DateOnly? Date,
    string OriginalString,
    string Format,
    string Culture,
    List<DateParseWarning> Warnings,
    List<DateParseError> Errors
);

public record DateParseWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DateParseError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record DateParseOptions(
    List<string> Formats,
    string Culture,
    bool AllowPartialDates,
    bool StrictMode,
    DateOnly? MinDate,
    DateOnly? MaxDate,
    Dictionary<string, object> CustomSettings
);

public record DateValidationResult(
    bool IsValid,
    string Message,
    DateOnly Date,
    string OriginalString,
    List<DateValidationWarning> Warnings,
    List<DateValidationError> Errors
);

public record DateValidationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DateValidationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record DateValidationOptions(
    DateOnly? MinDate,
    DateOnly? MaxDate,
    bool AllowFutureDates,
    bool AllowPastDates,
    int MaxDaysInFuture,
    int MaxDaysInPast,
    Dictionary<string, object> CustomSettings
);

public record DateFormatOptions(
    string Format,
    string Culture,
    bool IncludeTime,
    bool UseShortFormat,
    Dictionary<string, object> CustomSettings
);

public record DecimalParseResult(
    bool Success,
    string Message,
    decimal? Value,
    string OriginalString,
    string Culture,
    List<DecimalParseWarning> Warnings,
    List<DecimalParseError> Errors
);

public record DecimalParseWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DecimalParseError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record DecimalParseOptions(
    string Culture,
    bool AllowNegative,
    bool AllowZero,
    decimal? MinValue,
    decimal? MaxValue,
    int MaxDecimalPlaces,
    Dictionary<string, object> CustomSettings
);

public record DecimalValidationResult(
    bool IsValid,
    string Message,
    decimal Value,
    string OriginalString,
    List<DecimalValidationWarning> Warnings,
    List<DecimalValidationError> Errors
);

public record DecimalValidationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record DecimalValidationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record DecimalValidationOptions(
    decimal? MinValue,
    decimal? MaxValue,
    bool AllowNegative,
    bool AllowZero,
    int MaxDecimalPlaces,
    Dictionary<string, object> CustomSettings
);

public record DecimalFormatOptions(
    string Format,
    string Culture,
    bool IncludeCurrency,
    string CurrencySymbol,
    int DecimalPlaces,
    Dictionary<string, object> CustomSettings
);

public record AddressParseResult(
    bool Success,
    string Message,
    AddressBlock? Address,
    string OriginalString,
    string Country,
    List<AddressParseWarning> Warnings,
    List<AddressParseError> Errors
);

public record AddressParseWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record AddressParseError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record AddressParseOptions(
    string Country,
    bool StrictMode,
    bool AllowPartialAddresses,
    List<string> RequiredFields,
    Dictionary<string, object> CustomSettings
);

public record AddressBlock(
    string Street,
    string HouseNumber,
    string PostalCode,
    string City,
    string State,
    string Country,
    string FullAddress,
    Dictionary<string, string> AdditionalFields
);

public record AddressValidationResult(
    bool IsValid,
    string Message,
    AddressBlock Address,
    string OriginalString,
    List<AddressValidationWarning> Warnings,
    List<AddressValidationError> Errors
);

public record AddressValidationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record AddressValidationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record AddressValidationOptions(
    bool RequireStreet,
    bool RequirePostalCode,
    bool RequireCity,
    bool RequireCountry,
    List<string> AllowedCountries,
    Dictionary<string, object> CustomSettings
);

public record AddressFormatOptions(
    string Format,
    string Country,
    bool IncludeCountry,
    bool UseShortFormat,
    Dictionary<string, object> CustomSettings
);

public record TextNormalizationOptions(
    bool RemoveExtraSpaces,
    bool NormalizeUnicode,
    bool RemoveControlCharacters,
    bool ConvertToLowercase,
    bool RemoveAccents,
    Dictionary<string, object> CustomSettings
);

public record TextCleaningOptions(
    bool RemoveHtml,
    bool RemoveMarkdown,
    bool RemoveSpecialCharacters,
    bool RemoveNumbers,
    bool RemovePunctuation,
    Dictionary<string, object> CustomSettings
);

public record TextExtractionResult(
    bool Success,
    string Message,
    List<string> ExtractedTexts,
    string OriginalText,
    List<TextExtractionWarning> Warnings,
    List<TextExtractionError> Errors
);

public record TextExtractionWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record TextExtractionError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record TextExtractionOptions(
    List<string> Patterns,
    bool CaseSensitive,
    bool Multiline,
    bool Global,
    Dictionary<string, object> CustomSettings
);

public record TextValidationResult(
    bool IsValid,
    string Message,
    string Text,
    List<TextValidationWarning> Warnings,
    List<TextValidationError> Errors
);

public record TextValidationWarning(
    string Code,
    string Message,
    string Field,
    object? Value,
    string? Suggestion
);

public record TextValidationError(
    string Code,
    string Message,
    string Field,
    object? Value,
    Exception? Exception
);

public record TextValidationOptions(
    int MinLength,
    int MaxLength,
    bool AllowEmpty,
    bool AllowWhitespace,
    List<string> AllowedCharacters,
    Dictionary<string, object> CustomSettings
);

