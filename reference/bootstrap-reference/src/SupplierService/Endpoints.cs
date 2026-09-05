using Microsoft.Extensions.Options;
using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.Services;
using SupplierService.Shared.Options;

namespace SupplierService;

/// <summary>
/// Routing only. One line per route: resolve the service, call it, return the result.
/// Logic at the boundary is a defect - docs/architecture.md section 3.
/// </summary>
public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/supplier");

        group.MapPost("/search", async (
            ClientSearchRequest request,
            ISearchService searchService,
            CancellationToken ct) => Results.Ok(await searchService.SearchAsync(request, ct)));

        group.MapGet("/status", (IWebHostEnvironment env, IOptions<SupplierOptions> options) => Results.Ok(new
        {
            service = options.Value.Name,
            status = "up",
            environment = env.EnvironmentName,
            serverTimeUtc = DateTime.UtcNow
        }));
    }
}
