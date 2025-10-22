using Invoice.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<MLSettings>(configuration.GetSection("MLSettings"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        
        return services;
    }
}

