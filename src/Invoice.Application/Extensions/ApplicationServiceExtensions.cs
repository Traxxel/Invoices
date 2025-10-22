using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use Cases werden in späteren Aufgaben implementiert
        // services.AddScoped<IImportInvoiceUseCase, ImportInvoiceUseCase>();
        // services.AddScoped<IExtractFieldsUseCase, ExtractFieldsUseCase>();
        // services.AddScoped<ISaveInvoiceUseCase, SaveInvoiceUseCase>();
        // services.AddScoped<ITrainModelsUseCase, TrainModelsUseCase>();

        return services;
    }
}

