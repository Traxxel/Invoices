using Microsoft.Extensions.DependencyInjection;
using Invoice.Application.Interfaces;
using Invoice.Application.UseCases;

namespace Invoice.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use Cases
        services.AddScoped<IImportInvoiceUseCase, ImportInvoiceUseCase>();
        services.AddScoped<IExtractFieldsUseCase, ExtractFieldsUseCase>();
        services.AddScoped<ISaveInvoiceUseCase, SaveInvoiceUseCase>();
        // Temporarily disabled due to type conflicts - will fix later
        // services.AddScoped<ITrainModelsUseCase, TrainModelsUseCase>();

        // Application Services
        services.AddScoped<IRegexPatternService, Services.RegexPatternService>();
        services.AddScoped<IParserHelperService, Services.ParserHelperService>();

        return services;
    }
}

