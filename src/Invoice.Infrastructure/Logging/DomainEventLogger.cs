using Serilog;

namespace Invoice.Infrastructure.Logging;

public class DomainEventLogger
{
    private readonly ILogger _logger;

    public DomainEventLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogInvoiceCreated(Guid invoiceId, string invoiceNumber, DateTime createdDate)
    {
        _logger.Information("Invoice created: {InvoiceId}, Number: {InvoiceNumber}, Date: {CreatedDate}",
            invoiceId, invoiceNumber, createdDate);
    }

    public void LogInvoiceUpdated(Guid invoiceId, string invoiceNumber, string[] changedFields)
    {
        _logger.Information("Invoice updated: {InvoiceId}, Number: {InvoiceNumber}, Changed: {ChangedFields}",
            invoiceId, invoiceNumber, string.Join(", ", changedFields));
    }

    public void LogMLModelTrained(string modelVersion, int trainingSamples, float accuracy)
    {
        _logger.Information("ML Model trained: Version {ModelVersion}, Samples {TrainingSamples}, Accuracy {Accuracy}",
            modelVersion, trainingSamples, accuracy);
    }

    public void LogFileStored(Guid invoiceId, string filePath, long fileSize, string fileHash)
    {
        _logger.Debug("File stored: Invoice {InvoiceId}, Path {FilePath}, Size {FileSize}, Hash {FileHash}",
            invoiceId, filePath, fileSize, fileHash);
    }
}

