namespace Invoice.Domain.Entities;

public class InvoiceComparer : IEqualityComparer<Invoice>
{
    public bool Equals(Invoice? x, Invoice? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.InvoiceNumber == y.InvoiceNumber &&
               x.InvoiceDate == y.InvoiceDate &&
               x.GrossTotal == y.GrossTotal;
    }

    public int GetHashCode(Invoice obj)
    {
        return HashCode.Combine(obj.InvoiceNumber, obj.InvoiceDate, obj.GrossTotal);
    }
}

