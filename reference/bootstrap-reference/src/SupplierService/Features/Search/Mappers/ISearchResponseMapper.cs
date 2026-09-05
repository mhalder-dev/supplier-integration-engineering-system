using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;

namespace SupplierService.Features.Search.Mappers;

public interface ISearchResponseMapper
{
    List<FlightResultDto> Map(SupplierSearchResponse response, ClientSearchRequest request);
}
