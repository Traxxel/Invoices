namespace Invoice.Domain.Constants;

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

