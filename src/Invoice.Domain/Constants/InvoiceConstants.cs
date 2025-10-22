namespace Invoice.Domain.Constants;

public static class InvoiceConstants
{
    // Field Limits
    public const int MaxInvoiceNumberLength = 100;
    public const int MinInvoiceNumberLength = 3;
    public const int MaxIssuerNameLength = 200;
    public const int MaxIssuerStreetLength = 200;
    public const int MaxIssuerPostalCodeLength = 20;
    public const int MaxIssuerCityLength = 100;
    public const int MaxIssuerCountryLength = 100;
    public const int MaxModelVersionLength = 50;
    public const int MaxSourceFilePathLength = 500;

    // Financial Limits
    public const decimal MaxInvoiceAmount = 1000000m;
    public const decimal MinInvoiceAmount = 0.01m;
    public const decimal MaxVatRate = 50m;
    public const decimal MinVatRate = 0m;
    public const decimal AmountTolerance = 0.02m;

    // Date Limits
    public const int MaxFutureDays = 7;
    public const int MaxPastYears = 10;

    // Confidence Thresholds
    public const float HighConfidenceThreshold = 0.8f;
    public const float MediumConfidenceThreshold = 0.5f;
    public const float LowConfidenceThreshold = 0.3f;
    public const float VeryLowConfidenceThreshold = 0.1f;

    // File Limits
    public const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    public const int MaxFileSizeMB = 50;

    // Validation
    public const int MaxValidationErrors = 100;
    public const int MaxValidationWarnings = 50;
}

