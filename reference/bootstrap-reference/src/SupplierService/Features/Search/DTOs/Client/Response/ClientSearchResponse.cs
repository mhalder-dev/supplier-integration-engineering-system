using SupplierService.Shared.DTOs;

namespace SupplierService.Features.Search.DTOs.Client.Response;

public sealed class ClientSearchResponse : BaseResponse
{
    public List<FlightResultDto> Flights { get; set; } = [];
}

/// <summary>
/// The platform contract the aggregator consumes.
///
/// WARNING: this type exists in 17 divergent copies across the current ecosystem, with 17
/// different hashes and property counts from 4 to 23 (lesson L-02). Do NOT edit it to suit
/// one supplier. It moves to a shared package under ADR-0001; until then, changes go through
/// the aggregator team.
/// </summary>
public sealed class FlightResultDto
{
    public string FlightId { get; set; } = string.Empty;
    public string TrackingId { get; set; } = string.Empty;
    public string JourneyType { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierUid { get; set; } = string.Empty;
    public bool IsInternationalFlight { get; set; }
    public List<FlightSegmentDto> Segments { get; set; } = [];
    public FareDto Fare { get; set; } = new();
    public BaggageDto Baggage { get; set; } = new();
    public int Availability { get; set; }
}

public sealed class FlightSegmentDto
{
    public string MarketingCarrier { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string CabinClass { get; set; } = string.Empty;
}

/// <summary>Money is decimal and always carries its currency - docs/supplier-patterns.md section 7.</summary>
public sealed class FareDto
{
    public decimal BaseFare { get; set; }
    public decimal Taxes { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = string.Empty;

    /// <summary>Per-passenger, NOT the all-passenger total. Confusing the two is a classic defect.</summary>
    public decimal PerPassengerTotal { get; set; }
}

public sealed class BaggageDto
{
    public string CheckedAllowance { get; set; } = string.Empty;
    public string CabinAllowance { get; set; } = string.Empty;
}
