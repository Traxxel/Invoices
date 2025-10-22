using Microsoft.Extensions.DependencyInjection;

namespace Invoice.WinForms.Extensions;

public static class WinFormsServiceExtensions
{
    public static IServiceCollection AddWinFormsServices(this IServiceCollection services)
    {
        // Forms
        services.AddTransient<frmMain>();
        
        // Weitere Forms werden in späteren Aufgaben implementiert
        // services.AddTransient<ImportDialog>();
        // services.AddTransient<BatchImportDialog>();
        // services.AddTransient<ReviewForm>();
        // services.AddTransient<TrainingForm>();
        // services.AddTransient<SettingsForm>();

        return services;
    }
}

