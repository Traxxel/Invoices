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

