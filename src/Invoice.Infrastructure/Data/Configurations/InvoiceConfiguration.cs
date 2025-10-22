using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoice.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Domain.Entities.Invoice>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Invoice> builder)
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

