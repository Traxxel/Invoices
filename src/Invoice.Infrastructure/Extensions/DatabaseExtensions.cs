using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext wird in späteren Aufgaben implementiert
        // services.AddDbContext<InvoiceDbContext>(options =>
        // {
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        //     options.EnableSensitiveDataLogging(false);
        //     options.EnableDetailedErrors();
        // });

        // Repository Pattern wird in späteren Aufgaben implementiert
        // services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        // services.AddScoped<IInvoiceRawBlockRepository, InvoiceRawBlockRepository>();

        return services;
    }
}

