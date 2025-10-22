using Invoice.Application.Interfaces;
using Invoice.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.ML.Extensions;

public static class PredictionEngineExtensions
{
    public static IServiceCollection AddPredictionEngineServices(this IServiceCollection services)
    {
        services.AddScoped<IPredictionEngineService, PredictionEngineService>();

        return services;
    }
}

