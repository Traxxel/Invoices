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

