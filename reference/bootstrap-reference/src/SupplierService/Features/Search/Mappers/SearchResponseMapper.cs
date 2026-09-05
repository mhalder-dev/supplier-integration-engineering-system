using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

/// <summary>
/// THIN orchestrator. It composes builders and holds no field mapping. If this grows past
/// ~80 lines a concern has leaked in - extract another builder (ADR-0002).
///
/// The equivalent class in the current ecosystem runs to 2,028 lines. That is what this
/// shape exists to prevent.
/// </summary>
public sealed class SearchResponseMapper(FlightResultAssembler assembler) : ISearchResponseMapper
{
    public List<FlightResultDto> Map(SupplierSearchResponse response, ClientSearchRequest request) =>
        (response.Offers ?? [])
            .OrderBy(offer => offer.Price?.Total ?? decimal.MaxValue)
            .Select((offer, index) => assembler.Build(FlightId(request, index + 1), offer, request))
            .ToList();

    private static string FlightId(ClientSearchRequest request, int oneBasedIndex)
    {
        var uid = request.Supplier.SupplierUId;
        if (string.IsNullOrWhiteSpace(uid)) uid = request.Supplier.ShortCode;
        return $"{uid}-{oneBasedIndex}";
    }
}
