using Invoice.Application.Interfaces;
using Invoice.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.ML.Extensions;

public static class MLPipelineExtensions
{
    public static IServiceCollection AddMLPipelineServices(this IServiceCollection services)
    {
        services.AddScoped<IMLPipelineService, MLPipelineService>();

        return services;
    }
}

