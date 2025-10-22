using Invoice.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Invoice.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton(Log.Logger);
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<DomainEventLogger>();
        services.AddSingleton<PerformanceLogger>();

        return services;
    }
}

