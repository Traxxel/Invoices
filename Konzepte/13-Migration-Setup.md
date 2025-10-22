# Aufgabe 13: Migration-Setup und Initial Migration

## Ziel

EF Core Migration-Setup mit Initial Migration und automatischer Migration beim App-Start.

## 1. Initial Migration

**Datei:** `src/Invoice.Infrastructure/Data/Migrations/InitialCreate.cs`

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IssuerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IssuerStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssuerPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IssuerCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuerCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExtractionConfidence = table.Column<float>(type: "real", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceRawBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    X = table.Column<float>(type: "real", nullable: false),
                    Y = table.Column<float>(type: "real", nullable: false),
                    Width = table.Column<float>(type: "real", nullable: false),
                    Height = table.Column<float>(type: "real", nullable: false),
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    PredictedLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PredictionConfidence = table.Column<float>(type: "real", nullable: true),
                    ActualLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRawBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceRawBlocks_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber_Unique",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssuerName",
                table: "Invoices",
                column: "IssuerName");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ImportedAt",
                table: "Invoices",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ExtractionConfidence",
                table: "Invoices",
                column: "ExtractionConfidence");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ModelVersion",
                table: "Invoices",
                column: "ModelVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date_Issuer",
                table: "Invoices",
                columns: new[] { "InvoiceDate", "IssuerName" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Amount_Date",
                table: "Invoices",
                columns: new[] { "GrossTotal", "InvoiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_InvoiceId",
                table: "InvoiceRawBlocks",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Page",
                table: "InvoiceRawBlocks",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_PredictedLabel",
                table: "InvoiceRawBlocks",
                column: "PredictedLabel");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_ActualLabel",
                table: "InvoiceRawBlocks",
                column: "ActualLabel");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_PredictionConfidence",
                table: "InvoiceRawBlocks",
                column: "PredictionConfidence");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_CreatedAt",
                table: "InvoiceRawBlocks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Invoice_Page",
                table: "InvoiceRawBlocks",
                columns: new[] { "InvoiceId", "Page" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Invoice_LineIndex",
                table: "InvoiceRawBlocks",
                columns: new[] { "InvoiceId", "LineIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Label_Confidence",
                table: "InvoiceRawBlocks",
                columns: new[] { "PredictedLabel", "PredictionConfidence" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Actual_Predicted",
                table: "InvoiceRawBlocks",
                columns: new[] { "ActualLabel", "PredictedLabel" });

            // Create check constraints
            migrationBuilder.Sql(@"
                ALTER TABLE [Invoices] ADD CONSTRAINT [CK_Invoices_NetTotal_Positive] CHECK (NetTotal >= 0);
                ALTER TABLE [Invoices] ADD CONSTRAINT [CK_Invoices_VatTotal_Positive] CHECK (VatTotal >= 0);
                ALTER TABLE [Invoices] ADD CONSTRAINT [CK_Invoices_GrossTotal_Positive] CHECK (GrossTotal >= 0);
                ALTER TABLE [Invoices] ADD CONSTRAINT [CK_Invoices_Confidence_Range] CHECK (ExtractionConfidence >= 0.0 AND ExtractionConfidence <= 1.0);
                ALTER TABLE [Invoices] ADD CONSTRAINT [CK_Invoices_Amount_Consistency] CHECK (ABS((NetTotal + VatTotal) - GrossTotal) <= 0.02);

                ALTER TABLE [InvoiceRawBlocks] ADD CONSTRAINT [CK_InvoiceRawBlocks_Page_Positive] CHECK (Page > 0);
                ALTER TABLE [InvoiceRawBlocks] ADD CONSTRAINT [CK_InvoiceRawBlocks_Width_Positive] CHECK (Width > 0);
                ALTER TABLE [InvoiceRawBlocks] ADD CONSTRAINT [CK_InvoiceRawBlocks_Height_Positive] CHECK (Height > 0);
                ALTER TABLE [InvoiceRawBlocks] ADD CONSTRAINT [CK_InvoiceRawBlocks_LineIndex_Positive] CHECK (LineIndex >= 0);
                ALTER TABLE [InvoiceRawBlocks] ADD CONSTRAINT [CK_InvoiceRawBlocks_Confidence_Range] CHECK (PredictionConfidence IS NULL OR (PredictionConfidence >= 0.0 AND PredictionConfidence <= 1.0));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceRawBlocks");

            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
```

## 2. Migration Service

**Datei:** `src/Invoice.Infrastructure/Data/Services/MigrationService.cs`

```csharp
using Invoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Services;

public interface IMigrationService
{
    Task<bool> MigrateAsync();
    Task<bool> HasPendingMigrationsAsync();
    Task<List<string>> GetPendingMigrationsAsync();
    Task<List<string>> GetAppliedMigrationsAsync();
    Task<bool> CanConnectAsync();
    Task<bool> EnsureCreatedAsync();
    Task<bool> EnsureDeletedAsync();
}

public class MigrationService : IMigrationService
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(InvoiceDbContext context, ILogger<MigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");

            await _context.Database.MigrateAsync();

            _logger.LogInformation("Database migration completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            return false;
        }
    }

    public async Task<bool> HasPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for pending migrations");
            return false;
        }
    }

    public async Task<List<string>> GetPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending migrations");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetAppliedMigrationsAsync()
    {
        try
        {
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            return appliedMigrations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get applied migrations");
            return new List<string>();
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database");
            return false;
        }
    }

    public async Task<bool> EnsureCreatedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is created...");

            await _context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Database creation completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database creation failed");
            return false;
        }
    }

    public async Task<bool> EnsureDeletedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is deleted...");

            await _context.Database.EnsureDeletedAsync();

            _logger.LogInformation("Database deletion completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database deletion failed");
            return false;
        }
    }
}
```

## 3. Migration Startup

**Datei:** `src/Invoice.Infrastructure/Data/Services/MigrationStartupService.cs`

```csharp
using Invoice.Infrastructure.Data;
using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Services;

public interface IMigrationStartupService
{
    Task<bool> InitializeDatabaseAsync();
}

public class MigrationStartupService : IMigrationStartupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationStartupService> _logger;

    public MigrationStartupService(IServiceProvider serviceProvider, ILogger<MigrationStartupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");

            using var scope = _serviceProvider.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

            // Check if database can connect
            if (!await migrationService.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to database");
                return false;
            }

            // Check for pending migrations
            var hasPendingMigrations = await migrationService.HasPendingMigrationsAsync();
            if (hasPendingMigrations)
            {
                _logger.LogInformation("Found pending migrations, applying...");

                var pendingMigrations = await migrationService.GetPendingMigrationsAsync();
                _logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));

                // Apply migrations
                if (!await migrationService.MigrateAsync())
                {
                    _logger.LogError("Failed to apply migrations");
                    return false;
                }

                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }

            // Verify database is ready
            var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
            var canConnect = await context.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("Database is not accessible after migration");
                return false;
            }

            _logger.LogInformation("Database initialization completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            return false;
        }
    }
}
```

## 4. Migration Extensions

**Datei:** `src/Invoice.Infrastructure/Data/Extensions/MigrationExtensions.cs`

```csharp
using Invoice.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Data.Extensions;

public static class MigrationExtensions
{
    public static IServiceCollection AddMigrationServices(this IServiceCollection services)
    {
        services.AddScoped<IMigrationService, MigrationService>();
        services.AddScoped<IMigrationStartupService, MigrationStartupService>();

        return services;
    }
}
```

## 5. Program.cs Integration

**Datei:** `src/Invoice.WinForms/Program.cs` (Erweiterung)

```csharp
using Invoice.Application.Extensions;
using Invoice.Infrastructure.Data.Extensions;
using Invoice.Infrastructure.Data.Services;
using Invoice.Infrastructure.Extensions;
using Invoice.WinForms.Extensions;
using Invoice.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Invoice.WinForms;

static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        // Initialize database
        var migrationStartupService = host.Services.GetRequiredService<IMigrationStartupService>();
        var migrationSuccess = await migrationStartupService.InitializeDatabaseAsync();

        if (!migrationSuccess)
        {
            MessageBox.Show("Failed to initialize database. Application will exit.", "Database Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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
                services.AddMigrationServices(); // Migration Services

                // Application Layer
                services.AddApplicationServices();

                // WinForms Layer
                services.AddWinFormsServices();
            });
    }
}
```

## Wichtige Hinweise

- Vollständige Initial Migration mit allen Tabellen und Indizes
- Migration Service für programmatische Migration
- Startup Service für automatische Migration beim App-Start
- Error Handling und Logging für alle Migration-Operationen
- Check Constraints für Datenintegrität
- Foreign Key-Beziehungen mit Cascade Delete
- Indizes für Performance-Optimierung
- Service Registration für Migration Services
- Integration in Program.cs für automatische Migration
