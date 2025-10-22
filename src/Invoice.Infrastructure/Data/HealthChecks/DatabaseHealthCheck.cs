using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Invoice.Infrastructure.Data.HealthChecks;

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

