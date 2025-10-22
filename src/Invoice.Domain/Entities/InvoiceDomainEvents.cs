namespace Invoice.Domain.Entities;

public class InvoiceCreatedEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InvoiceUpdatedEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string[] ChangedFields { get; set; } = Array.Empty<string>();
    public DateTime UpdatedAt { get; set; }
}

public class InvoiceFinancialsUpdatedEvent
{
    public Guid InvoiceId { get; set; }
    public decimal OldNetTotal { get; set; }
    public decimal NewNetTotal { get; set; }
    public decimal OldGrossTotal { get; set; }
    public decimal NewGrossTotal { get; set; }
    public DateTime UpdatedAt { get; set; }
}

