using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoice.Infrastructure.Data.Configurations;

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

