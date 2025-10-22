namespace Invoice.Domain.ValueObjects;

public static class ValueObjectExtensions
{
    public static Money ToMoney(this decimal amount, string currency = "EUR")
    {
        return new Money(amount, currency);
    }

    public static Money ToMoney(this float amount, string currency = "EUR")
    {
        return new Money((decimal)amount, currency);
    }

    public static Money ToMoney(this double amount, string currency = "EUR")
    {
        return new Money((decimal)amount, currency);
    }

    public static InvoiceNumber ToInvoiceNumber(this string value)
    {
        return new InvoiceNumber(value);
    }

    public static Address ToAddress(this string street, string postalCode, string city, string? country = null)
    {
        return new Address(street, postalCode, city, country);
    }

    public static Confidence ToConfidence(this float value)
    {
        return new Confidence(value);
    }

    public static Confidence ToConfidence(this double value)
    {
        return new Confidence((float)value);
    }
}

