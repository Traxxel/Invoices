# Aufgabe 15: Database Seeding und First-Run Logic

## Ziel

Database Seeding für Initial Data und First-Run Logic für neue Installationen.

## 1. Database Seeder Interface

**Datei:** `src/Invoice.Application/Interfaces/IDatabaseSeeder.cs`

```csharp
namespace Invoice.Application.Interfaces;

public interface IDatabaseSeeder
{
    Task<bool> SeedAsync();
    Task<bool> IsSeededAsync();
    Task<bool> ClearSeedDataAsync();
    Task<SeedResult> GetSeedStatusAsync();
}

public class SeedResult
{
    public bool IsSeeded { get; set; }
    public int SeedCount { get; set; }
    public DateTime? LastSeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
```

## 2. Database Seeder Implementation

**Datei:** `src/Invoice.Infrastructure/Data/Seeders/DatabaseSeeder.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Domain.Entities;
using Invoice.Domain.ValueObjects;
using Invoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Seeders;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(InvoiceDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if already seeded
            if (await IsSeededAsync())
            {
                _logger.LogInformation("Database already seeded, skipping...");
                return true;
            }

            // Seed sample data
            await SeedSampleInvoicesAsync();
            await SeedSampleRawBlocksAsync();

            _logger.LogInformation("Database seeding completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed");
            return false;
        }
    }

    public async Task<bool> IsSeededAsync()
    {
        try
        {
            var count = await _context.Invoices.CountAsync();
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if database is seeded");
            return false;
        }
    }

    public async Task<bool> ClearSeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Clearing seed data...");

            // Delete all data
            await _context.InvoiceRawBlocks.ExecuteDeleteAsync();
            await _context.Invoices.ExecuteDeleteAsync();

            _logger.LogInformation("Seed data cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear seed data");
            return false;
        }
    }

    public async Task<SeedResult> GetSeedStatusAsync()
    {
        var result = new SeedResult();

        try
        {
            result.IsSeeded = await IsSeededAsync();
            result.SeedCount = await _context.Invoices.CountAsync();
            result.LastSeeded = await GetLastSeededDateAsync();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to get seed status: {ex.Message}");
        }

        return result;
    }

    private async Task SeedSampleInvoicesAsync()
    {
        var sampleInvoices = new List<Invoice>
        {
            Invoice.Create(
                "INV-2025-001",
                new DateOnly(2025, 1, 15),
                "Musterfirma GmbH",
                1000.00m,
                190.00m,
                1190.00m,
                "storage/invoices/2025/01/sample1.pdf",
                0.95f,
                "v1.0"
            ),
            Invoice.Create(
                "RE-2025-002",
                new DateOnly(2025, 1, 20),
                "Beispiel AG",
                500.00m,
                95.00m,
                595.00m,
                "storage/invoices/2025/01/sample2.pdf",
                0.88f,
                "v1.0"
            ),
            Invoice.Create(
                "RG-2025-003",
                new DateOnly(2025, 2, 1),
                "Test Unternehmen",
                2500.00m,
                475.00m,
                2975.00m,
                "storage/invoices/2025/02/sample3.pdf",
                0.92f,
                "v1.0"
            )
        };

        // Update issuer information
        sampleInvoices[0].UpdateIssuerInfo("Musterfirma GmbH", "Musterstraße 1", "12345", "Musterstadt", "Deutschland");
        sampleInvoices[1].UpdateIssuerInfo("Beispiel AG", "Beispielweg 2", "54321", "Beispielstadt", "Deutschland");
        sampleInvoices[2].UpdateIssuerInfo("Test Unternehmen", "Teststraße 3", "98765", "Teststadt", "Deutschland");

        _context.Invoices.AddRange(sampleInvoices);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sample invoices", sampleInvoices.Count);
    }

    private async Task SeedSampleRawBlocksAsync()
    {
        var invoices = await _context.Invoices.ToListAsync();
        var rawBlocks = new List<InvoiceRawBlock>();

        foreach (var invoice in invoices)
        {
            // Create sample raw blocks for each invoice
            var sampleBlocks = new List<InvoiceRawBlock>
            {
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Rechnungs-Nr.: {invoice.InvoiceNumber}",
                    100f, 50f, 200f, 20f,
                    0,
                    "InvoiceNumber",
                    0.95f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Datum: {invoice.InvoiceDate:dd.MM.yyyy}",
                    100f, 80f, 150f, 20f,
                    1,
                    "InvoiceDate",
                    0.90f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    invoice.IssuerName,
                    100f, 110f, 300f, 20f,
                    2,
                    "IssuerAddress",
                    0.85f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Netto: {invoice.NetTotal:C}",
                    100f, 140f, 150f, 20f,
                    3,
                    "NetTotal",
                    0.88f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"MwSt: {invoice.VatTotal:C}",
                    100f, 170f, 150f, 20f,
                    4,
                    "VatTotal",
                    0.87f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Gesamt: {invoice.GrossTotal:C}",
                    100f, 200f, 150f, 20f,
                    5,
                    "GrossTotal",
                    0.93f
                )
            };

            rawBlocks.AddRange(sampleBlocks);
        }

        _context.InvoiceRawBlocks.AddRange(rawBlocks);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sample raw blocks", rawBlocks.Count);
    }

    private async Task<DateTime?> GetLastSeededDateAsync()
    {
        try
        {
            return await _context.Invoices.MinAsync(i => i.ImportedAt);
        }
        catch
        {
            return null;
        }
    }
}
```

## 3. First-Run Service

**Datei:** `src/Invoice.Infrastructure/Data/Services/FirstRunService.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Services;

public interface IFirstRunService
{
    Task<bool> InitializeAsync();
    Task<bool> IsFirstRunAsync();
    Task<FirstRunResult> GetFirstRunStatusAsync();
}

public class FirstRunResult
{
    public bool IsFirstRun { get; set; }
    public bool DatabaseExists { get; set; }
    public bool MigrationsApplied { get; set; }
    public bool DataSeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class FirstRunService : IFirstRunService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FirstRunService> _logger;

    public FirstRunService(IServiceProvider serviceProvider, ILogger<FirstRunService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting first-run initialization...");

            using var scope = _serviceProvider.CreateScope();

            // Check if this is a first run
            if (!await IsFirstRunAsync())
            {
                _logger.LogInformation("Not a first run, skipping initialization");
                return true;
            }

            // Initialize database
            var migrationStartupService = scope.ServiceProvider.GetRequiredService<IMigrationStartupService>();
            if (!await migrationStartupService.InitializeDatabaseAsync())
            {
                _logger.LogError("Failed to initialize database");
                return false;
            }

            // Seed database
            var databaseSeeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            if (!await databaseSeeder.SeedAsync())
            {
                _logger.LogError("Failed to seed database");
                return false;
            }

            _logger.LogInformation("First-run initialization completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "First-run initialization failed");
            return false;
        }
    }

    public async Task<bool> IsFirstRunAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();

            // Check if database exists and has data
            var canConnect = await context.CanConnectAsync();
            if (!canConnect) return true;

            var hasData = await context.Invoices.AnyAsync();
            return !hasData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if first run");
            return true; // Assume first run if we can't determine
        }
    }

    public async Task<FirstRunResult> GetFirstRunStatusAsync()
    {
        var result = new FirstRunResult();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
            var databaseSeeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();

            // Check database connection
            result.DatabaseExists = await context.CanConnectAsync();

            if (result.DatabaseExists)
            {
                // Check if data exists
                result.DataSeeded = await databaseSeeder.IsSeededAsync();

                // Check for pending migrations
                var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
                result.MigrationsApplied = !await migrationService.HasPendingMigrationsAsync();
            }

            result.IsFirstRun = !result.DatabaseExists || !result.DataSeeded;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to get first run status: {ex.Message}");
        }

        return result;
    }
}
```

## 4. First-Run Extensions

**Datei:** `src/Invoice.Infrastructure/Data/Extensions/FirstRunExtensions.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Data.Seeders;
using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Data.Extensions;

public static class FirstRunExtensions
{
    public static IServiceCollection AddFirstRunServices(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IFirstRunService, FirstRunService>();

        return services;
    }
}
```

## 5. Program.cs Integration

**Datei:** `src/Invoice.WinForms/Program.cs` (Erweiterung)

```csharp
using Invoice.Application.Extensions;
using Invoice.Infrastructure.Data.Extensions;
using Invoice.Infrastructure.Data.Services;
using Invoice.Infrastructure.Extensions;
using Invoice.WinForms.Extensions;
using Invoice.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Invoice.WinForms;

static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        // Initialize database and first-run setup
        var firstRunService = host.Services.GetRequiredService<IFirstRunService>();
        var firstRunSuccess = await firstRunService.InitializeAsync();

        if (!firstRunSuccess)
        {
            MessageBox.Show("Failed to initialize database. Application will exit.", "Database Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Logging für Application Start
        var logger = host.Services.GetRequiredService<ILoggingService>();
        logger.LogInformation("Invoice Reader Application started");

        Application.Run(host.Services.GetRequiredService<MainForm>());
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Infrastructure Layer
                services.AddInfrastructure(context.Configuration);
                services.AddMigrationServices();
                services.AddFirstRunServices(); // First-Run Services

                // Application Layer
                services.AddApplicationServices();

                // WinForms Layer
                services.AddWinFormsServices();
            });
    }
}
```

## Wichtige Hinweise

- Database Seeder für Initial Data
- First-Run Detection für neue Installationen
- Sample Data für Testing und Demonstration
- Error Handling für alle Seeding-Operationen
- Status-Checks für Database-Initialisierung
- Service Registration für DI-Container
- Integration in Program.cs für automatische Initialisierung
- Logging für alle Seeding-Operationen
- Sample Raw Blocks für ML-Training
- Configurable Seeding für verschiedene Umgebungen
