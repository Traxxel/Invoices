using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Data.Seeders;
using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Data.Extensions;

public static class FirstRunExtensions
{
    public static IServiceCollection AddFirstRunServices(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IFirstRunService, FirstRunService>();

        return services;
    }
}

