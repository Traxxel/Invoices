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

