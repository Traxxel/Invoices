using Invoice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Invoice.Application.Services;

public class ParserHelperService : IParserHelperService
{
    private readonly ILogger<ParserHelperService> _logger;
    private readonly Dictionary<string, CultureInfo> _cultures;

    public ParserHelperService(ILogger<ParserHelperService> logger)
    {
        _logger = logger;
        _cultures = new Dictionary<string, CultureInfo>
        {
            ["de"] = new CultureInfo("de-DE"),
            ["en"] = new CultureInfo("en-US"),
            ["fr"] = new CultureInfo("fr-FR"),
            ["es"] = new CultureInfo("es-ES"),
            ["it"] = new CultureInfo("it-IT")
        };
    }

    public async Task<DateParseResult> ParseDateAsync(string dateString)
    {
        try
        {
            _logger.LogInformation("Parsing date: {DateString}", dateString);

            var warnings = new List<DateParseWarning>();
            var errors = new List<DateParseError>();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                errors.Add(new DateParseError("EMPTY_DATE", "Date string is empty", "DateString", dateString, null));
                return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
            }

            // Try different date formats
            var formats = new[]
            {
                "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy",
                "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "yyyy-M-d",
                "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yy", "d.M.yy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    var date = DateOnly.FromDateTime(dateTime);
                    _logger.LogInformation("Date parsed successfully: {DateString} -> {Date}", dateString, date);
                    return new DateParseResult(true, "Date parsed successfully", date, dateString, format, "invariant", warnings, errors);
                }
            }

            // Try culture-specific parsing
            foreach (var culture in _cultures.Values)
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var dateTime))
                {
                    var date = DateOnly.FromDateTime(dateTime);
                    _logger.LogInformation("Date parsed successfully with culture: {DateString} -> {Date}", dateString, date);
                    return new DateParseResult(true, "Date parsed successfully", date, dateString, "culture-specific", culture.Name, warnings, errors);
                }
            }

            errors.Add(new DateParseError("PARSE_FAILED", "Unable to parse date", "DateString", dateString, null));
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse date: {DateString}", dateString);
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", new List<DateParseWarning>(), new List<DateParseError> { new DateParseError("PARSE_FAILED", ex.Message, "DateString", dateString, ex) });
        }
    }

    public async Task<DateParseResult> ParseDateAsync(string dateString, string format)
    {
        try
        {
            _logger.LogInformation("Parsing date with format: {DateString}, {Format}", dateString, format);

            var warnings = new List<DateParseWarning>();
            var errors = new List<DateParseError>();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                errors.Add(new DateParseError("EMPTY_DATE", "Date string is empty", "DateString", dateString, null));
                return new DateParseResult(false, "Date parsing failed", null, dateString, format, "", warnings, errors);
            }

            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                var date = DateOnly.FromDateTime(dateTime);
                _logger.LogInformation("Date parsed successfully: {DateString} -> {Date}", dateString, date);
                return new DateParseResult(true, "Date parsed successfully", date, dateString, format, "invariant", warnings, errors);
            }

            errors.Add(new DateParseError("PARSE_FAILED", "Unable to parse date with specified format", "DateString", dateString, null));
            return new DateParseResult(false, "Date parsing failed", null, dateString, format, "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse date with format: {DateString}, {Format}", dateString, format);
            return new DateParseResult(false, "Date parsing failed", null, dateString, format, "", new List<DateParseWarning>(), new List<DateParseError> { new DateParseError("PARSE_FAILED", ex.Message, "DateString", dateString, ex) });
        }
    }

    public async Task<DateParseResult> ParseDateAsync(string dateString, List<string> formats)
    {
        try
        {
            _logger.LogInformation("Parsing date with formats: {DateString}, {Formats}", dateString, string.Join(", ", formats));

            var warnings = new List<DateParseWarning>();
            var errors = new List<DateParseError>();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                errors.Add(new DateParseError("EMPTY_DATE", "Date string is empty", "DateString", dateString, null));
                return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
            }

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    var date = DateOnly.FromDateTime(dateTime);
                    _logger.LogInformation("Date parsed successfully: {DateString} -> {Date}", dateString, date);
                    return new DateParseResult(true, "Date parsed successfully", date, dateString, format, "invariant", warnings, errors);
                }
            }

            errors.Add(new DateParseError("PARSE_FAILED", "Unable to parse date with any specified format", "DateString", dateString, null));
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse date with formats: {DateString}, {Formats}", dateString, string.Join(", ", formats));
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", new List<DateParseWarning>(), new List<DateParseError> { new DateParseError("PARSE_FAILED", ex.Message, "DateString", dateString, ex) });
        }
    }

    public async Task<DateParseResult> ParseDateAsync(string dateString, DateParseOptions options)
    {
        try
        {
            _logger.LogInformation("Parsing date with options: {DateString}", dateString);

            var warnings = new List<DateParseWarning>();
            var errors = new List<DateParseError>();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                errors.Add(new DateParseError("EMPTY_DATE", "Date string is empty", "DateString", dateString, null));
                return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
            }

            // Try specified formats first
            if (options.Formats.Any())
            {
                var result = await ParseDateAsync(dateString, options.Formats);
                if (result.Success)
                {
                    return result;
                }
            }

            // Try culture-specific parsing
            if (!string.IsNullOrWhiteSpace(options.Culture) && _cultures.TryGetValue(options.Culture, out var culture))
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var dateTime))
                {
                    var date = DateOnly.FromDateTime(dateTime);

                    // Apply validation rules
                    if (options.MinDate.HasValue && date < options.MinDate.Value)
                    {
                        warnings.Add(new DateParseWarning("DATE_TOO_EARLY", "Date is earlier than minimum allowed", "Date", date, "Consider using a later date"));
                    }

                    if (options.MaxDate.HasValue && date > options.MaxDate.Value)
                    {
                        warnings.Add(new DateParseWarning("DATE_TOO_LATE", "Date is later than maximum allowed", "Date", date, "Consider using an earlier date"));
                    }

                    _logger.LogInformation("Date parsed successfully with culture: {DateString} -> {Date}", dateString, date);
                    return new DateParseResult(true, "Date parsed successfully", date, dateString, "culture-specific", culture.Name, warnings, errors);
                }
            }

            errors.Add(new DateParseError("PARSE_FAILED", "Unable to parse date with specified options", "DateString", dateString, null));
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse date with options: {DateString}", dateString);
            return new DateParseResult(false, "Date parsing failed", null, dateString, "", "", new List<DateParseWarning>(), new List<DateParseError> { new DateParseError("PARSE_FAILED", ex.Message, "DateString", dateString, ex) });
        }
    }

    public async Task<List<DateParseResult>> ParseDatesAsync(List<string> dateStrings)
    {
        try
        {
            var results = new List<DateParseResult>();

            foreach (var dateString in dateStrings)
            {
                var result = await ParseDateAsync(dateString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse dates");
            return new List<DateParseResult>();
        }
    }

    public async Task<List<DateParseResult>> ParseDatesAsync(List<string> dateStrings, DateParseOptions options)
    {
        try
        {
            var results = new List<DateParseResult>();

            foreach (var dateString in dateStrings)
            {
                var result = await ParseDateAsync(dateString, options);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse dates with options");
            return new List<DateParseResult>();
        }
    }

    public async Task<DateValidationResult> ValidateDateAsync(string dateString)
    {
        try
        {
            _logger.LogInformation("Validating date: {DateString}", dateString);

            var warnings = new List<DateValidationWarning>();
            var errors = new List<DateValidationError>();

            var parseResult = await ParseDateAsync(dateString);
            if (!parseResult.Success)
            {
                errors.Add(new DateValidationError("PARSE_FAILED", "Date parsing failed", "DateString", dateString, null));
                return new DateValidationResult(false, "Date validation failed", DateOnly.MinValue, dateString, warnings, errors);
            }

            var date = parseResult.Date.Value;
            var now = DateOnly.FromDateTime(DateTime.Today);

            // Check if date is in the future
            if (date > now)
            {
                warnings.Add(new DateValidationWarning("FUTURE_DATE", "Date is in the future", "Date", date, "Consider using a past date"));
            }

            // Check if date is too far in the past
            if (date < now.AddYears(-10))
            {
                warnings.Add(new DateValidationWarning("OLD_DATE", "Date is very old", "Date", date, "Consider using a more recent date"));
            }

            _logger.LogInformation("Date validation completed: {DateString} -> {IsValid}", dateString, true);
            return new DateValidationResult(true, "Date validation successful", date, dateString, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate date: {DateString}", dateString);
            return new DateValidationResult(false, "Date validation failed", DateOnly.MinValue, dateString, new List<DateValidationWarning>(), new List<DateValidationError> { new DateValidationError("VALIDATION_FAILED", ex.Message, "DateString", dateString, ex) });
        }
    }

    public async Task<DateValidationResult> ValidateDateAsync(DateOnly date)
    {
        try
        {
            var warnings = new List<DateValidationWarning>();
            var errors = new List<DateValidationError>();

            var now = DateOnly.FromDateTime(DateTime.Today);

            // Check if date is in the future
            if (date > now)
            {
                warnings.Add(new DateValidationWarning("FUTURE_DATE", "Date is in the future", "Date", date, "Consider using a past date"));
            }

            // Check if date is too far in the past
            if (date < now.AddYears(-10))
            {
                warnings.Add(new DateValidationWarning("OLD_DATE", "Date is very old", "Date", date, "Consider using a more recent date"));
            }

            return new DateValidationResult(true, "Date validation successful", date, date.ToString(), warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate date: {Date}", date);
            return new DateValidationResult(false, "Date validation failed", date, date.ToString(), new List<DateValidationWarning>(), new List<DateValidationError> { new DateValidationError("VALIDATION_FAILED", ex.Message, "Date", date, ex) });
        }
    }

    public async Task<DateValidationResult> ValidateDateAsync(DateOnly date, DateValidationOptions options)
    {
        try
        {
            var warnings = new List<DateValidationWarning>();
            var errors = new List<DateValidationError>();

            // Check minimum date
            if (options.MinDate.HasValue && date < options.MinDate.Value)
            {
                errors.Add(new DateValidationError("DATE_TOO_EARLY", "Date is earlier than minimum allowed", "Date", date, null));
            }

            // Check maximum date
            if (options.MaxDate.HasValue && date > options.MaxDate.Value)
            {
                errors.Add(new DateValidationError("DATE_TOO_LATE", "Date is later than maximum allowed", "Date", date, null));
            }

            // Check future dates
            if (!options.AllowFutureDates)
            {
                var now = DateOnly.FromDateTime(DateTime.Today);
                if (date > now)
                {
                    errors.Add(new DateValidationError("FUTURE_DATE_NOT_ALLOWED", "Future dates are not allowed", "Date", date, null));
                }
            }

            // Check past dates
            if (!options.AllowPastDates)
            {
                var now = DateOnly.FromDateTime(DateTime.Today);
                if (date < now)
                {
                    errors.Add(new DateValidationError("PAST_DATE_NOT_ALLOWED", "Past dates are not allowed", "Date", date, null));
                }
            }

            // Check maximum days in future
            if (options.MaxDaysInFuture > 0)
            {
                var now = DateOnly.FromDateTime(DateTime.Today);
                var maxFutureDate = now.AddDays(options.MaxDaysInFuture);
                if (date > maxFutureDate)
                {
                    errors.Add(new DateValidationError("DATE_TOO_FAR_FUTURE", "Date is too far in the future", "Date", date, null));
                }
            }

            // Check maximum days in past
            if (options.MaxDaysInPast > 0)
            {
                var now = DateOnly.FromDateTime(DateTime.Today);
                var maxPastDate = now.AddDays(-options.MaxDaysInPast);
                if (date < maxPastDate)
                {
                    errors.Add(new DateValidationError("DATE_TOO_FAR_PAST", "Date is too far in the past", "Date", date, null));
                }
            }

            return new DateValidationResult(errors.Count == 0, "Date validation completed", date, date.ToString(), warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate date with options: {Date}", date);
            return new DateValidationResult(false, "Date validation failed", date, date.ToString(), new List<DateValidationWarning>(), new List<DateValidationError> { new DateValidationError("VALIDATION_FAILED", ex.Message, "Date", date, ex) });
        }
    }

    public async Task<List<DateValidationResult>> ValidateDatesAsync(List<string> dateStrings)
    {
        try
        {
            var results = new List<DateValidationResult>();

            foreach (var dateString in dateStrings)
            {
                var result = await ValidateDateAsync(dateString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate dates");
            return new List<DateValidationResult>();
        }
    }

    public async Task<List<DateValidationResult>> ValidateDatesAsync(List<DateOnly> dates)
    {
        try
        {
            var results = new List<DateValidationResult>();

            foreach (var date in dates)
            {
                var result = await ValidateDateAsync(date);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate dates");
            return new List<DateValidationResult>();
        }
    }

    public async Task<string> FormatDateAsync(DateOnly date, string format)
    {
        try
        {
            return date.ToString(format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format date: {Date}, {Format}", date, format);
            return date.ToString();
        }
    }

    public async Task<string> FormatDateAsync(DateOnly date, DateFormatOptions options)
    {
        try
        {
            var format = options.Format;
            if (string.IsNullOrWhiteSpace(format))
            {
                format = options.UseShortFormat ? "d" : "D";
            }

            return date.ToString(format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format date with options: {Date}", date);
            return date.ToString();
        }
    }

    public async Task<List<string>> FormatDatesAsync(List<DateOnly> dates, string format)
    {
        try
        {
            var formattedDates = new List<string>();

            foreach (var date in dates)
            {
                var formatted = await FormatDateAsync(date, format);
                formattedDates.Add(formatted);
            }

            return formattedDates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format dates");
            return new List<string>();
        }
    }

    public async Task<List<string>> FormatDatesAsync(List<DateOnly> dates, DateFormatOptions options)
    {
        try
        {
            var formattedDates = new List<string>();

            foreach (var date in dates)
            {
                var formatted = await FormatDateAsync(date, options);
                formattedDates.Add(formatted);
            }

            return formattedDates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format dates with options");
            return new List<string>();
        }
    }

    public async Task<DecimalParseResult> ParseDecimalAsync(string decimalString)
    {
        try
        {
            _logger.LogInformation("Parsing decimal: {DecimalString}", decimalString);

            var warnings = new List<DecimalParseWarning>();
            var errors = new List<DecimalParseError>();

            if (string.IsNullOrWhiteSpace(decimalString))
            {
                errors.Add(new DecimalParseError("EMPTY_DECIMAL", "Decimal string is empty", "DecimalString", decimalString, null));
                return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, "", warnings, errors);
            }

            // Clean the string
            var cleanedString = decimalString.Trim();

            // Remove currency symbols
            cleanedString = Regex.Replace(cleanedString, @"[â‚¬$Â£Â¥]", "");

            // Try different decimal separators
            var separators = new[] { ".", "," };
            foreach (var separator in separators)
            {
                if (decimal.TryParse(cleanedString, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                {
                    _logger.LogInformation("Decimal parsed successfully: {DecimalString} -> {Value}", decimalString, value);
                    return new DecimalParseResult(true, "Decimal parsed successfully", value, decimalString, "invariant", warnings, errors);
                }
            }

            // Try culture-specific parsing
            foreach (var culture in _cultures.Values)
            {
                if (decimal.TryParse(cleanedString, NumberStyles.Number, culture, out var value))
                {
                    _logger.LogInformation("Decimal parsed successfully with culture: {DecimalString} -> {Value}", decimalString, value);
                    return new DecimalParseResult(true, "Decimal parsed successfully", value, decimalString, culture.Name, warnings, errors);
                }
            }

            errors.Add(new DecimalParseError("PARSE_FAILED", "Unable to parse decimal", "DecimalString", decimalString, null));
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decimal: {DecimalString}", decimalString);
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, "", new List<DecimalParseWarning>(), new List<DecimalParseError> { new DecimalParseError("PARSE_FAILED", ex.Message, "DecimalString", decimalString, ex) });
        }
    }

    public async Task<DecimalParseResult> ParseDecimalAsync(string decimalString, string culture)
    {
        try
        {
            _logger.LogInformation("Parsing decimal with culture: {DecimalString}, {Culture}", decimalString, culture);

            var warnings = new List<DecimalParseWarning>();
            var errors = new List<DecimalParseError>();

            if (string.IsNullOrWhiteSpace(decimalString))
            {
                errors.Add(new DecimalParseError("EMPTY_DECIMAL", "Decimal string is empty", "DecimalString", decimalString, null));
                return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, culture, warnings, errors);
            }

            if (_cultures.TryGetValue(culture, out var cultureInfo))
            {
                if (decimal.TryParse(decimalString, NumberStyles.Number, cultureInfo, out var value))
                {
                    _logger.LogInformation("Decimal parsed successfully: {DecimalString} -> {Value}", decimalString, value);
                    return new DecimalParseResult(true, "Decimal parsed successfully", value, decimalString, culture, warnings, errors);
                }
            }

            errors.Add(new DecimalParseError("PARSE_FAILED", "Unable to parse decimal with specified culture", "DecimalString", decimalString, null));
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, culture, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decimal with culture: {DecimalString}, {Culture}", decimalString, culture);
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, culture, new List<DecimalParseWarning>(), new List<DecimalParseError> { new DecimalParseError("PARSE_FAILED", ex.Message, "DecimalString", decimalString, ex) });
        }
    }

    public async Task<DecimalParseResult> ParseDecimalAsync(string decimalString, DecimalParseOptions options)
    {
        try
        {
            _logger.LogInformation("Parsing decimal with options: {DecimalString}", decimalString);

            var warnings = new List<DecimalParseWarning>();
            var errors = new List<DecimalParseError>();

            if (string.IsNullOrWhiteSpace(decimalString))
            {
                errors.Add(new DecimalParseError("EMPTY_DECIMAL", "Decimal string is empty", "DecimalString", decimalString, null));
                return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, options.Culture, warnings, errors);
            }

            // Parse with specified culture
            var culture = !string.IsNullOrWhiteSpace(options.Culture) && _cultures.TryGetValue(options.Culture, out var cultureInfo)
                ? cultureInfo
                : CultureInfo.InvariantCulture;

            if (decimal.TryParse(decimalString, NumberStyles.Number, culture, out var value))
            {
                // Apply validation rules
                if (!options.AllowNegative && value < 0)
                {
                    errors.Add(new DecimalParseError("NEGATIVE_NOT_ALLOWED", "Negative values are not allowed", "Value", value, null));
                    return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, options.Culture, warnings, errors);
                }

                if (!options.AllowZero && value == 0)
                {
                    errors.Add(new DecimalParseError("ZERO_NOT_ALLOWED", "Zero values are not allowed", "Value", value, null));
                    return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, options.Culture, warnings, errors);
                }

                if (options.MinValue.HasValue && value < options.MinValue.Value)
                {
                    warnings.Add(new DecimalParseWarning("VALUE_TOO_SMALL", "Value is smaller than minimum allowed", "Value", value, "Consider using a larger value"));
                }

                if (options.MaxValue.HasValue && value > options.MaxValue.Value)
                {
                    warnings.Add(new DecimalParseWarning("VALUE_TOO_LARGE", "Value is larger than maximum allowed", "Value", value, "Consider using a smaller value"));
                }

                if (options.MaxDecimalPlaces > 0)
                {
                    var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
                    if (decimalPlaces > options.MaxDecimalPlaces)
                    {
                        warnings.Add(new DecimalParseWarning("TOO_MANY_DECIMAL_PLACES", "Value has too many decimal places", "Value", value, "Consider rounding the value"));
                    }
                }

                _logger.LogInformation("Decimal parsed successfully: {DecimalString} -> {Value}", decimalString, value);
                return new DecimalParseResult(true, "Decimal parsed successfully", value, decimalString, options.Culture, warnings, errors);
            }

            errors.Add(new DecimalParseError("PARSE_FAILED", "Unable to parse decimal with specified options", "DecimalString", decimalString, null));
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, options.Culture, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decimal with options: {DecimalString}", decimalString);
            return new DecimalParseResult(false, "Decimal parsing failed", null, decimalString, options.Culture, new List<DecimalParseWarning>(), new List<DecimalParseError> { new DecimalParseError("PARSE_FAILED", ex.Message, "DecimalString", decimalString, ex) });
        }
    }

    public async Task<List<DecimalParseResult>> ParseDecimalsAsync(List<string> decimalStrings)
    {
        try
        {
            var results = new List<DecimalParseResult>();

            foreach (var decimalString in decimalStrings)
            {
                var result = await ParseDecimalAsync(decimalString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decimals");
            return new List<DecimalParseResult>();
        }
    }

    public async Task<List<DecimalParseResult>> ParseDecimalsAsync(List<string> decimalStrings, DecimalParseOptions options)
    {
        try
        {
            var results = new List<DecimalParseResult>();

            foreach (var decimalString in decimalStrings)
            {
                var result = await ParseDecimalAsync(decimalString, options);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decimals with options");
            return new List<DecimalParseResult>();
        }
    }

    public async Task<DecimalValidationResult> ValidateDecimalAsync(string decimalString)
    {
        try
        {
            _logger.LogInformation("Validating decimal: {DecimalString}", decimalString);

            var warnings = new List<DecimalValidationWarning>();
            var errors = new List<DecimalValidationError>();

            var parseResult = await ParseDecimalAsync(decimalString);
            if (!parseResult.Success)
            {
                errors.Add(new DecimalValidationError("PARSE_FAILED", "Decimal parsing failed", "DecimalString", decimalString, null));
                return new DecimalValidationResult(false, "Decimal validation failed", 0m, decimalString, warnings, errors);
            }

            var value = parseResult.Value.Value;
            var now = DateTime.Today;

            // Check if value is negative
            if (value < 0)
            {
                warnings.Add(new DecimalValidationWarning("NEGATIVE_VALUE", "Value is negative", "Value", value, "Consider using a positive value"));
            }

            // Check if value is zero
            if (value == 0)
            {
                warnings.Add(new DecimalValidationWarning("ZERO_VALUE", "Value is zero", "Value", value, "Consider using a non-zero value"));
            }

            // Check if value is very large
            if (value > 1000000m)
            {
                warnings.Add(new DecimalValidationWarning("LARGE_VALUE", "Value is very large", "Value", value, "Consider using a smaller value"));
            }

            _logger.LogInformation("Decimal validation completed: {DecimalString} -> {IsValid}", decimalString, true);
            return new DecimalValidationResult(true, "Decimal validation successful", value, decimalString, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decimal: {DecimalString}", decimalString);
            return new DecimalValidationResult(false, "Decimal validation failed", 0m, decimalString, new List<DecimalValidationWarning>(), new List<DecimalValidationError> { new DecimalValidationError("VALIDATION_FAILED", ex.Message, "DecimalString", decimalString, ex) });
        }
    }

    public async Task<DecimalValidationResult> ValidateDecimalAsync(decimal value)
    {
        try
        {
            var warnings = new List<DecimalValidationWarning>();
            var errors = new List<DecimalValidationError>();

            // Check if value is negative
            if (value < 0)
            {
                warnings.Add(new DecimalValidationWarning("NEGATIVE_VALUE", "Value is negative", "Value", value, "Consider using a positive value"));
            }

            // Check if value is zero
            if (value == 0)
            {
                warnings.Add(new DecimalValidationWarning("ZERO_VALUE", "Value is zero", "Value", value, "Consider using a non-zero value"));
            }

            // Check if value is very large
            if (value > 1000000m)
            {
                warnings.Add(new DecimalValidationWarning("LARGE_VALUE", "Value is very large", "Value", value, "Consider using a smaller value"));
            }

            return new DecimalValidationResult(true, "Decimal validation successful", value, value.ToString(), warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decimal: {Value}", value);
            return new DecimalValidationResult(false, "Decimal validation failed", value, value.ToString(), new List<DecimalValidationWarning>(), new List<DecimalValidationError> { new DecimalValidationError("VALIDATION_FAILED", ex.Message, "Value", value, ex) });
        }
    }

    public async Task<DecimalValidationResult> ValidateDecimalAsync(decimal value, DecimalValidationOptions options)
    {
        try
        {
            var warnings = new List<DecimalValidationWarning>();
            var errors = new List<DecimalValidationError>();

            // Check minimum value
            if (options.MinValue.HasValue && value < options.MinValue.Value)
            {
                errors.Add(new DecimalValidationError("VALUE_TOO_SMALL", "Value is smaller than minimum allowed", "Value", value, null));
            }

            // Check maximum value
            if (options.MaxValue.HasValue && value > options.MaxValue.Value)
            {
                errors.Add(new DecimalValidationError("VALUE_TOO_LARGE", "Value is larger than maximum allowed", "Value", value, null));
            }

            // Check negative values
            if (!options.AllowNegative && value < 0)
            {
                errors.Add(new DecimalValidationError("NEGATIVE_NOT_ALLOWED", "Negative values are not allowed", "Value", value, null));
            }

            // Check zero values
            if (!options.AllowZero && value == 0)
            {
                errors.Add(new DecimalValidationError("ZERO_NOT_ALLOWED", "Zero values are not allowed", "Value", value, null));
            }

            // Check decimal places
            if (options.MaxDecimalPlaces > 0)
            {
                var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
                if (decimalPlaces > options.MaxDecimalPlaces)
                {
                    errors.Add(new DecimalValidationError("TOO_MANY_DECIMAL_PLACES", "Value has too many decimal places", "Value", value, null));
                }
            }

            return new DecimalValidationResult(errors.Count == 0, "Decimal validation completed", value, value.ToString(), warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decimal with options: {Value}", value);
            return new DecimalValidationResult(false, "Decimal validation failed", value, value.ToString(), new List<DecimalValidationWarning>(), new List<DecimalValidationError> { new DecimalValidationError("VALIDATION_FAILED", ex.Message, "Value", value, ex) });
        }
    }

    public async Task<List<DecimalValidationResult>> ValidateDecimalsAsync(List<string> decimalStrings)
    {
        try
        {
            var results = new List<DecimalValidationResult>();

            foreach (var decimalString in decimalStrings)
            {
                var result = await ValidateDecimalAsync(decimalString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decimals");
            return new List<DecimalValidationResult>();
        }
    }

    public async Task<List<DecimalValidationResult>> ValidateDecimalsAsync(List<decimal> values)
    {
        try
        {
            var results = new List<DecimalValidationResult>();

            foreach (var value in values)
            {
                var result = await ValidateDecimalAsync(value);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decimals");
            return new List<DecimalValidationResult>();
        }
    }

    public async Task<string> FormatDecimalAsync(decimal value, string format)
    {
        try
        {
            return value.ToString(format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format decimal: {Value}, {Format}", value, format);
            return value.ToString();
        }
    }

    public async Task<string> FormatDecimalAsync(decimal value, DecimalFormatOptions options)
    {
        try
        {
            var format = options.Format;
            if (string.IsNullOrWhiteSpace(format))
            {
                format = options.DecimalPlaces > 0 ? $"F{options.DecimalPlaces}" : "F2";
            }

            var formatted = value.ToString(format);

            if (options.IncludeCurrency && !string.IsNullOrWhiteSpace(options.CurrencySymbol))
            {
                formatted = $"{options.CurrencySymbol} {formatted}";
            }

            return formatted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format decimal with options: {Value}", value);
            return value.ToString();
        }
    }

    public async Task<List<string>> FormatDecimalsAsync(List<decimal> values, string format)
    {
        try
        {
            var formattedValues = new List<string>();

            foreach (var value in values)
            {
                var formatted = await FormatDecimalAsync(value, format);
                formattedValues.Add(formatted);
            }

            return formattedValues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format decimals");
            return new List<string>();
        }
    }

    public async Task<List<string>> FormatDecimalsAsync(List<decimal> values, DecimalFormatOptions options)
    {
        try
        {
            var formattedValues = new List<string>();

            foreach (var value in values)
            {
                var formatted = await FormatDecimalAsync(value, options);
                formattedValues.Add(formatted);
            }

            return formattedValues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format decimals with options");
            return new List<string>();
        }
    }

    public async Task<AddressParseResult> ParseAddressAsync(string addressString)
    {
        try
        {
            _logger.LogInformation("Parsing address: {AddressString}", addressString);

            var warnings = new List<AddressParseWarning>();
            var errors = new List<AddressParseError>();

            if (string.IsNullOrWhiteSpace(addressString))
            {
                errors.Add(new AddressParseError("EMPTY_ADDRESS", "Address string is empty", "AddressString", addressString, null));
                return new AddressParseResult(false, "Address parsing failed", null, addressString, "", warnings, errors);
            }

            // Simple address parsing - this would be more sophisticated in a real implementation
            var parts = addressString.Split(',');
            if (parts.Length < 2)
            {
                errors.Add(new AddressParseError("INVALID_ADDRESS_FORMAT", "Address format is invalid", "AddressString", addressString, null));
                return new AddressParseResult(false, "Address parsing failed", null, addressString, "", warnings, errors);
            }

            var street = parts[0].Trim();
            var cityPart = parts[1].Trim();

            // Extract postal code and city
            var postalCodeMatch = Regex.Match(cityPart, @"(\d{5})\s*(.+)");
            if (!postalCodeMatch.Success)
            {
                warnings.Add(new AddressParseWarning("NO_POSTAL_CODE", "No postal code found", "AddressString", addressString, "Consider adding a postal code"));
            }

            var postalCode = postalCodeMatch.Success ? postalCodeMatch.Groups[1].Value : "";
            var city = postalCodeMatch.Success ? postalCodeMatch.Groups[2].Value : cityPart;

            var address = new AddressBlock(
                street,
                "", // House number - would need more sophisticated parsing
                postalCode,
                city,
                "", // State
                "", // Country
                addressString,
                new Dictionary<string, string>()
            );

            _logger.LogInformation("Address parsed successfully: {AddressString} -> {Street}, {City}", addressString, street, city);
            return new AddressParseResult(true, "Address parsed successfully", address, addressString, "", warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse address: {AddressString}", addressString);
            return new AddressParseResult(false, "Address parsing failed", null, addressString, "", new List<AddressParseWarning>(), new List<AddressParseError> { new AddressParseError("PARSE_FAILED", ex.Message, "AddressString", addressString, ex) });
        }
    }

    public async Task<AddressParseResult> ParseAddressAsync(string addressString, string country)
    {
        try
        {
            _logger.LogInformation("Parsing address with country: {AddressString}, {Country}", addressString, country);

            var result = await ParseAddressAsync(addressString);
            if (result.Success && result.Address != null)
            {
                var updatedAddress = result.Address with { Country = country };
                return result with { Address = updatedAddress };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse address with country: {AddressString}, {Country}", addressString, country);
            return new AddressParseResult(false, "Address parsing failed", null, addressString, country, new List<AddressParseWarning>(), new List<AddressParseError> { new AddressParseError("PARSE_FAILED", ex.Message, "AddressString", addressString, ex) });
        }
    }

    public async Task<AddressParseResult> ParseAddressAsync(string addressString, AddressParseOptions options)
    {
        try
        {
            _logger.LogInformation("Parsing address with options: {AddressString}", addressString);

            var result = await ParseAddressAsync(addressString, options.Country);
            if (result.Success && result.Address != null)
            {
                // Apply additional validation based on options
                if (options.StrictMode)
                {
                    if (string.IsNullOrWhiteSpace(result.Address.Street))
                    {
                        result.Errors.Add(new AddressParseError("MISSING_STREET", "Street is required in strict mode", "Street", result.Address.Street, null));
                    }

                    if (string.IsNullOrWhiteSpace(result.Address.PostalCode))
                    {
                        result.Errors.Add(new AddressParseError("MISSING_POSTAL_CODE", "Postal code is required in strict mode", "PostalCode", result.Address.PostalCode, null));
                    }

                    if (string.IsNullOrWhiteSpace(result.Address.City))
                    {
                        result.Errors.Add(new AddressParseError("MISSING_CITY", "City is required in strict mode", "City", result.Address.City, null));
                    }
                }

                if (options.RequiredFields.Any())
                {
                    foreach (var field in options.RequiredFields)
                    {
                        var value = field switch
                        {
                            "Street" => result.Address.Street,
                            "PostalCode" => result.Address.PostalCode,
                            "City" => result.Address.City,
                            "Country" => result.Address.Country,
                            _ => ""
                        };

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            result.Errors.Add(new AddressParseError("MISSING_REQUIRED_FIELD", $"Required field {field} is missing", field, value, null));
                        }
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse address with options: {AddressString}", addressString);
            return new AddressParseResult(false, "Address parsing failed", null, addressString, options.Country, new List<AddressParseWarning>(), new List<AddressParseError> { new AddressParseError("PARSE_FAILED", ex.Message, "AddressString", addressString, ex) });
        }
    }

    public async Task<List<AddressParseResult>> ParseAddressesAsync(List<string> addressStrings)
    {
        try
        {
            var results = new List<AddressParseResult>();

            foreach (var addressString in addressStrings)
            {
                var result = await ParseAddressAsync(addressString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse addresses");
            return new List<AddressParseResult>();
        }
    }

    public async Task<List<AddressParseResult>> ParseAddressesAsync(List<string> addressStrings, AddressParseOptions options)
    {
        try
        {
            var results = new List<AddressParseResult>();

            foreach (var addressString in addressStrings)
            {
                var result = await ParseAddressAsync(addressString, options);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse addresses with options");
            return new List<AddressParseResult>();
        }
    }

    public async Task<AddressValidationResult> ValidateAddressAsync(string addressString)
    {
        try
        {
            _logger.LogInformation("Validating address: {AddressString}", addressString);

            var warnings = new List<AddressValidationWarning>();
            var errors = new List<AddressValidationError>();

            var parseResult = await ParseAddressAsync(addressString);
            if (!parseResult.Success || parseResult.Address == null)
            {
                errors.Add(new AddressValidationError("PARSE_FAILED", "Address parsing failed", "AddressString", addressString, null));
                return new AddressValidationResult(false, "Address validation failed", new AddressBlock("", "", "", "", "", "", "", new Dictionary<string, string>()), addressString, warnings, errors);
            }

            var address = parseResult.Address;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(address.Street))
            {
                errors.Add(new AddressValidationError("MISSING_STREET", "Street is required", "Street", address.Street, null));
            }

            if (string.IsNullOrWhiteSpace(address.City))
            {
                errors.Add(new AddressValidationError("MISSING_CITY", "City is required", "City", address.City, null));
            }

            // Validate postal code format
            if (!string.IsNullOrWhiteSpace(address.PostalCode))
            {
                if (!Regex.IsMatch(address.PostalCode, @"^\d{5}$"))
                {
                    warnings.Add(new AddressValidationWarning("INVALID_POSTAL_CODE", "Postal code format is invalid", "PostalCode", address.PostalCode, "Consider using a 5-digit postal code"));
                }
            }

            _logger.LogInformation("Address validation completed: {AddressString} -> {IsValid}", addressString, errors.Count == 0);
            return new AddressValidationResult(errors.Count == 0, "Address validation completed", address, addressString, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate address: {AddressString}", addressString);
            return new AddressValidationResult(false, "Address validation failed", new AddressBlock("", "", "", "", "", "", "", new Dictionary<string, string>()), addressString, new List<AddressValidationWarning>(), new List<AddressValidationError> { new AddressValidationError("VALIDATION_FAILED", ex.Message, "AddressString", addressString, ex) });
        }
    }

    public async Task<AddressValidationResult> ValidateAddressAsync(AddressBlock address)
    {
        try
        {
            var warnings = new List<AddressValidationWarning>();
            var errors = new List<AddressValidationError>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(address.Street))
            {
                errors.Add(new AddressValidationError("MISSING_STREET", "Street is required", "Street", address.Street, null));
            }

            if (string.IsNullOrWhiteSpace(address.City))
            {
                errors.Add(new AddressValidationError("MISSING_CITY", "City is required", "City", address.City, null));
            }

            // Validate postal code format
            if (!string.IsNullOrWhiteSpace(address.PostalCode))
            {
                if (!Regex.IsMatch(address.PostalCode, @"^\d{5}$"))
                {
                    warnings.Add(new AddressValidationWarning("INVALID_POSTAL_CODE", "Postal code format is invalid", "PostalCode", address.PostalCode, "Consider using a 5-digit postal code"));
                }
            }

            return new AddressValidationResult(errors.Count == 0, "Address validation completed", address, address.FullAddress, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate address: {Address}", address.FullAddress);
            return new AddressValidationResult(false, "Address validation failed", address, address.FullAddress, new List<AddressValidationWarning>(), new List<AddressValidationError> { new AddressValidationError("VALIDATION_FAILED", ex.Message, "Address", address.FullAddress, ex) });
        }
    }

    public async Task<AddressValidationResult> ValidateAddressAsync(AddressBlock address, AddressValidationOptions options)
    {
        try
        {
            var warnings = new List<AddressValidationWarning>();
            var errors = new List<AddressValidationError>();

            // Validate required fields based on options
            if (options.RequireStreet && string.IsNullOrWhiteSpace(address.Street))
            {
                errors.Add(new AddressValidationError("MISSING_STREET", "Street is required", "Street", address.Street, null));
            }

            if (options.RequirePostalCode && string.IsNullOrWhiteSpace(address.PostalCode))
            {
                errors.Add(new AddressValidationError("MISSING_POSTAL_CODE", "Postal code is required", "PostalCode", address.PostalCode, null));
            }

            if (options.RequireCity && string.IsNullOrWhiteSpace(address.City))
            {
                errors.Add(new AddressValidationError("MISSING_CITY", "City is required", "City", address.City, null));
            }

            if (options.RequireCountry && string.IsNullOrWhiteSpace(address.Country))
            {
                errors.Add(new AddressValidationError("MISSING_COUNTRY", "Country is required", "Country", address.Country, null));
            }

            // Validate allowed countries
            if (options.AllowedCountries.Any() && !string.IsNullOrWhiteSpace(address.Country))
            {
                if (!options.AllowedCountries.Contains(address.Country))
                {
                    errors.Add(new AddressValidationError("COUNTRY_NOT_ALLOWED", "Country is not allowed", "Country", address.Country, null));
                }
            }

            return new AddressValidationResult(errors.Count == 0, "Address validation completed", address, address.FullAddress, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate address with options: {Address}", address.FullAddress);
            return new AddressValidationResult(false, "Address validation failed", address, address.FullAddress, new List<AddressValidationWarning>(), new List<AddressValidationError> { new AddressValidationError("VALIDATION_FAILED", ex.Message, "Address", address.FullAddress, ex) });
        }
    }

    public async Task<List<AddressValidationResult>> ValidateAddressesAsync(List<string> addressStrings)
    {
        try
        {
            var results = new List<AddressValidationResult>();

            foreach (var addressString in addressStrings)
            {
                var result = await ValidateAddressAsync(addressString);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate addresses");
            return new List<AddressValidationResult>();
        }
    }

    public async Task<List<AddressValidationResult>> ValidateAddressesAsync(List<AddressBlock> addresses)
    {
        try
        {
            var results = new List<AddressValidationResult>();

            foreach (var address in addresses)
            {
                var result = await ValidateAddressAsync(address);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate addresses");
            return new List<AddressValidationResult>();
        }
    }

    public async Task<string> FormatAddressAsync(AddressBlock address)
    {
        try
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(address.Street))
            {
                parts.Add(address.Street);
            }

            if (!string.IsNullOrWhiteSpace(address.HouseNumber))
            {
                parts.Add(address.HouseNumber);
            }

            if (!string.IsNullOrWhiteSpace(address.PostalCode) || !string.IsNullOrWhiteSpace(address.City))
            {
                var cityPart = string.Join(" ", new[] { address.PostalCode, address.City }.Where(s => !string.IsNullOrWhiteSpace(s)));
                parts.Add(cityPart);
            }

            if (!string.IsNullOrWhiteSpace(address.State))
            {
                parts.Add(address.State);
            }

            if (!string.IsNullOrWhiteSpace(address.Country))
            {
                parts.Add(address.Country);
            }

            return string.Join(", ", parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format address: {Address}", address.FullAddress);
            return address.FullAddress;
        }
    }

    public async Task<string> FormatAddressAsync(AddressBlock address, AddressFormatOptions options)
    {
        try
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(address.Street))
            {
                parts.Add(address.Street);
            }

            if (!string.IsNullOrWhiteSpace(address.HouseNumber))
            {
                parts.Add(address.HouseNumber);
            }

            if (!string.IsNullOrWhiteSpace(address.PostalCode) || !string.IsNullOrWhiteSpace(address.City))
            {
                var cityPart = string.Join(" ", new[] { address.PostalCode, address.City }.Where(s => !string.IsNullOrWhiteSpace(s)));
                parts.Add(cityPart);
            }

            if (!string.IsNullOrWhiteSpace(address.State))
            {
                parts.Add(address.State);
            }

            if (options.IncludeCountry && !string.IsNullOrWhiteSpace(address.Country))
            {
                parts.Add(address.Country);
            }

            return string.Join(", ", parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format address with options: {Address}", address.FullAddress);
            return address.FullAddress;
        }
    }

    public async Task<List<string>> FormatAddressesAsync(List<AddressBlock> addresses)
    {
        try
        {
            var formattedAddresses = new List<string>();

            foreach (var address in addresses)
            {
                var formatted = await FormatAddressAsync(address);
                formattedAddresses.Add(formatted);
            }

            return formattedAddresses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format addresses");
            return new List<string>();
        }
    }

    public async Task<List<string>> FormatAddressesAsync(List<AddressBlock> addresses, AddressFormatOptions options)
    {
        try
        {
            var formattedAddresses = new List<string>();

            foreach (var address in addresses)
            {
                var formatted = await FormatAddressAsync(address, options);
                formattedAddresses.Add(formatted);
            }

            return formattedAddresses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format addresses with options");
            return new List<string>();
        }
    }

    public async Task<string> NormalizeTextAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove extra whitespace
            text = Regex.Replace(text, @"\s+", " ");

            // Trim
            text = text.Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize text: {Text}", text);
            return text;
        }
    }

    public async Task<string> NormalizeTextAsync(string text, TextNormalizationOptions options)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove extra whitespace
            if (options.RemoveExtraSpaces)
            {
                text = Regex.Replace(text, @"\s+", " ");
            }

            // Normalize Unicode
            if (options.NormalizeUnicode)
            {
                text = text.Normalize(System.Text.NormalizationForm.FormC);
            }

            // Remove control characters
            if (options.RemoveControlCharacters)
            {
                text = Regex.Replace(text, @"[\p{C}]", "");
            }

            // Convert to lowercase
            if (options.ConvertToLowercase)
            {
                text = text.ToLowerInvariant();
            }

            // Remove accents
            if (options.RemoveAccents)
            {
                text = RemoveAccents(text);
            }

            // Trim
            text = text.Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize text with options: {Text}", text);
            return text;
        }
    }

    public async Task<List<string>> NormalizeTextsAsync(List<string> texts)
    {
        try
        {
            var normalizedTexts = new List<string>();

            foreach (var text in texts)
            {
                var normalized = await NormalizeTextAsync(text);
                normalizedTexts.Add(normalized);
            }

            return normalizedTexts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize texts");
            return new List<string>();
        }
    }

    public async Task<List<string>> NormalizeTextsAsync(List<string> texts, TextNormalizationOptions options)
    {
        try
        {
            var normalizedTexts = new List<string>();

            foreach (var text in texts)
            {
                var normalized = await NormalizeTextAsync(text, options);
                normalizedTexts.Add(normalized);
            }

            return normalizedTexts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize texts with options");
            return new List<string>();
        }
    }

    public async Task<string> CleanTextAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove HTML tags
            text = Regex.Replace(text, @"<[^>]+>", "");

            // Remove markdown
            text = Regex.Replace(text, @"[*_`#\[\]]", "");

            // Remove special characters
            text = Regex.Replace(text, @"[^\w\s]", "");

            // Remove extra whitespace
            text = Regex.Replace(text, @"\s+", " ");

            // Trim
            text = text.Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean text: {Text}", text);
            return text;
        }
    }

    public async Task<string> CleanTextAsync(string text, TextCleaningOptions options)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove HTML tags
            if (options.RemoveHtml)
            {
                text = Regex.Replace(text, @"<[^>]+>", "");
            }

            // Remove markdown
            if (options.RemoveMarkdown)
            {
                text = Regex.Replace(text, @"[*_`#\[\]]", "");
            }

            // Remove special characters
            if (options.RemoveSpecialCharacters)
            {
                text = Regex.Replace(text, @"[^\w\s]", "");
            }

            // Remove numbers
            if (options.RemoveNumbers)
            {
                text = Regex.Replace(text, @"\d", "");
            }

            // Remove punctuation
            if (options.RemovePunctuation)
            {
                text = Regex.Replace(text, @"[^\w\s]", "");
            }

            // Remove extra whitespace
            text = Regex.Replace(text, @"\s+", " ");

            // Trim
            text = text.Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean text with options: {Text}", text);
            return text;
        }
    }

    public async Task<List<string>> CleanTextsAsync(List<string> texts)
    {
        try
        {
            var cleanedTexts = new List<string>();

            foreach (var text in texts)
            {
                var cleaned = await CleanTextAsync(text);
                cleanedTexts.Add(cleaned);
            }

            return cleanedTexts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean texts");
            return new List<string>();
        }
    }

    public async Task<List<string>> CleanTextsAsync(List<string> texts, TextCleaningOptions options)
    {
        try
        {
            var cleanedTexts = new List<string>();

            foreach (var text in texts)
            {
                var cleaned = await CleanTextAsync(text, options);
                cleanedTexts.Add(cleaned);
            }

            return cleanedTexts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean texts with options");
            return new List<string>();
        }
    }

    public async Task<TextExtractionResult> ExtractTextAsync(string text, string pattern)
    {
        try
        {
            _logger.LogInformation("Extracting text with pattern: {Pattern}", pattern);

            var warnings = new List<TextExtractionWarning>();
            var errors = new List<TextExtractionError>();

            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add(new TextExtractionError("EMPTY_TEXT", "Text is empty", "Text", text, null));
                return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, warnings, errors);
            }

            if (string.IsNullOrWhiteSpace(pattern))
            {
                errors.Add(new TextExtractionError("EMPTY_PATTERN", "Pattern is empty", "Pattern", pattern, null));
                return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, warnings, errors);
            }

            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matches = regex.Matches(text);
                var extractedTexts = matches.Cast<Match>().Select(m => m.Value).ToList();

                _logger.LogInformation("Text extraction completed: {Count} matches", extractedTexts.Count);
                return new TextExtractionResult(true, "Text extraction successful", extractedTexts, text, warnings, errors);
            }
            catch (ArgumentException ex)
            {
                errors.Add(new TextExtractionError("INVALID_PATTERN", "Invalid regex pattern", "Pattern", pattern, ex));
                return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, warnings, errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text: {Text}, {Pattern}", text, pattern);
            return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, new List<TextExtractionWarning>(), new List<TextExtractionError> { new TextExtractionError("EXTRACTION_FAILED", ex.Message, "Text", text, ex) });
        }
    }

    public async Task<TextExtractionResult> ExtractTextAsync(string text, List<string> patterns)
    {
        try
        {
            _logger.LogInformation("Extracting text with patterns: {Patterns}", string.Join(", ", patterns));

            var warnings = new List<TextExtractionWarning>();
            var errors = new List<TextExtractionError>();
            var extractedTexts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add(new TextExtractionError("EMPTY_TEXT", "Text is empty", "Text", text, null));
                return new TextExtractionResult(false, "Text extraction failed", extractedTexts, text, warnings, errors);
            }

            foreach (var pattern in patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    warnings.Add(new TextExtractionWarning("EMPTY_PATTERN", "Pattern is empty", "Pattern", pattern, "Consider using a valid pattern"));
                    continue;
                }

                try
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var matches = regex.Matches(text);
                    var patternTexts = matches.Cast<Match>().Select(m => m.Value).ToList();
                    extractedTexts.AddRange(patternTexts);
                }
                catch (ArgumentException ex)
                {
                    errors.Add(new TextExtractionError("INVALID_PATTERN", "Invalid regex pattern", "Pattern", pattern, ex));
                }
            }

            _logger.LogInformation("Text extraction completed: {Count} matches", extractedTexts.Count);
            return new TextExtractionResult(true, "Text extraction successful", extractedTexts, text, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text with patterns: {Text}, {Patterns}", text, string.Join(", ", patterns));
            return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, new List<TextExtractionWarning>(), new List<TextExtractionError> { new TextExtractionError("EXTRACTION_FAILED", ex.Message, "Text", text, ex) });
        }
    }

    public async Task<TextExtractionResult> ExtractTextAsync(string text, TextExtractionOptions options)
    {
        try
        {
            _logger.LogInformation("Extracting text with options: {Text}", text);

            var warnings = new List<TextExtractionWarning>();
            var errors = new List<TextExtractionError>();
            var extractedTexts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add(new TextExtractionError("EMPTY_TEXT", "Text is empty", "Text", text, null));
                return new TextExtractionResult(false, "Text extraction failed", extractedTexts, text, warnings, errors);
            }

            foreach (var pattern in options.Patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    warnings.Add(new TextExtractionWarning("EMPTY_PATTERN", "Pattern is empty", "Pattern", pattern, "Consider using a valid pattern"));
                    continue;
                }

                try
                {
                    var regexOptions = RegexOptions.None;
                    if (!options.CaseSensitive)
                        regexOptions |= RegexOptions.IgnoreCase;
                    if (options.Multiline)
                        regexOptions |= RegexOptions.Multiline;

                    var regex = new Regex(pattern, regexOptions);
                    var matches = regex.Matches(text);
                    var patternTexts = matches.Cast<Match>().Select(m => m.Value).ToList();
                    extractedTexts.AddRange(patternTexts);
                }
                catch (ArgumentException ex)
                {
                    errors.Add(new TextExtractionError("INVALID_PATTERN", "Invalid regex pattern", "Pattern", pattern, ex));
                }
            }

            _logger.LogInformation("Text extraction completed: {Count} matches", extractedTexts.Count);
            return new TextExtractionResult(true, "Text extraction successful", extractedTexts, text, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text with options: {Text}", text);
            return new TextExtractionResult(false, "Text extraction failed", new List<string>(), text, new List<TextExtractionWarning>(), new List<TextExtractionError> { new TextExtractionError("EXTRACTION_FAILED", ex.Message, "Text", text, ex) });
        }
    }

    public async Task<List<TextExtractionResult>> ExtractTextsAsync(List<string> texts, string pattern)
    {
        try
        {
            var results = new List<TextExtractionResult>();

            foreach (var text in texts)
            {
                var result = await ExtractTextAsync(text, pattern);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract texts");
            return new List<TextExtractionResult>();
        }
    }

    public async Task<List<TextExtractionResult>> ExtractTextsAsync(List<string> texts, List<string> patterns)
    {
        try
        {
            var results = new List<TextExtractionResult>();

            foreach (var text in texts)
            {
                var result = await ExtractTextAsync(text, patterns);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract texts with patterns");
            return new List<TextExtractionResult>();
        }
    }

    public async Task<TextValidationResult> ValidateTextAsync(string text)
    {
        try
        {
            _logger.LogInformation("Validating text: {Text}", text);

            var warnings = new List<TextValidationWarning>();
            var errors = new List<TextValidationError>();

            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add(new TextValidationError("EMPTY_TEXT", "Text is empty", "Text", text, null));
                return new TextValidationResult(false, "Text validation failed", text, warnings, errors);
            }

            // Check text length
            if (text.Length < 3)
            {
                warnings.Add(new TextValidationWarning("SHORT_TEXT", "Text is very short", "Text", text, "Consider using longer text"));
            }

            if (text.Length > 1000)
            {
                warnings.Add(new TextValidationWarning("LONG_TEXT", "Text is very long", "Text", text, "Consider using shorter text"));
            }

            // Check for special characters
            if (text.Any(c => char.IsControl(c)))
            {
                warnings.Add(new TextValidationWarning("CONTROL_CHARACTERS", "Text contains control characters", "Text", text, "Consider removing control characters"));
            }

            _logger.LogInformation("Text validation completed: {Text} -> {IsValid}", text, errors.Count == 0);
            return new TextValidationResult(errors.Count == 0, "Text validation completed", text, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate text: {Text}", text);
            return new TextValidationResult(false, "Text validation failed", text, new List<TextValidationWarning>(), new List<TextValidationError> { new TextValidationError("VALIDATION_FAILED", ex.Message, "Text", text, ex) });
        }
    }

    public async Task<TextValidationResult> ValidateTextAsync(string text, TextValidationOptions options)
    {
        try
        {
            _logger.LogInformation("Validating text with options: {Text}", text);

            var warnings = new List<TextValidationWarning>();
            var errors = new List<TextValidationError>();

            // Check if text is empty
            if (string.IsNullOrWhiteSpace(text))
            {
                if (!options.AllowEmpty)
                {
                    errors.Add(new TextValidationError("EMPTY_TEXT", "Text is empty", "Text", text, null));
                }
                return new TextValidationResult(errors.Count == 0, "Text validation completed", text, warnings, errors);
            }

            // Check text length
            if (text.Length < options.MinLength)
            {
                errors.Add(new TextValidationError("TEXT_TOO_SHORT", "Text is too short", "Text", text, null));
            }

            if (text.Length > options.MaxLength)
            {
                errors.Add(new TextValidationError("TEXT_TOO_LONG", "Text is too long", "Text", text, null));
            }

            // Check for whitespace
            if (!options.AllowWhitespace && text.Any(char.IsWhiteSpace))
            {
                errors.Add(new TextValidationError("WHITESPACE_NOT_ALLOWED", "Text contains whitespace", "Text", text, null));
            }

            // Check allowed characters
            if (options.AllowedCharacters.Any())
            {
                var invalidCharacters = text.Where(c => !options.AllowedCharacters.Contains(c.ToString())).ToList();
                if (invalidCharacters.Any())
                {
                    errors.Add(new TextValidationError("INVALID_CHARACTERS", "Text contains invalid characters", "Text", text, null));
                }
            }

            return new TextValidationResult(errors.Count == 0, "Text validation completed", text, warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate text with options: {Text}", text);
            return new TextValidationResult(false, "Text validation failed", text, new List<TextValidationWarning>(), new List<TextValidationError> { new TextValidationError("VALIDATION_FAILED", ex.Message, "Text", text, ex) });
        }
    }

    public async Task<List<TextValidationResult>> ValidateTextsAsync(List<string> texts)
    {
        try
        {
            var results = new List<TextValidationResult>();

            foreach (var text in texts)
            {
                var result = await ValidateTextAsync(text);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate texts");
            return new List<TextValidationResult>();
        }
    }

    public async Task<List<TextValidationResult>> ValidateTextsAsync(List<string> texts, TextValidationOptions options)
    {
        try
        {
            var results = new List<TextValidationResult>();

            foreach (var text in texts)
            {
                var result = await ValidateTextAsync(text, options);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate texts with options");
            return new List<TextValidationResult>();
        }
    }

    private string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
