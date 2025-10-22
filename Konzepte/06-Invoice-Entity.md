# Aufgabe 06: Invoice Entity mit allen Properties

## Ziel

Haupt-Entity für Rechnungen mit allen Properties gemäß Konzept definieren.

## 1. Invoice Entity

**Datei:** `src/InvoiceReader.Domain/Entities/Invoice.cs`

```csharp
using InvoiceReader.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace InvoiceReader.Domain.Entities;

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
```

## 2. Invoice Factory

**Datei:** `src/InvoiceReader.Domain/Entities/InvoiceFactory.cs`

```csharp
using InvoiceReader.Domain.ValueObjects;

namespace InvoiceReader.Domain.Entities;

public static class InvoiceFactory
{
    public static Invoice CreateFromExtraction(
        string invoiceNumber,
        DateOnly invoiceDate,
        string issuerName,
        string issuerStreet,
        string issuerPostalCode,
        string issuerCity,
        string? issuerCountry,
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        string sourceFilePath,
        float extractionConfidence,
        string modelVersion)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            IssuerName = issuerName,
            IssuerStreet = issuerStreet,
            IssuerPostalCode = issuerPostalCode,
            IssuerCity = issuerCity,
            IssuerCountry = issuerCountry,
            NetTotal = netTotal,
            VatTotal = vatTotal,
            GrossTotal = grossTotal,
            SourceFilePath = sourceFilePath,
            ExtractionConfidence = extractionConfidence,
            ModelVersion = modelVersion
        };

        return invoice;
    }

    public static Invoice CreateManual(
        string invoiceNumber,
        DateOnly invoiceDate,
        string issuerName,
        decimal netTotal,
        decimal vatTotal,
        decimal grossTotal,
        string sourceFilePath)
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
            ExtractionConfidence = 1.0f, // Manuell erstellt = 100% Confidence
            ModelVersion = "manual"
        };
    }
}
```

## 3. Invoice Domain Events

**Datei:** `src/InvoiceReader.Domain/Entities/InvoiceDomainEvents.cs`

```csharp
namespace InvoiceReader.Domain.Entities;

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
```

## 4. Invoice Specifications

**Datei:** `src/InvoiceReader.Domain/Entities/InvoiceSpecifications.cs`

```csharp
using System.Linq.Expressions;

namespace InvoiceReader.Domain.Entities;

public static class InvoiceSpecifications
{
    public static Expression<Func<Invoice, bool>> ByInvoiceNumber(string invoiceNumber)
    {
        return x => x.InvoiceNumber == invoiceNumber;
    }

    public static Expression<Func<Invoice, bool>> ByIssuerName(string issuerName)
    {
        return x => x.IssuerName.Contains(issuerName);
    }

    public static Expression<Func<Invoice, bool>> ByDateRange(DateOnly startDate, DateOnly endDate)
    {
        return x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate;
    }

    public static Expression<Func<Invoice, bool>> ByAmountRange(decimal minAmount, decimal maxAmount)
    {
        return x => x.GrossTotal >= minAmount && x.GrossTotal <= maxAmount;
    }

    public static Expression<Func<Invoice, bool>> ByConfidenceThreshold(float threshold)
    {
        return x => x.ExtractionConfidence >= threshold;
    }

    public static Expression<Func<Invoice, bool>> ByModelVersion(string modelVersion)
    {
        return x => x.ModelVersion == modelVersion;
    }

    public static Expression<Func<Invoice, bool>> RecentlyImported(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return x => x.ImportedAt >= cutoffDate;
    }
}
```

## 5. Invoice Comparer

**Datei:** `src/InvoiceReader.Domain/Entities/InvoiceComparer.cs`

```csharp
using System.Collections.Generic;

namespace InvoiceReader.Domain.Entities;

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
```

## Wichtige Hinweise

- Alle Properties gemäß Konzept definiert
- Validation-Logic in der Entity
- Factory Methods für verschiedene Erstellungs-Szenarien
- Domain Events für Audit-Trail
- Specifications für komplexe Queries
- Computed Properties für Business Logic
- Navigation Properties für EF Core
- ToString() für Debugging
- InvoiceComparer für Duplikat-Erkennung
