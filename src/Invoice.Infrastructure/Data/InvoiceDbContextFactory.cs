using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoice.Infrastructure.Data;

public class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvoiceDbContext>();

        // Default connection string for design-time
        var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Invoice;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new InvoiceDbContext(optionsBuilder.Options);
    }
}

