using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Data.Extensions;

public static class MigrationExtensions
{
    public static IServiceCollection AddMigrationServices(this IServiceCollection services)
    {
        services.AddScoped<IMigrationService, MigrationService>();
        services.AddScoped<IMigrationStartupService, MigrationStartupService>();

        return services;
    }
}

