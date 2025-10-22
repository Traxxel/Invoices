using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddConfiguration(configuration);

        // Logging
        services.AddLoggingServices();

        // Database
        services.AddDatabase(configuration);

        // File Storage
        services.AddFileStorage(configuration);

        // PDF Processing
        services.AddPdfProcessing();

        // ML Services
        services.AddMLServices(configuration);

        return services;
    }
}

