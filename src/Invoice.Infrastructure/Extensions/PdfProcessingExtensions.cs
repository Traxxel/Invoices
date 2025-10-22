using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class PdfProcessingExtensions
{
    public static IServiceCollection AddPdfProcessing(this IServiceCollection services)
    {
        // PDF Parser Service
        services.AddScoped<IPdfParserService, PdfPigParserService>();
        
        // Text Normalization Service
        services.AddScoped<ITextNormalizationService, TextNormalizationService>();
        
        // Feature Extraction Service
        services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        return services;
    }
}

