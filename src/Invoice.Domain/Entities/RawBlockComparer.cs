namespace Invoice.Domain.Entities;

public class RawBlockComparer : IEqualityComparer<InvoiceRawBlock>
{
    public bool Equals(InvoiceRawBlock? x, InvoiceRawBlock? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.InvoiceId == y.InvoiceId &&
               x.Page == y.Page &&
               x.LineIndex == y.LineIndex &&
               x.Text == y.Text;
    }

    public int GetHashCode(InvoiceRawBlock obj)
    {
        return HashCode.Combine(obj.InvoiceId, obj.Page, obj.LineIndex, obj.Text);
    }
}

public class RawBlockByPositionComparer : IComparer<InvoiceRawBlock>
{
    public int Compare(InvoiceRawBlock? x, InvoiceRawBlock? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Sort by page first, then by Y position, then by X position
        var pageComparison = x.Page.CompareTo(y.Page);
        if (pageComparison != 0) return pageComparison;

        var yComparison = x.Y.CompareTo(y.Y);
        if (yComparison != 0) return yComparison;

        return x.X.CompareTo(y.X);
    }
}

