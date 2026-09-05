using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Supplier.Request;

namespace SupplierService.Features.Search.ModelBuilder;

public interface ISearchRequestModelBuilder
{
    SupplierSearchRequest Build(ClientSearchRequest request);
}
