using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class PdfProcessingExtensions
{
    public static IServiceCollection AddPdfProcessing(this IServiceCollection services)
    {
        // PDF Processing Services werden in späteren Aufgaben implementiert
        // services.AddScoped<IPdfParserService, PdfPigParserService>();
        // services.AddScoped<ITextNormalizationService, TextNormalizationService>();
        // services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        return services;
    }
}

