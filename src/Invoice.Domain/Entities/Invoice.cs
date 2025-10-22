using System.ComponentModel.DataAnnotations;

namespace Invoice.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateOnly InvoiceDate { get; set; }

    // Issuer Information
    [Required]
    [MaxLength(200)]
    public string IssuerName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string IssuerStreet { get; set; } = string.Empty;

    [MaxLength(20)]
    public string IssuerPostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string IssuerCity { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? IssuerCountry { get; set; }

    // Financial Information
    [Required]
    public decimal NetTotal { get; set; }

    [Required]
    public decimal VatTotal { get; set; }

    [Required]
    public decimal GrossTotal { get; set; }

    // Metadata
    [Required]
    [MaxLength(500)]
    public string SourceFilePath { get; set; } = string.Empty;

    public DateTime ImportedAt { get; set; }

    public float ExtractionConfidence { get; set; }

    [MaxLength(50)]
    public string ModelVersion { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<InvoiceRawBlock> RawBlocks { get; set; } = new List<InvoiceRawBlock>();

    // Computed Properties
    public bool IsValid => ValidateInvoice();

    public decimal VatRate => NetTotal > 0 ? (VatTotal / NetTotal) * 100 : 0;

    // Constructor
    public Invoice()
    {
        Id = Guid.NewGuid();
        ImportedAt = DateTime.UtcNow;
    }

    // Factory Methods
    public static Invoice Create(
        string invoiceNumber,
        DateOnly invoiceDate,
        string issuerName,
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        string sourceFilePath,
        float extractionConfidence = 0.0f,
        string modelVersion = "")
    {
        return new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            IssuerName = issuerName,
            NetTotal = netTotal,
            VatTotal = vatTotal,
            GrossTotal = grossTotal,
            SourceFilePath = sourceFilePath,
            ExtractionConfidence = extractionConfidence,
            ModelVersion = modelVersion
        };
    }

    // Business Methods
    public void UpdateFinancials(decimal netTotal, decimal vatTotal, decimal grossTotal)
    {
        NetTotal = netTotal;
        VatTotal = vatTotal;
        GrossTotal = grossTotal;
    }

    public void UpdateIssuerInfo(string name, string street, string postalCode, string city, string? country = null)
    {
        IssuerName = name;
        IssuerStreet = street;
        IssuerPostalCode = postalCode;
        IssuerCity = city;
        IssuerCountry = country;
    }

    public void UpdateExtractionInfo(float confidence, string modelVersion)
    {
        ExtractionConfidence = confidence;
        ModelVersion = modelVersion;
    }

    // Validation
    private bool ValidateInvoice()
    {
        return !string.IsNullOrWhiteSpace(InvoiceNumber) &&
               !string.IsNullOrWhiteSpace(IssuerName) &&
               NetTotal >= 0 &&
               VatTotal >= 0 &&
               GrossTotal >= 0 &&
               Math.Abs((NetTotal + VatTotal) - GrossTotal) <= 0.02m; // Toleranz für Rundungsfehler
    }

    public override string ToString()
    {
        return $"Invoice {InvoiceNumber} from {IssuerName} - {GrossTotal:C}";
    }
}

