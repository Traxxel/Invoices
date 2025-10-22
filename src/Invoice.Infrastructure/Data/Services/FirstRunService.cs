using Invoice.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

