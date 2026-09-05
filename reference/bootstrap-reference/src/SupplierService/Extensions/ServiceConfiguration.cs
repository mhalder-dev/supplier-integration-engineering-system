using FluentValidation;
using Microsoft.Extensions.Options;
using SupplierService.Features.Search.Mappers;
using SupplierService.Features.Search.ModelBuilder;
using SupplierService.Features.Search.Services;
using SupplierService.Shared.Options;
using SupplierService.Shared.Services;

namespace SupplierService.Extensions;

public static class ServiceConfiguration
{
    /// <summary>Feature registrations, grouped by feature. One group per slice.</summary>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenCache, TokenCache>();

        // Search
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ISearchRequestModelBuilder, SearchRequestModelBuilder>();
        services.AddScoped<ISearchResponseMapper, SearchResponseMapper>();
        services.AddScoped<FlightResultAssembler>();

        return services;
    }

    public static IServiceCollection AddSupplierOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SupplierOptions>(configuration.GetSection(SupplierOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Named clients only. Never new HttpClient() - see lesson L-04. Client certificates
    /// attach here via ConfigurePrimaryHttpMessageHandler, not by bypassing the factory.
    /// </summary>
    public static IServiceCollection AddSupplierHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(SearchService), (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplierOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }

    /// <summary>Validators auto-register by assembly scan - no per-validator DI wiring.</summary>
    public static IServiceCollection AddSupplierValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }
}
