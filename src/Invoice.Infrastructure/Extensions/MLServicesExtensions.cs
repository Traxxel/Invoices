using Invoice.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class MLServicesExtensions
{
    public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MLSettings>(configuration.GetSection("MLSettings"));

        // ML Services werden in späteren Aufgaben implementiert
        // services.AddScoped<IMLModelService, MLModelService>();
        // services.AddScoped<IFieldExtractionService, MLFieldExtractionService>();
        // services.AddScoped<IModelTrainingService, ModelTrainingService>();

        return services;
    }
}

