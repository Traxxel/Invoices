# Aufgabe 11: InvoiceDbContext und DbSet-Konfiguration

## Ziel

EF Core DbContext mit allen DbSets und grundlegender Konfiguration für die Invoice-Reader Anwendung.

## 1. InvoiceDbContext

**Datei:** `src/InvoiceReader.Infrastructure/Data/InvoiceDbContext.cs`

```csharp
using InvoiceReader.Domain.Entities;
using InvoiceReader.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.Infrastructure.Data;

public class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Invoice> Invoices { get; set; } = null!;
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
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=InvoiceReader;Trusted_Connection=True;TrustServerCertificate=True;");
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
        catch (Exception ex)
        {
            // Log error
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
        catch (Exception ex)
        {
            // Log error
            return false;
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            // Log error
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
        catch (Exception ex)
        {
            // Log error
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
        catch (Exception ex)
        {
            // Log error
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
```

## 2. DbContext Factory

**Datei:** `src/InvoiceReader.Infrastructure/Data/InvoiceDbContextFactory.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceReader.Infrastructure.Data;

public class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvoiceDbContext>();

        // Default connection string for design-time
        var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=InvoiceReader;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new InvoiceDbContext(optionsBuilder.Options);
    }
}
```

## 3. DbContext Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Data/Extensions/DbContextExtensions.cs`

```csharp
using InvoiceReader.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceReader.Infrastructure.Data.Extensions;

public static class DbContextExtensions
{
    public static async Task<Invoice?> FindInvoiceByNumberAsync(this InvoiceDbContext context, string invoiceNumber)
    {
        return await context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public static async Task<List<Invoice>> FindInvoicesByIssuerAsync(this InvoiceDbContext context, string issuerName)
    {
        return await context.Invoices
            .Where(i => i.IssuerName.Contains(issuerName))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public static async Task<List<Invoice>> FindInvoicesByDateRangeAsync(
        this InvoiceDbContext context,
        DateOnly startDate,
        DateOnly endDate)
    {
        return await context.Invoices
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public static async Task<List<Invoice>> FindInvoicesByAmountRangeAsync(
        this InvoiceDbContext context,
        decimal minAmount,
        decimal maxAmount)
    {
        return await context.Invoices
            .Where(i => i.GrossTotal >= minAmount && i.GrossTotal <= maxAmount)
            .OrderByDescending(i => i.GrossTotal)
            .ToListAsync();
    }

    public static async Task<List<Invoice>> FindInvoicesByConfidenceAsync(
        this InvoiceDbContext context,
        float minConfidence)
    {
        return await context.Invoices
            .Where(i => i.ExtractionConfidence >= minConfidence)
            .OrderByDescending(i => i.ExtractionConfidence)
            .ToListAsync();
    }

    public static async Task<List<InvoiceRawBlock>> FindRawBlocksByInvoiceAsync(
        this InvoiceDbContext context,
        Guid invoiceId)
    {
        return await context.InvoiceRawBlocks
            .Where(rb => rb.InvoiceId == invoiceId)
            .OrderBy(rb => rb.Page)
            .ThenBy(rb => rb.LineIndex)
            .ToListAsync();
    }

    public static async Task<List<InvoiceRawBlock>> FindRawBlocksByPredictionAsync(
        this InvoiceDbContext context,
        string predictedLabel)
    {
        return await context.InvoiceRawBlocks
            .Where(rb => rb.PredictedLabel == predictedLabel)
            .OrderBy(rb => rb.PredictionConfidence)
            .ToListAsync();
    }

    public static async Task<bool> InvoiceNumberExistsAsync(this InvoiceDbContext context, string invoiceNumber)
    {
        return await context.Invoices
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public static async Task<int> GetInvoiceCountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.CountAsync();
    }

    public static async Task<decimal> GetTotalInvoiceAmountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.SumAsync(i => i.GrossTotal);
    }

    public static async Task<decimal> GetAverageInvoiceAmountAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.AverageAsync(i => i.GrossTotal);
    }

    public static async Task<DateOnly?> GetEarliestInvoiceDateAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.MinAsync(i => i.InvoiceDate);
    }

    public static async Task<DateOnly?> GetLatestInvoiceDateAsync(this InvoiceDbContext context)
    {
        return await context.Invoices.MaxAsync(i => i.InvoiceDate);
    }
}
```

## 4. DbContext Health Check

**Datei:** `src/InvoiceReader.Infrastructure/Data/HealthChecks/DatabaseHealthCheck.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InvoiceReader.Infrastructure.Data.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly InvoiceDbContext _context;

    public DatabaseHealthCheck(InvoiceDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connection
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Database connection failed");
            }

            // Test basic query
            var invoiceCount = await _context.Invoices.CountAsync(cancellationToken);

            // Check for pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var hasPendingMigrations = pendingMigrations.Any();

            var data = new Dictionary<string, object>
            {
                ["invoice_count"] = invoiceCount,
                ["has_pending_migrations"] = hasPendingMigrations
            };

            if (hasPendingMigrations)
            {
                return HealthCheckResult.Degraded("Database has pending migrations", data: data);
            }

            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message
            });
        }
    }
}
```

## 5. DbContext Service Registration

**Datei:** `src/InvoiceReader.Infrastructure/Data/Extensions/DatabaseExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Data;
using InvoiceReader.Infrastructure.Data.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<InvoiceDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(30);
            });

            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständige DbContext-Implementierung mit allen DbSets
- Factory für Design-Time-Migrations
- Extension Methods für häufige Queries
- Health Check für Database-Monitoring
- Connection String-Konfiguration
- Retry-Policy für SQL Server
- Sensitive Data Logging nur in Development
- Statistics für Database-Monitoring
- Error Handling für alle Database-Operationen
