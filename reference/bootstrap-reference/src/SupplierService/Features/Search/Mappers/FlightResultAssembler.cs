using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

/// <summary>
/// Composes builder output into the client DTO. Holds assembly logic only - any real
/// field mapping belongs in a builder.
/// </summary>
public sealed class FlightResultAssembler
{
    public FlightResultDto Build(string flightId, SupplierOffer offer, ClientSearchRequest request)
    {
        var passengerCount = Math.Max(request.Adults + request.Children, 1);

        return new FlightResultDto
        {
            FlightId = flightId,
            TrackingId = request.TrackingId,
            JourneyType = request.JourneyType,
            SupplierCode = request.Supplier.ShortCode,
            SupplierUid = request.Supplier.SupplierUId,
            IsInternationalFlight = IsInternational(offer),
            Segments = SegmentBuilder.Build(offer),
            Fare = FareBuilder.Build(offer, passengerCount),
            Baggage = BaggageBuilder.Build(offer),
            Availability = offer.SeatsRemaining ?? 0
        };
    }

    /// <summary>
    /// Placeholder: real implementations resolve the country of each airport from reference
    /// data. Replace when wiring a supplier - do not ship this heuristic.
    /// </summary>
    private static bool IsInternational(SupplierOffer offer)
    {
        var segments = offer.Segments ?? [];
        if (segments.Count == 0) return false;

        var first = segments[0].From;
        return segments.Any(s => !string.Equals(s.To, first, StringComparison.OrdinalIgnoreCase));
    }
}
