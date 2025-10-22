namespace Invoice.Infrastructure.Logging;

public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);

    // Spezifische Logging-Methoden für Invoice-Reader
    void LogInvoiceImport(string invoiceNumber, string filePath, bool success);
    void LogMLPrediction(string modelVersion, float confidence, string fieldType);
    void LogFileOperation(string operation, string filePath, bool success);
    void LogDatabaseOperation(string operation, int recordCount, bool success);
}

