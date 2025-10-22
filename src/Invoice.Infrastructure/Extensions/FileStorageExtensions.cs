using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;

namespace Invoice.Infrastructure.Extensions;

public static class FileStorageExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        // Add IFileSystem for testability
        services.AddSingleton<IFileSystem, FileSystem>();
        
        // Add File Storage Services
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFileHashService, FileHashService>();
        services.AddScoped<IRetentionPolicyService, RetentionPolicyService>();

        return services;
    }
}

