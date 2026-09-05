namespace SupplierService.Features.Search.DTOs.Supplier.Response;

/// <summary>
/// Supplier wire shape. Every field is nullable until a captured response proves otherwise -
/// a spec saying "mandatory" is not proof (docs/coding-standards.md section 4).
/// </summary>
public sealed class SupplierSearchResponse
{
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SupplierOffer>? Offers { get; set; }
}

public sealed class SupplierOffer
{
    public string? OfferId { get; set; }
    public List<SupplierSegment>? Segments { get; set; }
    public SupplierPrice? Price { get; set; }
    public string? BaggageAllowance { get; set; }
    public string? CabinBaggage { get; set; }
    public int? SeatsRemaining { get; set; }
}

public sealed class SupplierSegment
{
    public string? Carrier { get; set; }
    public string? FlightNo { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? DepartsLocal { get; set; }
    public string? ArrivesLocal { get; set; }
    public string? Cabin { get; set; }
}

public sealed class SupplierPrice
{
    public decimal? Base { get; set; }
    public decimal? Tax { get; set; }
    public decimal? Total { get; set; }
    public string? Currency { get; set; }
}
