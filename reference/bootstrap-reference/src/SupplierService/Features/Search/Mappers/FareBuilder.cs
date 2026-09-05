using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

/// <summary>
/// One output concern: fare. Amounts are never rounded here - rounding is a presentation
/// decision that belongs upstream (docs/supplier-patterns.md section 7).
/// </summary>
public static class FareBuilder
{
    public static FareDto Build(SupplierOffer offer, int passengerCount)
    {
        var price = offer.Price;
        var total = price?.Total ?? 0m;

        return new FareDto
        {
            BaseFare = price?.Base ?? 0m,
            Taxes = price?.Tax ?? 0m,
            TotalPrice = total,
            Currency = price?.Currency ?? string.Empty,
            PerPassengerTotal = passengerCount > 0 ? total / passengerCount : total
        };
    }
}
