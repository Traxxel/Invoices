namespace Invoice.Domain.Constants;

public static class ValidationConstants
{
    // Regex Patterns
    public const string InvoiceNumberPattern = @"^[A-Za-z0-9\-\/\.]+$";
    public const string PostalCodePattern = @"^\d{5}$";
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string PhonePattern = @"^[\+]?[0-9\s\-\(\)]+$";

    // Date Patterns
    public const string GermanDatePattern = @"\b(0?[1-9]|[12][0-9]|3[01])\.(0?[1-9]|1[0-2])\.(19|20)\d\d\b";
    public const string IsoDatePattern = @"\b(19|20)\d\d-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])\b";

    // Amount Patterns
    public const string GermanAmountPattern = @"[-+]?\d{1,3}(\.\d{3})*,\d{2}";
    public const string EnglishAmountPattern = @"[-+]?\d{1,3}(,\d{3})*\.\d{2}";
    public const string CurrencyPattern = @"€|EUR|USD|\$";

    // VAT Rates (Germany)
    public static readonly decimal[] StandardVatRates = { 0m, 7m, 19m, 21m };
    public const decimal DefaultVatRate = 19m;
    public const decimal ReducedVatRate = 7m;

    // Business Rules
    public const int MaxDuplicateCheckDays = 7;
    public const int MaxSuspiciousAmountCheckDays = 30;
    public const int MaxAddressValidationDays = 90;

    // Error Messages
    public const string RequiredFieldMessage = "{0} is required";
    public const string InvalidFormatMessage = "{0} has invalid format";
    public const string OutOfRangeMessage = "{0} is out of valid range";
    public const string DuplicateMessage = "{0} already exists";
    public const string SuspiciousAmountMessage = "Amount seems unusual: {0}";
}

