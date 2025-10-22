using Invoice.Application.Interfaces;
using Invoice.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Application.Extensions;

public static class ParserHelperExtensions
{
    public static IServiceCollection AddParserHelperServices(this IServiceCollection services)
    {
        services.AddSingleton<IParserHelperService, ParserHelperService>();

        return services;
    }
}

