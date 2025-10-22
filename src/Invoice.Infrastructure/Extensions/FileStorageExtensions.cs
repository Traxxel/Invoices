using Invoice.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class FileStorageExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

        // File Storage Services werden in späteren Aufgaben implementiert
        // services.AddScoped<IFileStorageService, FileStorageService>();
        // services.AddScoped<IFileHashService, FileHashService>();

        return services;
    }
}

