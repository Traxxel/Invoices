# Aufgabe 05: Dependency Injection Container konfigurieren

## Ziel

Vollständige DI-Container-Konfiguration für alle Schichten der Anwendung.

## 1. Service Registration Extensions

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Infrastructure.Configuration;
using InvoiceReader.Infrastructure.Data;
using InvoiceReader.Infrastructure.Logging;
using InvoiceReader.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddConfiguration(configuration);

        // Logging
        services.AddLoggingServices();

        // Database
        services.AddDatabase(configuration);

        // File Storage
        services.AddFileStorage(configuration);

        // PDF Processing
        services.AddPdfProcessing();

        // ML Services
        services.AddMLServices(configuration);

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use Cases werden in späteren Aufgaben registriert
        return services;
    }
}
```

## 2. Database Services

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/DatabaseExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Data;
using InvoiceReader.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InvoiceDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors();
        });

        // Repository Pattern
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceRawBlockRepository, InvoiceRawBlockRepository>();

        return services;
    }
}
```

## 3. File Storage Services

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/FileStorageExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class FileStorageExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFileHashService, FileHashService>();

        return services;
    }
}
```

## 4. PDF Processing Services

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/PdfProcessingExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class PdfProcessingExtensions
{
    public static IServiceCollection AddPdfProcessing(this IServiceCollection services)
    {
        services.AddScoped<IPdfParserService, PdfPigParserService>();
        services.AddScoped<ITextNormalizationService, TextNormalizationService>();
        services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        return services;
    }
}
```

## 5. ML Services

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/MLServicesExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.ML;
using InvoiceReader.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class MLServicesExtensions
{
    public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MLSettings>(configuration.GetSection("MLSettings"));

        services.AddScoped<IMLModelService, MLModelService>();
        services.AddScoped<IFieldExtractionService, MLFieldExtractionService>();
        services.AddScoped<IModelTrainingService, ModelTrainingService>();

        return services;
    }
}
```

## 6. Application Services

**Datei:** `src/InvoiceReader.Application/Extensions/ApplicationServiceExtensions.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use Cases
        services.AddScoped<IImportInvoiceUseCase, ImportInvoiceUseCase>();
        services.AddScoped<IExtractFieldsUseCase, ExtractFieldsUseCase>();
        services.AddScoped<ISaveInvoiceUseCase, SaveInvoiceUseCase>();
        services.AddScoped<ITrainModelsUseCase, TrainModelsUseCase>();

        return services;
    }
}
```

## 7. WinForms Services

**Datei:** `src/InvoiceReader.WinForms/Extensions/WinFormsServiceExtensions.cs`

```csharp
using InvoiceReader.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.WinForms.Extensions;

public static class WinFormsServiceExtensions
{
    public static IServiceCollection AddWinFormsServices(this IServiceCollection services)
    {
        // Forms
        services.AddTransient<MainForm>();
        services.AddTransient<ImportDialog>();
        services.AddTransient<BatchImportDialog>();
        services.AddTransient<ReviewForm>();
        services.AddTransient<TrainingForm>();
        services.AddTransient<SettingsForm>();

        return services;
    }
}
```

## 8. Vollständige Program.cs

**Datei:** `src/InvoiceReader.WinForms/Program.cs`

```csharp
using InvoiceReader.Application.Extensions;
using InvoiceReader.Infrastructure.Extensions;
using InvoiceReader.WinForms.Extensions;
using InvoiceReader.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvoiceReader.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        // Logging für Application Start
        var logger = host.Services.GetRequiredService<ILoggingService>();
        logger.LogInformation("Invoice Reader Application started");

        Application.Run(host.Services.GetRequiredService<MainForm>());
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Infrastructure Layer
                services.AddInfrastructure(context.Configuration);

                // Application Layer
                services.AddApplicationServices();

                // WinForms Layer
                services.AddWinFormsServices();
            });
    }
}
```

## 9. Service Lifetime Guidelines

**Datei:** `src/InvoiceReader.Infrastructure/Documentation/ServiceLifetimes.md`

```markdown
# Service Lifetime Guidelines

## Singleton

- Configuration Services
- Logging Services
- ML Models (nach dem Laden)
- Performance Loggers

## Scoped

- DbContext
- Repository Implementations
- Use Case Implementations
- File Storage Services
- PDF Processing Services

## Transient

- WinForms (jede Form-Instanz neu)
- DTOs und Value Objects
- Helper Services
```

## Wichtige Hinweise

- Saubere Trennung der Service-Registrierung nach Schichten
- Scoped Lifetime für DbContext und Repository Pattern
- Transient für WinForms (jede Form-Instanz neu)
- Singleton für Configuration und Logging
- Extension Methods für saubere Organisation
- Vollständige DI-Container-Konfiguration
- Service Lifetime Guidelines dokumentiert
