using SupplierService.Features.Search.Constants;
using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Supplier.Request;

namespace SupplierService.Features.Search.ModelBuilder;

/// <summary>
/// Client request -> supplier request. Builds payloads only: no network, no mapping back.
/// </summary>
public sealed class SearchRequestModelBuilder : ISearchRequestModelBuilder
{
    public SupplierSearchRequest Build(ClientSearchRequest request)
    {
        var leg = request.Segments[0];

        return new SupplierSearchRequest
        {
            OriginCode = leg.Origin.ToUpperInvariant(),
            DestinationCode = leg.Destination.ToUpperInvariant(),
            DepartureDate = leg.DepartureDate.ToString(SearchProtocol.DateFormat),
            PassengerCount = request.Adults + request.Children + request.Infants,
            CabinCode = SearchProtocol.Cabin.Resolve(request.CabinClass)
        };
    }
}
