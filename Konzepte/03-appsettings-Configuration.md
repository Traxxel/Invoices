# Aufgabe 03: appsettings.json Struktur und Configuration-Setup

## Ziel
Konfigurationsstruktur für die gesamte Anwendung definieren und Configuration-Setup implementieren.

## 1. appsettings.json Struktur

**Datei:** `src/InvoiceReader.WinForms/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=InvoiceReader;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "StoragePath": "storage",
    "ModelsPath": "data/models",
    "LabeledDataPath": "data/labeled",
    "SamplesPath": "data/samples",
    "Culture": "de-DE",
    "Currency": "EUR",
    "MaxFileSizeMB": 50,
    "SupportedFileExtensions": [".pdf"],
    "RetentionYears": 10
  },
  "MLSettings": {
    "ModelVersion": "v1.0",
    "ConfidenceThreshold": 0.7,
    "TrainingDataSplit": {
      "TrainPercentage": 0.8,
      "ValidationPercentage": 0.1,
      "TestPercentage": 0.1
    },
    "Features": {
      "UseRegexFeatures": true,
      "UsePositionFeatures": true,
      "UseContextFeatures": true
    }
  },
  "FileStorage": {
    "BasePath": "storage",
    "InvoiceSubPath": "invoices",
    "UseYearMonthStructure": true,
    "GenerateUniqueFilenames": true,
    "CalculateFileHash": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    },
    "Serilog": {
      "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
      "MinimumLevel": "Information",
      "WriteTo": [
        {
          "Name": "Console",
          "Args": {
            "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
          }
        },
        {
          "Name": "File",
          "Args": {
            "path": "logs/invoice-reader-.log",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30,
            "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
          }
        }
      ]
    }
  }
}
```

## 2. appsettings.Development.json

**Datei:** `src/InvoiceReader.WinForms/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    },
    "Serilog": {
      "MinimumLevel": "Debug"
    }
  },
  "AppSettings": {
    "StoragePath": "storage_dev",
    "ModelsPath": "data/models_dev"
  }
}
```

## 3. Configuration Models

**Datei:** `src/InvoiceReader.Infrastructure/Configuration/AppSettings.cs`

```csharp
namespace InvoiceReader.Infrastructure.Configuration;

public class AppSettings
{
    public string StoragePath { get; set; } = "storage";
    public string ModelsPath { get; set; } = "data/models";
    public string LabeledDataPath { get; set; } = "data/labeled";
    public string SamplesPath { get; set; } = "data/samples";
    public string Culture { get; set; } = "de-DE";
    public string Currency { get; set; } = "EUR";
    public int MaxFileSizeMB { get; set; } = 50;
    public string[] SupportedFileExtensions { get; set; } = { ".pdf" };
    public int RetentionYears { get; set; } = 10;
}
```

**Datei:** `src/InvoiceReader.Infrastructure/Configuration/MLSettings.cs`

```csharp
namespace InvoiceReader.Infrastructure.Configuration;

public class MLSettings
{
    public string ModelVersion { get; set; } = "v1.0";
    public float ConfidenceThreshold { get; set; } = 0.7f;
    public TrainingDataSplit TrainingDataSplit { get; set; } = new();
    public FeatureSettings Features { get; set; } = new();
}

public class TrainingDataSplit
{
    public float TrainPercentage { get; set; } = 0.8f;
    public float ValidationPercentage { get; set; } = 0.1f;
    public float TestPercentage { get; set; } = 0.1f;
}

public class FeatureSettings
{
    public bool UseRegexFeatures { get; set; } = true;
    public bool UsePositionFeatures { get; set; } = true;
    public bool UseContextFeatures { get; set; } = true;
}
```

**Datei:** `src/InvoiceReader.Infrastructure/Configuration/FileStorageSettings.cs`

```csharp
namespace InvoiceReader.Infrastructure.Configuration;

public class FileStorageSettings
{
    public string BasePath { get; set; } = "storage";
    public string InvoiceSubPath { get; set; } = "invoices";
    public bool UseYearMonthStructure { get; set; } = true;
    public bool GenerateUniqueFilenames { get; set; } = true;
    public bool CalculateFileHash { get; set; } = true;
}
```

## 4. Configuration Extension

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/ConfigurationExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<MLSettings>(configuration.GetSection("MLSettings"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        
        return services;
    }
}
```

## 5. Configuration in Program.cs

**Datei:** `src/InvoiceReader.WinForms/Program.cs`

```csharp
using InvoiceReader.Infrastructure.Extensions;
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
        
        Application.Run(host.Services.GetRequiredService<MainForm>());
    }
    
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddConfiguration(context.Configuration);
                // Weitere Service-Registrierungen folgen in späteren Aufgaben
            });
    }
}
```

## Wichtige Hinweise
- Alle Pfade relativ zum Application-Verzeichnis
- Development-spezifische Overrides
- Strukturierte Configuration Models
- Extension Methods für saubere DI-Registrierung
- Logging-Konfiguration für Serilog
- Culture-spezifische Einstellungen für de-DE

