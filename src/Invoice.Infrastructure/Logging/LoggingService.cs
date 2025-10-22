using Serilog;

namespace Invoice.Infrastructure.Logging;

public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;

    public LoggingService(ILogger logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.Error(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.Error(exception, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }

    public void LogInvoiceImport(string invoiceNumber, string filePath, bool success)
    {
        if (success)
        {
            _logger.Information("Invoice imported successfully: {InvoiceNumber} from {FilePath}",
                invoiceNumber, filePath);
        }
        else
        {
            _logger.Warning("Invoice import failed: {InvoiceNumber} from {FilePath}",
                invoiceNumber, filePath);
        }
    }

    public void LogMLPrediction(string modelVersion, float confidence, string fieldType)
    {
        _logger.Debug("ML Prediction: Model {ModelVersion}, Field {FieldType}, Confidence {Confidence}",
            modelVersion, fieldType, confidence);
    }

    public void LogFileOperation(string operation, string filePath, bool success)
    {
        if (success)
        {
            _logger.Debug("File operation {Operation} successful: {FilePath}", operation, filePath);
        }
        else
        {
            _logger.Warning("File operation {Operation} failed: {FilePath}", operation, filePath);
        }
    }

    public void LogDatabaseOperation(string operation, int recordCount, bool success)
    {
        if (success)
        {
            _logger.Debug("Database operation {Operation} successful: {RecordCount} records",
                operation, recordCount);
        }
        else
        {
            _logger.Warning("Database operation {Operation} failed: {RecordCount} records",
                operation, recordCount);
        }
    }
}

