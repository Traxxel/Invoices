# Aufgabe 08: Value Objects (Money, Address, InvoiceNumber)

## Ziel

Domain-spezifische Value Objects für typsichere und validierte Werte definieren.

## 1. Money Value Object

**Datei:** `src/Invoice.Domain/ValueObjects/Money.cs`

```csharp
using System.Globalization;

namespace Invoice.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero => new(0m);
    public static Money Euro(decimal amount) => new(amount, "EUR");
    public static Money Dollar(decimal amount) => new(amount, "USD");

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide by zero");

        return new Money(money.Amount / divisor, money.Currency);
    }

    public static bool operator ==(Money left, Money right)
    {
        return left.Amount == right.Amount && left.Currency == right.Currency;
    }

    public static bool operator !=(Money left, Money right)
    {
        return !(left == right);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        return left > right || left == right;
    }

    public static bool operator <=(Money left, Money right)
    {
        return left < right || left == right;
    }

    public Money Round(int decimals = 2)
    {
        return new Money(Math.Round(Amount, decimals), Currency);
    }

    public Money Abs()
    {
        return new Money(Math.Abs(Amount), Currency);
    }

    public bool IsZero => Amount == 0;
    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    public string ToString(string format)
    {
        return Amount.ToString(format) + $" {Currency}";
    }

    public string ToLocalizedString(CultureInfo culture)
    {
        return Amount.ToString("C", culture);
    }
}
```

## 2. Address Value Object

**Datei:** `src/Invoice.Domain/ValueObjects/Address.cs`

```csharp
namespace Invoice.Domain.ValueObjects;

public readonly record struct Address
{
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string? Country { get; }

    public Address(string street, string postalCode, string city, string? country = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Country = country;

        if (string.IsNullOrWhiteSpace(Street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));

        if (string.IsNullOrWhiteSpace(PostalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(City))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
    }

    public static Address Create(string street, string postalCode, string city, string? country = null)
    {
        return new Address(street, postalCode, city, country);
    }

    public static Address Empty => new("", "", "");

    public bool IsEmpty => string.IsNullOrWhiteSpace(Street) &&
                          string.IsNullOrWhiteSpace(PostalCode) &&
                          string.IsNullOrWhiteSpace(City);

    public string ToSingleLine()
    {
        if (IsEmpty) return string.Empty;

        var parts = new List<string> { Street, $"{PostalCode} {City}" };
        if (!string.IsNullOrWhiteSpace(Country))
            parts.Add(Country);

        return string.Join(", ", parts);
    }

    public string ToMultiLine()
    {
        if (IsEmpty) return string.Empty;

        var lines = new List<string> { Street, $"{PostalCode} {City}" };
        if (!string.IsNullOrWhiteSpace(Country))
            lines.Add(Country);

        return string.Join(Environment.NewLine, lines);
    }

    public override string ToString()
    {
        return ToSingleLine();
    }
}
```

## 3. InvoiceNumber Value Object

**Datei:** `src/Invoice.Domain/ValueObjects/InvoiceNumber.cs`

```csharp
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
```

## 4. DateRange Value Object

**Datei:** `src/Invoice.Domain/ValueObjects/DateRange.cs`

```csharp
namespace Invoice.Domain.ValueObjects;

public readonly record struct DateRange
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public DateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateOnly startDate, DateOnly endDate)
    {
        return new DateRange(startDate, endDate);
    }

    public static DateRange Create(int year, int month)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new DateRange(startDate, endDate);
    }

    public static DateRange Create(int year)
    {
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year, 12, 31);
        return new DateRange(startDate, endDate);
    }

    public bool Contains(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public int Days => (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;

    public bool IsEmpty => StartDate == EndDate;

    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
    }
}
```

## 5. Confidence Value Object

**Datei:** `src/Invoice.Domain/ValueObjects/Confidence.cs`

```csharp
namespace Invoice.Domain.ValueObjects;

public readonly record struct Confidence
{
    public float Value { get; }

    public Confidence(float value)
    {
        if (value < 0.0f || value > 1.0f)
            throw new ArgumentException("Confidence must be between 0.0 and 1.0", nameof(value));

        Value = value;
    }

    public static Confidence Create(float value)
    {
        return new Confidence(value);
    }

    public static Confidence Zero => new(0.0f);
    public static Confidence One => new(1.0f);
    public static Confidence Half => new(0.5f);

    public bool IsHigh => Value >= 0.8f;
    public bool IsMedium => Value >= 0.5f && Value < 0.8f;
    public bool IsLow => Value < 0.5f;
    public bool IsVeryLow => Value < 0.3f;

    public ConfidenceLevel GetLevel()
    {
        if (IsHigh) return ConfidenceLevel.High;
        if (IsMedium) return ConfidenceLevel.Medium;
        if (IsLow) return ConfidenceLevel.Low;
        return ConfidenceLevel.VeryLow;
    }

    public override string ToString()
    {
        return $"{Value:P1}";
    }

    public static implicit operator float(Confidence confidence)
    {
        return confidence.Value;
    }

    public static implicit operator Confidence(float value)
    {
        return new Confidence(value);
    }
}

public enum ConfidenceLevel
{
    VeryLow,
    Low,
    Medium,
    High
}
```

## 6. Value Object Extensions

**Datei:** `src/Invoice.Domain/ValueObjects/ValueObjectExtensions.cs`

```csharp
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
```

## Wichtige Hinweise

- Alle Value Objects sind immutable (readonly record struct)
- Validierung in Konstruktoren
- Operator Overloads für natürliche Verwendung
- Factory Methods für verschiedene Erstellungs-Szenarien
- Computed Properties für Business Logic
- Implicit Conversions für einfache Verwendung
- ToString() Overrides für Debugging
- Extension Methods für Fluent API
- Typsicherheit durch Value Objects
