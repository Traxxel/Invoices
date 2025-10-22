using Invoice.Domain.Constants;
using Invoice.Domain.ValueObjects;

namespace Invoice.Domain.Extensions;

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

