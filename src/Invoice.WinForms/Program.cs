using Invoice.Application.Extensions;
using Invoice.Infrastructure.Extensions;
using Invoice.Infrastructure.Logging;
using Invoice.WinForms.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Invoice.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        // Logging für Application Start
        var logger = host.Services.GetRequiredService<ILoggingService>();
        logger.LogInformation("Invoice Reader Application started");

        try
        {
            Application.Run(host.Services.GetRequiredService<frmMain>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            logger.LogInformation("Invoice Reader Application stopped");
            Log.CloseAndFlush();
        }
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

