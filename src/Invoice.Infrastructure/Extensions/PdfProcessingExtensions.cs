using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class PdfProcessingExtensions
{
    public static IServiceCollection AddPdfProcessing(this IServiceCollection services)
    {
        // PDF Processing Services werden in späteren Konzepten (19-22) implementiert
        // Platzhalter für:
        // - IPdfParserService, PdfPigParserService
        // - ITextNormalizationService, TextNormalizationService
        // - IFeatureExtractionService, FeatureExtractionService
        
        // services.AddScoped<IPdfParserService, PdfPigParserService>();
        // services.AddScoped<ITextNormalizationService, TextNormalizationService>();
        // services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        return services;
    }
}

