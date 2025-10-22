using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Services;

public interface IMigrationService
{
    Task<bool> MigrateAsync();
    Task<bool> HasPendingMigrationsAsync();
    Task<List<string>> GetPendingMigrationsAsync();
    Task<List<string>> GetAppliedMigrationsAsync();
    Task<bool> CanConnectAsync();
    Task<bool> EnsureCreatedAsync();
    Task<bool> EnsureDeletedAsync();
}

public class MigrationService : IMigrationService
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(InvoiceDbContext context, ILogger<MigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");

            await _context.Database.MigrateAsync();

            _logger.LogInformation("Database migration completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            return false;
        }
    }

    public async Task<bool> HasPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for pending migrations");
            return false;
        }
    }

    public async Task<List<string>> GetPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending migrations");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetAppliedMigrationsAsync()
    {
        try
        {
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            return appliedMigrations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get applied migrations");
            return new List<string>();
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database");
            return false;
        }
    }

    public async Task<bool> EnsureCreatedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is created...");

            await _context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Database creation completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database creation failed");
            return false;
        }
    }

    public async Task<bool> EnsureDeletedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is deleted...");

            await _context.Database.EnsureDeletedAsync();

            _logger.LogInformation("Database deletion completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database deletion failed");
            return false;
        }
    }
}

