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

