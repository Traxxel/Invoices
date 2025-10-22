using Invoice.Application.Interfaces;
using Invoice.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Infrastructure.Data.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceRawBlockRepository, InvoiceRawBlockRepository>();

        return services;
    }
}

