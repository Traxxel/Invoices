using Invoice.Domain.Enums;

namespace Invoice.Domain.Extensions;

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

