using Invoice.Application.Interfaces;
using Invoice.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;

namespace Invoice.Infrastructure.ML.Extensions;

public static class MLServiceExtensions
{
    public static IServiceCollection AddMLServices(this IServiceCollection services)
    {
        // Register MLContext as singleton
        services.AddSingleton(sp => new MLContext(seed: 42));

        // Register ML Pipeline Service
        services.AddScoped<IMLPipelineService, MLPipelineService>();

        // Register Model Training Service
        services.AddScoped<IModelTrainingService, ModelTrainingService>();

        // Register Prediction Engine Service
        services.AddScoped<IPredictionEngineService, PredictionEngineService>();

        return services;
    }
}

