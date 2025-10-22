using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Services;

public interface IMigrationStartupService
{
    Task<bool> InitializeDatabaseAsync();
}

public class MigrationStartupService : IMigrationStartupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationStartupService> _logger;

    public MigrationStartupService(IServiceProvider serviceProvider, ILogger<MigrationStartupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");

            using var scope = _serviceProvider.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

            // Check if database can connect
            if (!await migrationService.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to database");
                return false;
            }

            // Check for pending migrations
            var hasPendingMigrations = await migrationService.HasPendingMigrationsAsync();
            if (hasPendingMigrations)
            {
                _logger.LogInformation("Found pending migrations, applying...");

                var pendingMigrations = await migrationService.GetPendingMigrationsAsync();
                _logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));

                // Apply migrations
                if (!await migrationService.MigrateAsync())
                {
                    _logger.LogError("Failed to apply migrations");
                    return false;
                }

                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }

            // Verify database is ready
            var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
            var canConnect = await context.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("Database is not accessible after migration");
                return false;
            }

            _logger.LogInformation("Database initialization completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            return false;
        }
    }
}

