using Invoice.Infrastructure.Data;
using Invoice.Infrastructure.Data.Extensions;
using Invoice.Infrastructure.Data.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Add DbContext
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

        // Add repositories
        services.AddRepositories();

        // Add migration services
        services.AddMigrationServices();

        // Add first-run services
        services.AddFirstRunServices();

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        return services;
    }
}

