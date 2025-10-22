using System.Text.RegularExpressions;

namespace Invoice.Domain.ValueObjects;

public readonly record struct InvoiceNumber
{
    public string Value { get; }

    public InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invoice number cannot be null or empty", nameof(value));

        Value = value.Trim();

        if (Value.Length < 3)
            throw new ArgumentException("Invoice number must be at least 3 characters long", nameof(value));

        if (Value.Length > 100)
            throw new ArgumentException("Invoice number cannot exceed 100 characters", nameof(value));
    }

    public static InvoiceNumber Create(string value)
    {
        return new InvoiceNumber(value);
    }

    public static InvoiceNumber Empty => new("UNKNOWN");

    public bool IsEmpty => Value == "UNKNOWN";

    public bool IsValid => !IsEmpty && Value.Length >= 3 && Value.Length <= 100;

    public string Normalize()
    {
        if (IsEmpty) return Value;

        // Remove extra whitespace and normalize
        var normalized = Regex.Replace(Value, @"\s+", " ").Trim();

        // Remove common prefixes if they exist
        var prefixes = new[] { "RE-", "INV-", "RG-", "RNR-", "Rechnungs-Nr.:", "RechnungsNr.:", "RechnungsNr:" };
        foreach (var prefix in prefixes)
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(prefix.Length).Trim();
                break;
            }
        }

        return normalized;
    }

    public bool Contains(string searchText)
    {
        return Value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    public bool StartsWith(string prefix)
    {
        return Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public bool EndsWith(string suffix)
    {
        return Value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(InvoiceNumber invoiceNumber)
    {
        return invoiceNumber.Value;
    }

    public static implicit operator InvoiceNumber(string value)
    {
        return new InvoiceNumber(value);
    }
}

