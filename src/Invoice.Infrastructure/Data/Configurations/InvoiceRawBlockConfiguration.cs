using Invoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoice.Infrastructure.Data.Configurations;

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

