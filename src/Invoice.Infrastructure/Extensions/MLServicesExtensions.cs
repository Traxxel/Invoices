using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class MLServicesExtensions
{
    public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ML Services werden in späteren Konzepten (23-25) implementiert
        // Platzhalter für:
        // - IMLModelService, MLModelService
        // - IFieldExtractionService, MLFieldExtractionService
        // - IModelTrainingService, ModelTrainingService
        
        // MLSettings wird bereits in ConfigurationExtensions registriert
        
        // services.AddScoped<IMLModelService, MLModelService>();
        // services.AddScoped<IFieldExtractionService, MLFieldExtractionService>();
        // services.AddScoped<IModelTrainingService, ModelTrainingService>();

        return services;
    }
}

