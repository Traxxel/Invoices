using Invoice.Application.Interfaces;
using Invoice.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.ML.Extensions;

public static class ModelTrainingExtensions
{
    public static IServiceCollection AddModelTrainingServices(this IServiceCollection services)
    {
        services.AddScoped<IModelTrainingService, ModelTrainingService>();

        return services;
    }
}

