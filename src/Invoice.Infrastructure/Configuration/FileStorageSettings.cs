namespace Invoice.Infrastructure.Configuration;

public class FileStorageSettings
{
    public string BasePath { get; set; } = "storage";
    public string InvoiceSubPath { get; set; } = "invoices";
    public bool UseYearMonthStructure { get; set; } = true;
    public bool GenerateUniqueFilenames { get; set; } = true;
    public bool CalculateFileHash { get; set; } = true;
}

