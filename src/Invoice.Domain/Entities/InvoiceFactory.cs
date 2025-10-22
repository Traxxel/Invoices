namespace Invoice.Domain.Entities;

public static class InvoiceFactory
{
    public static Invoice CreateFromExtraction(
        string invoiceNumber,
        DateOnly invoiceDate,
        string issuerName,
        string issuerStreet,
        string issuerPostalCode,
        string issuerCity,
        string? issuerCountry,
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        string sourceFilePath,
        float extractionConfidence,
        string modelVersion)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            IssuerName = issuerName,
            IssuerStreet = issuerStreet,
            IssuerPostalCode = issuerPostalCode,
            IssuerCity = issuerCity,
            IssuerCountry = issuerCountry,
            NetTotal = netTotal,
            VatTotal = vatTotal,
            GrossTotal = grossTotal,
            SourceFilePath = sourceFilePath,
            ExtractionConfidence = extractionConfidence,
            ModelVersion = modelVersion
        };

        return invoice;
    }

    public static Invoice CreateManual(
        string invoiceNumber,
        DateOnly invoiceDate,
        string issuerName,
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        string sourceFilePath)
    {
        return new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            IssuerName = issuerName,
            NetTotal = netTotal,
            VatTotal = vatTotal,
            GrossTotal = grossTotal,
            SourceFilePath = sourceFilePath,
            ExtractionConfidence = 1.0f, // Manuell erstellt = 100% Confidence
            ModelVersion = "manual"
        };
    }
}

