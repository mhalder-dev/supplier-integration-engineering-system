using FluentValidation;
using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Client.Response;
using SupplierService.Features.Search.DTOs.Supplier.Response;
using SupplierService.Features.Search.Mappers;
using SupplierService.Features.Search.ModelBuilder;
using SupplierService.Shared.Constants;
using SupplierService.Shared.Http;
using SupplierService.Shared.Services;

namespace SupplierService.Features.Search.Services;

/// <summary>
/// Orchestration ONLY - no payload building, no field mapping. The sequence below is the
/// standard one for every feature (docs/architecture.md section 3).
/// </summary>
public sealed class SearchService(
    IValidator<ClientSearchRequest> validator,
    ISearchRequestModelBuilder modelBuilder,
    ISearchResponseMapper mapper,
    ILogger<SearchService> logger)
    : SupplierServiceBase(logger), ISearchService
{
    protected override string ApiName => ApiNames.Search;

    private const string NoOffersReturned = "Supplier returned no offers for this itinerary";

    public async Task<ClientSearchResponse> SearchAsync(ClientSearchRequest request, CancellationToken ct = default)
    {
        var response = ResponseBuilder.Init<ClientSearchResponse>(request?.TrackingId ?? string.Empty);

        try
        {
            // 1. Validate FIRST - before reading any field of the request.
            var validation = await validator.ValidateAsync(request!, ct);
            if (!validation.IsValid)
                return ResponseBuilder.ValidationError(response, validation);

            await LogAsync("ClientRequest", response.TrackingId, request!.JourneyType);

            // 2. Build the supplier payload.
            var supplierRequest = modelBuilder.Build(request);
            await LogAsync("SupplierRequest", response.TrackingId, supplierRequest.OriginCode);

            // 3. Call the supplier.
            //    TEMPLATE STUB: replace with the named IHttpClientFactory client and the
            //    token cache. Left unimplemented deliberately - it needs the real API.
            SupplierSearchResponse? supplierResponse = await CallSupplierAsync(supplierRequest, ct);
            await LogAsync("SupplierResponse", response.TrackingId, supplierResponse?.Status ?? "null");

            // 4. Guard the result before mapping.
            if (supplierResponse is null)
                return Fail(response, ServiceMessages.NoSupplierResponse, statusCode: ServiceStatusCodes.BadGateway);

            if (!string.IsNullOrWhiteSpace(supplierResponse.ErrorCode))
                return Fail(response, ServiceMessages.SupplierError,
                    $"{supplierResponse.ErrorCode}: {supplierResponse.ErrorMessage}",
                    ServiceStatusCodes.UnprocessableEntity);

            // 5. Map.
            response.Flights = mapper.Map(supplierResponse, request);
            if (response.Flights.Count == 0)
                return ResponseBuilder.NoContent(response, NoOffersReturned);

            await LogAsync("ClientResponse", response.TrackingId, $"{response.Flights.Count} results");
            return ResponseBuilder.Success(response);
        }
        catch (OperationCanceledException)
        {
            throw; // Cancellation is not a failure - let it propagate.
        }
        catch (Exception ex)
        {
            return Fail(response, ServiceMessages.UnhandledException, ex.Message);
        }
    }

    private static Task<SupplierSearchResponse?> CallSupplierAsync(
        DTOs.Supplier.Request.SupplierSearchRequest request, CancellationToken ct)
    {
        _ = request;
        _ = ct;
        throw new NotImplementedException(
            "Template stub. Wire the named HttpClient and ITokenCache when implementing a real supplier.");
    }
}
