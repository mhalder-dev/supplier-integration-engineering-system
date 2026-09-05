using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

/// <summary>One output concern: baggage allowance.</summary>
public static class BaggageBuilder
{
    public static BaggageDto Build(SupplierOffer offer) => new()
    {
        CheckedAllowance = offer.BaggageAllowance ?? string.Empty,
        CabinAllowance = offer.CabinBaggage ?? string.Empty
    };
}
