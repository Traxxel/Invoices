using Invoice.Application.Extensions;
using Invoice.Infrastructure.Data.Extensions;
using Invoice.Infrastructure.Data.Services;
using Invoice.Infrastructure.Extensions;
using Invoice.Infrastructure.Logging;
using Invoice.WinForms.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using WinFormsApp = System.Windows.Forms.Application;

namespace Invoice.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        // Initialize database and first-run setup
        var firstRunService = host.Services.GetRequiredService<IFirstRunService>();
        var firstRunSuccess = await firstRunService.InitializeAsync();

        if (!firstRunSuccess)
        {
            MessageBox.Show("Failed to initialize database. Application will exit.", "Database Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Logging für Application Start
        var logger = host.Services.GetRequiredService<ILoggingService>();
        logger.LogInformation("Invoice Reader Application started");

        try
        {
            WinFormsApp.Run(host.Services.GetRequiredService<frmMain>());
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
                services.AddMigrationServices();
                services.AddFirstRunServices();

                // Application Layer
                services.AddApplicationServices();

                // WinForms Layer
                services.AddWinFormsServices();
            });
    }
}

