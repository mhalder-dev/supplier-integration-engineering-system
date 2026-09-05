namespace SupplierService.Features.Search.DTOs.Supplier.Request;

/// <summary>
/// Supplier wire shape. MUST NOT appear in any client-facing signature -
/// docs/architecture.md section 2. Replace with the real supplier contract.
/// </summary>
public sealed class SupplierSearchRequest
{
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public string DepartureDate { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public string CabinCode { get; set; } = string.Empty;
}
