using Invoice.Application.Interfaces;
using Invoice.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoice.Application.Extensions;

public static class RegexPatternExtensions
{
    public static IServiceCollection AddRegexPatternServices(this IServiceCollection services)
    {
        services.AddSingleton<IRegexPatternService, RegexPatternService>();

        return services;
    }
}

