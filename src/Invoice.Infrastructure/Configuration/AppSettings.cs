namespace Invoice.Infrastructure.Configuration;

public class AppSettings
{
    public string StoragePath { get; set; } = "storage";
    public string ModelsPath { get; set; } = "data/models";
    public string LabeledDataPath { get; set; } = "data/labeled";
    public string SamplesPath { get; set; } = "data/samples";
    public string Culture { get; set; } = "de-DE";
    public string Currency { get; set; } = "EUR";
    public int MaxFileSizeMB { get; set; } = 50;
    public string[] SupportedFileExtensions { get; set; } = { ".pdf" };
    public int RetentionYears { get; set; } = 10;
}

