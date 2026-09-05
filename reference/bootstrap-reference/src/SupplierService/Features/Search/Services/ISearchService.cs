using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Client.Response;

namespace SupplierService.Features.Search.Services;

public interface ISearchService
{
    Task<ClientSearchResponse> SearchAsync(ClientSearchRequest request, CancellationToken ct = default);
}
