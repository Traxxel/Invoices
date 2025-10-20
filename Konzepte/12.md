# Aufgabe 12: Entity Configurations (Fluent API)

## Ziel

EF Core Fluent API-Konfigurationen für alle Entities mit Indizes, Constraints und Beziehungen.

## 1. Invoice Configuration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Configurations/InvoiceConfiguration.cs`

```csharp
using InvoiceReader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceReader.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // Table configuration
        builder.ToTable("Invoices");

        // Primary key
        builder.HasKey(i => i.Id);

        // Properties configuration
        builder.Property(i => i.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Unique invoice number");

        builder.Property(i => i.InvoiceDate)
            .IsRequired()
            .HasColumnType("date")
            .HasComment("Invoice date");

        // Issuer information
        builder.Property(i => i.IssuerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Name of the invoice issuer");

        builder.Property(i => i.IssuerStreet)
            .HasMaxLength(200)
            .HasComment("Issuer street address");

        builder.Property(i => i.IssuerPostalCode)
            .HasMaxLength(20)
            .HasComment("Issuer postal code");

        builder.Property(i => i.IssuerCity)
            .HasMaxLength(100)
            .HasComment("Issuer city");

        builder.Property(i => i.IssuerCountry)
            .HasMaxLength(100)
            .HasComment("Issuer country");

        // Financial information
        builder.Property(i => i.NetTotal)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Net total amount");

        builder.Property(i => i.VatTotal)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("VAT total amount");

        builder.Property(i => i.GrossTotal)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Gross total amount");

        // Metadata
        builder.Property(i => i.SourceFilePath)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Path to source PDF file");

        builder.Property(i => i.ImportedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Import timestamp");

        builder.Property(i => i.ExtractionConfidence)
            .IsRequired()
            .HasColumnType("real")
            .HasComment("ML extraction confidence (0.0-1.0)");

        builder.Property(i => i.ModelVersion)
            .HasMaxLength(50)
            .HasComment("ML model version used for extraction");

        // Indexes
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoices_InvoiceNumber_Unique");

        builder.HasIndex(i => i.InvoiceDate)
            .HasDatabaseName("IX_Invoices_InvoiceDate");

        builder.HasIndex(i => i.IssuerName)
            .HasDatabaseName("IX_Invoices_IssuerName");

        builder.HasIndex(i => i.ImportedAt)
            .HasDatabaseName("IX_Invoices_ImportedAt");

        builder.HasIndex(i => i.ExtractionConfidence)
            .HasDatabaseName("IX_Invoices_ExtractionConfidence");

        builder.HasIndex(i => i.ModelVersion)
            .HasDatabaseName("IX_Invoices_ModelVersion");

        // Composite indexes
        builder.HasIndex(i => new { i.InvoiceDate, i.IssuerName })
            .HasDatabaseName("IX_Invoices_Date_Issuer");

        builder.HasIndex(i => new { i.GrossTotal, i.InvoiceDate })
            .HasDatabaseName("IX_Invoices_Amount_Date");

        // Check constraints
        builder.HasCheckConstraint("CK_Invoices_NetTotal_Positive", "NetTotal >= 0");
        builder.HasCheckConstraint("CK_Invoices_VatTotal_Positive", "VatTotal >= 0");
        builder.HasCheckConstraint("CK_Invoices_GrossTotal_Positive", "GrossTotal >= 0");
        builder.HasCheckConstraint("CK_Invoices_Confidence_Range", "ExtractionConfidence >= 0.0 AND ExtractionConfidence <= 1.0");
        builder.HasCheckConstraint("CK_Invoices_Amount_Consistency", "ABS((NetTotal + VatTotal) - GrossTotal) <= 0.02");

        // Navigation properties
        builder.HasMany(i => i.RawBlocks)
            .WithOne(rb => rb.Invoice)
            .HasForeignKey(rb => rb.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## 2. InvoiceRawBlock Configuration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Configurations/InvoiceRawBlockConfiguration.cs`

```csharp
using InvoiceReader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceReader.Infrastructure.Data.Configurations;

public class InvoiceRawBlockConfiguration : IEntityTypeConfiguration<InvoiceRawBlock>
{
    public void Configure(EntityTypeBuilder<InvoiceRawBlock> builder)
    {
        // Table configuration
        builder.ToTable("InvoiceRawBlocks");

        // Primary key
        builder.HasKey(rb => rb.Id);

        // Properties configuration
        builder.Property(rb => rb.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(rb => rb.InvoiceId)
            .IsRequired()
            .HasComment("Foreign key to Invoice");

        builder.Property(rb => rb.Page)
            .IsRequired()
            .HasComment("Page number in PDF");

        builder.Property(rb => rb.Text)
            .IsRequired()
            .HasMaxLength(4000)
            .HasComment("Extracted text content");

        builder.Property(rb => rb.X)
            .IsRequired()
            .HasColumnType("real")
            .HasComment("X coordinate of bounding box");

        builder.Property(rb => rb.Y)
            .IsRequired()
            .HasColumnType("real")
            .HasComment("Y coordinate of bounding box");

        builder.Property(rb => rb.Width)
            .IsRequired()
            .HasColumnType("real")
            .HasComment("Width of bounding box");

        builder.Property(rb => rb.Height)
            .IsRequired()
            .HasColumnType("real")
            .HasComment("Height of bounding box");

        builder.Property(rb => rb.LineIndex)
            .IsRequired()
            .HasComment("Line index within page");

        builder.Property(rb => rb.PredictedLabel)
            .HasMaxLength(50)
            .HasComment("ML predicted label");

        builder.Property(rb => rb.PredictionConfidence)
            .HasColumnType("real")
            .HasComment("ML prediction confidence");

        builder.Property(rb => rb.ActualLabel)
            .HasMaxLength(50)
            .HasComment("Manually assigned label");

        builder.Property(rb => rb.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Creation timestamp");

        // Indexes
        builder.HasIndex(rb => rb.InvoiceId)
            .HasDatabaseName("IX_InvoiceRawBlocks_InvoiceId");

        builder.HasIndex(rb => rb.Page)
            .HasDatabaseName("IX_InvoiceRawBlocks_Page");

        builder.HasIndex(rb => rb.PredictedLabel)
            .HasDatabaseName("IX_InvoiceRawBlocks_PredictedLabel");

        builder.HasIndex(rb => rb.ActualLabel)
            .HasDatabaseName("IX_InvoiceRawBlocks_ActualLabel");

        builder.HasIndex(rb => rb.PredictionConfidence)
            .HasDatabaseName("IX_InvoiceRawBlocks_PredictionConfidence");

        builder.HasIndex(rb => rb.CreatedAt)
            .HasDatabaseName("IX_InvoiceRawBlocks_CreatedAt");

        // Composite indexes
        builder.HasIndex(rb => new { rb.InvoiceId, rb.Page })
            .HasDatabaseName("IX_InvoiceRawBlocks_Invoice_Page");

        builder.HasIndex(rb => new { rb.InvoiceId, rb.LineIndex })
            .HasDatabaseName("IX_InvoiceRawBlocks_Invoice_LineIndex");

        builder.HasIndex(rb => new { rb.PredictedLabel, rb.PredictionConfidence })
            .HasDatabaseName("IX_InvoiceRawBlocks_Label_Confidence");

        builder.HasIndex(rb => new { rb.ActualLabel, rb.PredictedLabel })
            .HasDatabaseName("IX_InvoiceRawBlocks_Actual_Predicted");

        // Check constraints
        builder.HasCheckConstraint("CK_InvoiceRawBlocks_Page_Positive", "Page > 0");
        builder.HasCheckConstraint("CK_InvoiceRawBlocks_Width_Positive", "Width > 0");
        builder.HasCheckConstraint("CK_InvoiceRawBlocks_Height_Positive", "Height > 0");
        builder.HasCheckConstraint("CK_InvoiceRawBlocks_LineIndex_Positive", "LineIndex >= 0");
        builder.HasCheckConstraint("CK_InvoiceRawBlocks_Confidence_Range", "PredictionConfidence IS NULL OR (PredictionConfidence >= 0.0 AND PredictionConfidence <= 1.0)");

        // Foreign key relationship
        builder.HasOne(rb => rb.Invoice)
            .WithMany(i => i.RawBlocks)
            .HasForeignKey(rb => rb.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## 3. Global Configuration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Configurations/GlobalConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceReader.Infrastructure.Data.Configurations;

public static class GlobalConfiguration
{
    public static void ConfigureGlobalProperties<T>(EntityTypeBuilder<T> builder) where T : class
    {
        // Configure common properties for all entities
        var entityType = typeof(T);

        // If entity has Id property
        var idProperty = entityType.GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            builder.Property("Id")
                .IsRequired()
                .HasDefaultValueSql("NEWID()");
        }

        // If entity has CreatedAt property
        var createdAtProperty = entityType.GetProperty("CreatedAt");
        if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
        {
            builder.Property("CreatedAt")
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
        }

        // If entity has UpdatedAt property
        var updatedAtProperty = entityType.GetProperty("UpdatedAt");
        if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
        {
            builder.Property("UpdatedAt")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }

    public static void ConfigureDecimalPrecision(EntityTypeBuilder builder, string propertyName, int precision = 18, int scale = 2)
    {
        builder.Property(propertyName)
            .HasColumnType($"decimal({precision},{scale})");
    }

    public static void ConfigureStringLength(EntityTypeBuilder builder, string propertyName, int maxLength)
    {
        builder.Property(propertyName)
            .HasMaxLength(maxLength);
    }

    public static void ConfigureRequiredString(EntityTypeBuilder builder, string propertyName, int maxLength)
    {
        builder.Property(propertyName)
            .IsRequired()
            .HasMaxLength(maxLength);
    }

    public static void ConfigureOptionalString(EntityTypeBuilder builder, string propertyName, int maxLength)
    {
        builder.Property(propertyName)
            .HasMaxLength(maxLength);
    }
}
```

## 4. Index Configuration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Configurations/IndexConfiguration.cs`

```csharp
using InvoiceReader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceReader.Infrastructure.Data.Configurations;

public static class IndexConfiguration
{
    public static void ConfigureInvoiceIndexes(EntityTypeBuilder<Invoice> builder)
    {
        // Single column indexes
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoices_InvoiceNumber_Unique");

        builder.HasIndex(i => i.InvoiceDate)
            .HasDatabaseName("IX_Invoices_InvoiceDate");

        builder.HasIndex(i => i.IssuerName)
            .HasDatabaseName("IX_Invoices_IssuerName");

        builder.HasIndex(i => i.ImportedAt)
            .HasDatabaseName("IX_Invoices_ImportedAt");

        builder.HasIndex(i => i.ExtractionConfidence)
            .HasDatabaseName("IX_Invoices_ExtractionConfidence");

        builder.HasIndex(i => i.ModelVersion)
            .HasDatabaseName("IX_Invoices_ModelVersion");

        // Composite indexes for common queries
        builder.HasIndex(i => new { i.InvoiceDate, i.IssuerName })
            .HasDatabaseName("IX_Invoices_Date_Issuer");

        builder.HasIndex(i => new { i.GrossTotal, i.InvoiceDate })
            .HasDatabaseName("IX_Invoices_Amount_Date");

        builder.HasIndex(i => new { i.ExtractionConfidence, i.ModelVersion })
            .HasDatabaseName("IX_Invoices_Confidence_Model");

        // Partial indexes for specific scenarios
        builder.HasIndex(i => i.InvoiceDate)
            .HasFilter("ExtractionConfidence < 0.5")
            .HasDatabaseName("IX_Invoices_Date_LowConfidence");

        builder.HasIndex(i => i.ImportedAt)
            .HasFilter("ImportedAt >= DATEADD(day, -30, GETUTCDATE())")
            .HasDatabaseName("IX_Invoices_ImportedAt_Recent");
    }

    public static void ConfigureRawBlockIndexes(EntityTypeBuilder<InvoiceRawBlock> builder)
    {
        // Single column indexes
        builder.HasIndex(rb => rb.InvoiceId)
            .HasDatabaseName("IX_InvoiceRawBlocks_InvoiceId");

        builder.HasIndex(rb => rb.Page)
            .HasDatabaseName("IX_InvoiceRawBlocks_Page");

        builder.HasIndex(rb => rb.PredictedLabel)
            .HasDatabaseName("IX_InvoiceRawBlocks_PredictedLabel");

        builder.HasIndex(rb => rb.ActualLabel)
            .HasDatabaseName("IX_InvoiceRawBlocks_ActualLabel");

        builder.HasIndex(rb => rb.PredictionConfidence)
            .HasDatabaseName("IX_InvoiceRawBlocks_PredictionConfidence");

        // Composite indexes
        builder.HasIndex(rb => new { rb.InvoiceId, rb.Page })
            .HasDatabaseName("IX_InvoiceRawBlocks_Invoice_Page");

        builder.HasIndex(rb => new { rb.InvoiceId, rb.LineIndex })
            .HasDatabaseName("IX_InvoiceRawBlocks_Invoice_LineIndex");

        builder.HasIndex(rb => new { rb.PredictedLabel, rb.PredictionConfidence })
            .HasDatabaseName("IX_InvoiceRawBlocks_Label_Confidence");

        builder.HasIndex(rb => new { rb.ActualLabel, rb.PredictedLabel })
            .HasDatabaseName("IX_InvoiceRawBlocks_Actual_Predicted");

        // Partial indexes
        builder.HasIndex(rb => rb.PredictionConfidence)
            .HasFilter("PredictionConfidence IS NOT NULL")
            .HasDatabaseName("IX_InvoiceRawBlocks_PredictionConfidence_NotNull");

        builder.HasIndex(rb => rb.ActualLabel)
            .HasFilter("ActualLabel IS NOT NULL")
            .HasDatabaseName("IX_InvoiceRawBlocks_ActualLabel_NotNull");
    }
}
```

## 5. Configuration Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Data/Extensions/ConfigurationExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace InvoiceReader.Infrastructure.Data.Extensions;

public static class ConfigurationExtensions
{
    public static ModelBuilder ApplyAllConfigurations(this ModelBuilder modelBuilder)
    {
        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceRawBlockConfiguration());

        // Apply global configurations
        ApplyGlobalConfigurations(modelBuilder);

        return modelBuilder;
    }

    private static void ApplyGlobalConfigurations(ModelBuilder modelBuilder)
    {
        // Configure all entities with common properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure decimal precision for all decimal properties
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal))
                {
                    property.SetColumnType("decimal(18,2)");
                }
            }
        }
    }

    public static ModelBuilder ConfigureIndexes(this ModelBuilder modelBuilder)
    {
        // Apply index configurations
        IndexConfiguration.ConfigureInvoiceIndexes(
            modelBuilder.Entity<InvoiceReader.Domain.Entities.Invoice>());

        IndexConfiguration.ConfigureRawBlockIndexes(
            modelBuilder.Entity<InvoiceReader.Domain.Entities.InvoiceRawBlock>());

        return modelBuilder;
    }
}
```

## Wichtige Hinweise

- Vollständige Fluent API-Konfiguration für alle Entities
- Indizes für Performance-Optimierung
- Check Constraints für Datenintegrität
- Composite Indexes für häufige Queries
- Partial Indexes für spezifische Szenarien
- Foreign Key-Beziehungen mit Cascade Delete
- Decimal-Precision für Geldbeträge
- String-Length-Limits für alle Text-Felder
- Kommentare für alle Properties
- Global Configuration für wiederverwendbare Patterns
