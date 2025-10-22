# Aufgabe 04: Logging-Infrastructure (Serilog)

## Ziel

Serilog-basierte Logging-Infrastructure für die gesamte Anwendung einrichten.

## 1. Serilog Configuration

**Datei:** `src/InvoiceReader.Infrastructure/Logging/SerilogExtensions.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace InvoiceReader.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder UseSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/invoice-reader-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        });
    }
}
```

## 2. Logging Service Interface

**Datei:** `src/InvoiceReader.Infrastructure/Logging/ILoggingService.cs`

```csharp
namespace InvoiceReader.Infrastructure.Logging;

public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);

    // Spezifische Logging-Methoden für Invoice-Reader
    void LogInvoiceImport(string invoiceNumber, string filePath, bool success);
    void LogMLPrediction(string modelVersion, float confidence, string fieldType);
    void LogFileOperation(string operation, string filePath, bool success);
    void LogDatabaseOperation(string operation, int recordCount, bool success);
}
```

## 3. Logging Service Implementation

**Datei:** `src/InvoiceReader.Infrastructure/Logging/LoggingService.cs`

```csharp
using Serilog;

namespace InvoiceReader.Infrastructure.Logging;

public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;

    public LoggingService(ILogger logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.Error(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.Error(exception, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }

    public void LogInvoiceImport(string invoiceNumber, string filePath, bool success)
    {
        if (success)
        {
            _logger.Information("Invoice imported successfully: {InvoiceNumber} from {FilePath}",
                invoiceNumber, filePath);
        }
        else
        {
            _logger.Warning("Invoice import failed: {InvoiceNumber} from {FilePath}",
                invoiceNumber, filePath);
        }
    }

    public void LogMLPrediction(string modelVersion, float confidence, string fieldType)
    {
        _logger.Debug("ML Prediction: Model {ModelVersion}, Field {FieldType}, Confidence {Confidence}",
            modelVersion, fieldType, confidence);
    }

    public void LogFileOperation(string operation, string filePath, bool success)
    {
        if (success)
        {
            _logger.Debug("File operation {Operation} successful: {FilePath}", operation, filePath);
        }
        else
        {
            _logger.Warning("File operation {Operation} failed: {FilePath}", operation, filePath);
        }
    }

    public void LogDatabaseOperation(string operation, int recordCount, bool success)
    {
        if (success)
        {
            _logger.Debug("Database operation {Operation} successful: {RecordCount} records",
                operation, recordCount);
        }
        else
        {
            _logger.Warning("Database operation {Operation} failed: {RecordCount} records",
                operation, recordCount);
        }
    }
}
```

## 4. Structured Logging für Domain Events

**Datei:** `src/InvoiceReader.Infrastructure/Logging/DomainEventLogger.cs`

```csharp
using Serilog;

namespace InvoiceReader.Infrastructure.Logging;

public class DomainEventLogger
{
    private readonly ILogger _logger;

    public DomainEventLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogInvoiceCreated(Guid invoiceId, string invoiceNumber, DateTime createdDate)
    {
        _logger.Information("Invoice created: {InvoiceId}, Number: {InvoiceNumber}, Date: {CreatedDate}",
            invoiceId, invoiceNumber, createdDate);
    }

    public void LogInvoiceUpdated(Guid invoiceId, string invoiceNumber, string[] changedFields)
    {
        _logger.Information("Invoice updated: {InvoiceId}, Number: {InvoiceNumber}, Changed: {ChangedFields}",
            invoiceId, invoiceNumber, string.Join(", ", changedFields));
    }

    public void LogMLModelTrained(string modelVersion, int trainingSamples, float accuracy)
    {
        _logger.Information("ML Model trained: Version {ModelVersion}, Samples {TrainingSamples}, Accuracy {Accuracy}",
            modelVersion, trainingSamples, accuracy);
    }

    public void LogFileStored(Guid invoiceId, string filePath, long fileSize, string fileHash)
    {
        _logger.Debug("File stored: Invoice {InvoiceId}, Path {FilePath}, Size {FileSize}, Hash {FileHash}",
            invoiceId, filePath, fileSize, fileHash);
    }
}
```

## 5. Performance Logging

**Datei:** `src/InvoiceReader.Infrastructure/Logging/PerformanceLogger.cs`

```csharp
using System.Diagnostics;
using Serilog;

namespace InvoiceReader.Infrastructure.Logging;

public class PerformanceLogger
{
    private readonly ILogger _logger;

    public PerformanceLogger(ILogger logger)
    {
        _logger = logger;
    }

    public IDisposable LogOperation(string operationName)
    {
        return new PerformanceScope(_logger, operationName);
    }

    private class PerformanceScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public PerformanceScope(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();

            _logger.Debug("Starting operation: {OperationName}", _operationName);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Debug("Completed operation: {OperationName} in {ElapsedMs}ms",
                _operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 6. Service Registration

**Datei:** `src/InvoiceReader.Infrastructure/Extensions/LoggingExtensions.cs`

```csharp
using InvoiceReader.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace InvoiceReader.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<DomainEventLogger>();
        services.AddSingleton<PerformanceLogger>();

        return services;
    }
}
```

## 7. Integration in Program.cs

**Datei:** `src/InvoiceReader.WinForms/Program.cs` (Erweiterung)

```csharp
using InvoiceReader.Infrastructure.Extensions;
using InvoiceReader.Infrastructure.Logging;
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
            .UseSerilog() // Serilog Integration
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
                services.AddLoggingServices(); // Logging Services registrieren
                // Weitere Service-Registrierungen folgen
            });
    }
}
```

## Wichtige Hinweise

- Strukturiertes Logging mit Serilog
- Spezifische Logging-Methoden für Invoice-Reader Use Cases
- Performance-Logging für kritische Operationen
- Domain Event Logging für Audit-Trail
- Konfigurierbar über appsettings.json
- Console + File Output
- Rolling File Logs (täglich, 30 Tage Retention)
