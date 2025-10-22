using Invoice.Application.Interfaces;
using Invoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.Data.Seeders;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(InvoiceDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if already seeded
            if (await IsSeededAsync())
            {
                _logger.LogInformation("Database already seeded, skipping...");
                return true;
            }

            // Seed sample data
            await SeedSampleInvoicesAsync();
            await SeedSampleRawBlocksAsync();

            _logger.LogInformation("Database seeding completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed");
            return false;
        }
    }

    public async Task<bool> IsSeededAsync()
    {
        try
        {
            var count = await _context.Invoices.CountAsync();
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if database is seeded");
            return false;
        }
    }

    public async Task<bool> ClearSeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Clearing seed data...");

            // Delete all data
            await _context.InvoiceRawBlocks.ExecuteDeleteAsync();
            await _context.Invoices.ExecuteDeleteAsync();

            _logger.LogInformation("Seed data cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear seed data");
            return false;
        }
    }

    public async Task<SeedResult> GetSeedStatusAsync()
    {
        var result = new SeedResult();

        try
        {
            result.IsSeeded = await IsSeededAsync();
            result.SeedCount = await _context.Invoices.CountAsync();
            result.LastSeeded = await GetLastSeededDateAsync();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to get seed status: {ex.Message}");
        }

        return result;
    }

    private async Task SeedSampleInvoicesAsync()
    {
        var sampleInvoices = new List<Domain.Entities.Invoice>
        {
            Domain.Entities.Invoice.Create(
                "INV-2025-001",
                new DateOnly(2025, 1, 15),
                "Musterfirma GmbH",
                1000.00m,
                190.00m,
                1190.00m,
                "storage/invoices/2025/01/sample1.pdf",
                0.95f,
                "v1.0"
            ),
            Domain.Entities.Invoice.Create(
                "RE-2025-002",
                new DateOnly(2025, 1, 20),
                "Beispiel AG",
                500.00m,
                95.00m,
                595.00m,
                "storage/invoices/2025/01/sample2.pdf",
                0.88f,
                "v1.0"
            ),
            Domain.Entities.Invoice.Create(
                "RG-2025-003",
                new DateOnly(2025, 2, 1),
                "Test Unternehmen",
                2500.00m,
                475.00m,
                2975.00m,
                "storage/invoices/2025/02/sample3.pdf",
                0.92f,
                "v1.0"
            )
        };

        // Update issuer information
        sampleInvoices[0].UpdateIssuerInfo("Musterfirma GmbH", "Musterstraße 1", "12345", "Musterstadt", "Deutschland");
        sampleInvoices[1].UpdateIssuerInfo("Beispiel AG", "Beispielweg 2", "54321", "Beispielstadt", "Deutschland");
        sampleInvoices[2].UpdateIssuerInfo("Test Unternehmen", "Teststraße 3", "98765", "Teststadt", "Deutschland");

        _context.Invoices.AddRange(sampleInvoices);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sample invoices", sampleInvoices.Count);
    }

    private async Task SeedSampleRawBlocksAsync()
    {
        var invoices = await _context.Invoices.ToListAsync();
        var rawBlocks = new List<InvoiceRawBlock>();

        foreach (var invoice in invoices)
        {
            // Create sample raw blocks for each invoice
            var sampleBlocks = new List<InvoiceRawBlock>
            {
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Rechnungs-Nr.: {invoice.InvoiceNumber}",
                    100f, 50f, 200f, 20f,
                    0,
                    "InvoiceNumber",
                    0.95f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Datum: {invoice.InvoiceDate:dd.MM.yyyy}",
                    100f, 80f, 150f, 20f,
                    1,
                    "InvoiceDate",
                    0.90f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    invoice.IssuerName,
                    100f, 110f, 300f, 20f,
                    2,
                    "IssuerAddress",
                    0.85f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Netto: {invoice.NetTotal:C}",
                    100f, 140f, 150f, 20f,
                    3,
                    "NetTotal",
                    0.88f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"MwSt: {invoice.VatTotal:C}",
                    100f, 170f, 150f, 20f,
                    4,
                    "VatTotal",
                    0.87f
                ),
                InvoiceRawBlock.CreateWithPrediction(
                    invoice.Id,
                    1,
                    $"Gesamt: {invoice.GrossTotal:C}",
                    100f, 200f, 150f, 20f,
                    5,
                    "GrossTotal",
                    0.93f
                )
            };

            rawBlocks.AddRange(sampleBlocks);
        }

        _context.InvoiceRawBlocks.AddRange(rawBlocks);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sample raw blocks", rawBlocks.Count);
    }

    private async Task<DateTime?> GetLastSeededDateAsync()
    {
        try
        {
            if (!await _context.Invoices.AnyAsync()) return null;
            return await _context.Invoices.MinAsync(i => i.ImportedAt);
        }
        catch
        {
            return null;
        }
    }
}

