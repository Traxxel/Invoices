using Invoice.Domain.Entities;
using Invoice.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Data;

public class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Domain.Entities.Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceRawBlock> InvoiceRawBlocks { get; set; } = null!;

    // Override OnModelCreating
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceRawBlockConfiguration());

        // Apply global query filters if needed
        ApplyGlobalQueryFilters(modelBuilder);
    }

    // Override OnConfiguring for additional configuration
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback configuration if not configured via DI
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Invoice;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        base.OnConfiguring(optionsBuilder);
    }

    // Global query filters
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Example: Filter out soft-deleted records if implementing soft delete
        // modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
    }

    // Database operations
    public async Task<bool> EnsureCreatedAsync()
    {
        try
        {
            await Database.EnsureCreatedAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EnsureDeletedAsync()
    {
        try
        {
            await Database.EnsureDeletedAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    // Migration operations
    public async Task<bool> MigrateAsync()
    {
        try
        {
            await Database.MigrateAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> HasPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await Database.GetPendingMigrationsAsync();
            return pendingMigrations.Any();
        }
        catch
        {
            return false;
        }
    }

    // Statistics
    public async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        var invoiceCount = await Invoices.CountAsync();
        var rawBlockCount = await InvoiceRawBlocks.CountAsync();
        var totalFileSize = await Invoices.SumAsync(i => i.SourceFilePath.Length); // Simplified

        return new DatabaseStatistics
        {
            InvoiceCount = invoiceCount,
            RawBlockCount = rawBlockCount,
            TotalFileSize = totalFileSize,
            LastUpdated = DateTime.UtcNow
        };
    }
}

// Statistics model
public class DatabaseStatistics
{
    public int InvoiceCount { get; set; }
    public int RawBlockCount { get; set; }
    public long TotalFileSize { get; set; }
    public DateTime LastUpdated { get; set; }
}

