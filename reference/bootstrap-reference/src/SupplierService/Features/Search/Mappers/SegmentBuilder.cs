using SupplierService.Features.Search.Constants;
using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

/// <summary>One output concern: flight segments. Stateless, so static.</summary>
public static class SegmentBuilder
{
    public static List<FlightSegmentDto> Build(SupplierOffer offer) =>
        (offer.Segments ?? []).Select(Map).ToList();

    private static FlightSegmentDto Map(SupplierSegment segment) => new()
    {
        MarketingCarrier = segment.Carrier ?? string.Empty,
        FlightNumber = segment.FlightNo ?? string.Empty,
        Origin = segment.From ?? string.Empty,
        Destination = segment.To ?? string.Empty,
        DepartureTime = segment.DepartsLocal ?? string.Empty,
        ArrivalTime = segment.ArrivesLocal ?? string.Empty,
        CabinClass = SearchProtocol.Cabin.Resolve(segment.Cabin)
    };
}
