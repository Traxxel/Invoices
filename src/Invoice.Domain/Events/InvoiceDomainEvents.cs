namespace Invoice.Domain.Events;

public abstract class DomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class InvoiceCreatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public decimal GrossTotal { get; set; }
    public float ExtractionConfidence { get; set; }
}

public class InvoiceUpdatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string[] ChangedFields { get; set; } = Array.Empty<string>();
    public string ChangedBy { get; set; } = string.Empty;
}

public class InvoiceValidatedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string[] ValidationErrors { get; set; } = Array.Empty<string>();
    public string[] ValidationWarnings { get; set; } = Array.Empty<string>();
}

public class InvoiceSuspiciousAmountEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

