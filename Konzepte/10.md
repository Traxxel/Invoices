# Aufgabe 10: Domain Enums und Constants

## Ziel

Domain-spezifische Enums und Constants für typsichere und wartbare Domain-Logik definieren.

## 1. Domain Enums

**Datei:** `src/InvoiceReader.Domain/Enums/InvoiceStatus.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum InvoiceStatus
{
    Draft,
    Imported,
    Validated,
    Approved,
    Rejected,
    Processed,
    Archived
}
```

**Datei:** `src/InvoiceReader.Domain/Enums/ExtractionStatus.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum ExtractionStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    RequiresReview,
    ManuallyCorrected
}
```

**Datei:** `src/InvoiceReader.Domain/Enums/FieldType.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum FieldType
{
    None,
    InvoiceNumber,
    InvoiceDate,
    IssuerAddress,
    NetTotal,
    VatTotal,
    GrossTotal
}
```

**Datei:** `src/InvoiceReader.Domain/Enums/ConfidenceLevel.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum ConfidenceLevel
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4
}
```

**Datei:** `src/InvoiceReader.Domain/Enums/ValidationSeverity.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
```

**Datei:** `src/InvoiceReader.Domain/Enums/FileType.cs`

```csharp
namespace InvoiceReader.Domain.Enums;

public enum FileType
{
    PDF,
    Image,
    Unknown
}
```

## 2. Domain Constants

**Datei:** `src/InvoiceReader.Domain/Constants/InvoiceConstants.cs`

```csharp
namespace InvoiceReader.Domain.Constants;

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
```

**Datei:** `src/InvoiceReader.Domain/Constants/MLConstants.cs`

```csharp
namespace InvoiceReader.Domain.Constants;

public static class MLConstants
{
    // Model Versions
    public const string DefaultModelVersion = "v1.0";
    public const string ManualModelVersion = "manual";
    public const string UnknownModelVersion = "unknown";

    // Training Data
    public const int MinTrainingSamples = 100;
    public const int RecommendedTrainingSamples = 1000;
    public const int MaxTrainingSamples = 10000;

    // Data Splits
    public const float DefaultTrainSplit = 0.8f;
    public const float DefaultValidationSplit = 0.1f;
    public const float DefaultTestSplit = 0.1f;

    // Performance Thresholds
    public const float MinAccuracyThreshold = 0.85f;
    public const float MinPrecisionThreshold = 0.80f;
    public const float MinRecallThreshold = 0.80f;
    public const float MinF1ScoreThreshold = 0.80f;

    // Feature Limits
    public const int MaxTextLength = 4000;
    public const int MaxContextLines = 3;
    public const int MaxRegexHits = 10;

    // Model Storage
    public const string ModelFileExtension = ".zip";
    public const string TrainingDataExtension = ".jsonl";
    public const string LabeledDataExtension = ".tsv";
}
```

**Datei:** `src/InvoiceReader.Domain/Constants/FileConstants.cs`

```csharp
namespace InvoiceReader.Domain.Constants;

public static class FileConstants
{
    // Supported Extensions
    public static readonly string[] SupportedExtensions = { ".pdf" };
    public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".tiff", ".bmp" };

    // File Paths
    public const string StorageBasePath = "storage";
    public const string InvoiceSubPath = "invoices";
    public const string ModelsPath = "data/models";
    public const string LabeledDataPath = "data/labeled";
    public const string SamplesPath = "data/samples";
    public const string LogsPath = "logs";

    // File Naming
    public const string FileNameFormat = "{0}.pdf";
    public const string ModelFileNameFormat = "field_classifier_{0}.zip";
    public const string TrainingDataFileNameFormat = "training_data_{0}.jsonl";

    // Retention
    public const int DefaultRetentionYears = 10;
    public const int MaxRetentionYears = 20;
    public const int MinRetentionYears = 1;

    // File Operations
    public const int MaxConcurrentFileOperations = 10;
    public const int FileOperationTimeoutSeconds = 30;
    public const int MaxRetryAttempts = 3;
}
```

**Datei:** `src/InvoiceReader.Domain/Constants/ValidationConstants.cs`

```csharp
namespace InvoiceReader.Domain.Constants;

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
```

## 3. Enum Extensions

**Datei:** `src/InvoiceReader.Domain/Extensions/EnumExtensions.cs`

```csharp
using InvoiceReader.Domain.Enums;

namespace InvoiceReader.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Draft => "Draft",
            InvoiceStatus.Imported => "Imported",
            InvoiceStatus.Validated => "Validated",
            InvoiceStatus.Approved => "Approved",
            InvoiceStatus.Rejected => "Rejected",
            InvoiceStatus.Processed => "Processed",
            InvoiceStatus.Archived => "Archived",
            _ => status.ToString()
        };
    }

    public static string GetDisplayName(this FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.None => "None",
            FieldType.InvoiceNumber => "Invoice Number",
            FieldType.InvoiceDate => "Invoice Date",
            FieldType.IssuerAddress => "Issuer Address",
            FieldType.NetTotal => "Net Total",
            FieldType.VatTotal => "VAT Total",
            FieldType.GrossTotal => "Gross Total",
            _ => fieldType.ToString()
        };
    }

    public static string GetDisplayName(this ConfidenceLevel confidenceLevel)
    {
        return confidenceLevel switch
        {
            ConfidenceLevel.VeryLow => "Very Low",
            ConfidenceLevel.Low => "Low",
            ConfidenceLevel.Medium => "Medium",
            ConfidenceLevel.High => "High",
            ConfidenceLevel.VeryHigh => "Very High",
            _ => confidenceLevel.ToString()
        };
    }

    public static string GetDisplayName(this ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Info => "Information",
            ValidationSeverity.Warning => "Warning",
            ValidationSeverity.Error => "Error",
            ValidationSeverity.Critical => "Critical",
            _ => severity.ToString()
        };
    }

    public static bool IsValid(this FieldType fieldType)
    {
        return fieldType != FieldType.None;
    }

    public static bool IsFinancialField(this FieldType fieldType)
    {
        return fieldType is FieldType.NetTotal or FieldType.VatTotal or FieldType.GrossTotal;
    }

    public static bool IsAddressField(this FieldType fieldType)
    {
        return fieldType == FieldType.IssuerAddress;
    }

    public static bool IsDateField(this FieldType fieldType)
    {
        return fieldType == FieldType.InvoiceDate;
    }

    public static bool IsNumberField(this FieldType fieldType)
    {
        return fieldType == FieldType.InvoiceNumber;
    }
}
```

## 4. Constants Extensions

**Datei:** `src/InvoiceReader.Domain/Extensions/ConstantsExtensions.cs`

```csharp
using InvoiceReader.Domain.Constants;
using InvoiceReader.Domain.Enums;

namespace InvoiceReader.Domain.Extensions;

public static class ConstantsExtensions
{
    public static bool IsValidInvoiceNumber(this string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return false;

        if (invoiceNumber.Length < InvoiceConstants.MinInvoiceNumberLength ||
            invoiceNumber.Length > InvoiceConstants.MaxInvoiceNumberLength)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(invoiceNumber, ValidationConstants.InvoiceNumberPattern);
    }

    public static bool IsValidAmount(this decimal amount)
    {
        return amount >= InvoiceConstants.MinInvoiceAmount && amount <= InvoiceConstants.MaxInvoiceAmount;
    }

    public static bool IsValidVatRate(this decimal vatRate)
    {
        return vatRate >= InvoiceConstants.MinVatRate && vatRate <= InvoiceConstants.MaxVatRate;
    }

    public static bool IsHighConfidence(this float confidence)
    {
        return confidence >= InvoiceConstants.HighConfidenceThreshold;
    }

    public static bool IsLowConfidence(this float confidence)
    {
        return confidence < InvoiceConstants.LowConfidenceThreshold;
    }

    public static ConfidenceLevel GetConfidenceLevel(this float confidence)
    {
        if (confidence >= InvoiceConstants.HighConfidenceThreshold)
            return ConfidenceLevel.High;
        if (confidence >= InvoiceConstants.MediumConfidenceThreshold)
            return ConfidenceLevel.Medium;
        if (confidence >= InvoiceConstants.LowConfidenceThreshold)
            return ConfidenceLevel.Low;
        return ConfidenceLevel.VeryLow;
    }

    public static bool IsSupportedFileExtension(this string extension)
    {
        return FileConstants.SupportedExtensions.Contains(extension.ToLowerInvariant());
    }

    public static bool IsImageFileExtension(this string extension)
    {
        return FileConstants.ImageExtensions.Contains(extension.ToLowerInvariant());
    }

    public static string GetModelFileName(this string version)
    {
        return string.Format(FileConstants.ModelFileNameFormat, version);
    }

    public static string GetTrainingDataFileName(this string version)
    {
        return string.Format(FileConstants.TrainingDataFileNameFormat, version);
    }
}
```

## Wichtige Hinweise

- Alle Enums mit Display Names für UI
- Constants für alle Domain-spezifischen Werte
- Extension Methods für Enum-Funktionalität
- Regex Patterns für Validierung
- File-spezifische Constants
- ML-spezifische Constants
- Validation-spezifische Constants
- Typsichere Enum-Operations
- Erweiterbare Constants-Struktur
