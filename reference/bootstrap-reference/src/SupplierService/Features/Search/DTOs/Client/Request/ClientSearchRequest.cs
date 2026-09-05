using SupplierService.Shared.DTOs;

namespace SupplierService.Features.Search.DTOs.Client.Request;

/// <summary>Platform contract in. Never shaped by the supplier's wire format.</summary>
public sealed class ClientSearchRequest
{
    public string TrackingId { get; set; } = string.Empty;
    public string JourneyType { get; set; } = string.Empty;
    public List<SearchSegment> Segments { get; set; } = [];
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Infants { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public SupplierCredential Supplier { get; set; } = new();
}

public sealed class SearchSegment
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
}
