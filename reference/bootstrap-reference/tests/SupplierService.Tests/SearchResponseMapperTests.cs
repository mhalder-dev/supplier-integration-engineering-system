using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.DTOs.Supplier.Response;
using SupplierService.Features.Search.Mappers;
using SupplierService.Shared.DTOs;

namespace SupplierService.Tests;

/// <summary>
/// Layer 1 - mapper tests (docs/testing-strategy.md). Pure: fixture in, DTO out.
/// No network, no credentials, deterministic.
/// </summary>
public class SearchResponseMapperTests
{
    private static readonly SearchResponseMapper Mapper = new(new FlightResultAssembler());

    private static ClientSearchRequest Request(int adults = 1, int children = 0) => new()
    {
        TrackingId = "TRK-1",
        JourneyType = "OneWay",
        Adults = adults,
        Children = children,
        Segments = [new SearchSegment { Origin = "AAA", Destination = "BBB", DepartureDate = new DateOnly(2026, 10, 12) }],
        Supplier = new SupplierCredential { ShortCode = "XX", SupplierUId = "XX00001", ClientId = "id", ClientSecret = "secret" }
    };

    [Fact]
    public void Maps_every_offer_to_a_flight()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.NotEmpty(r.FlightId));
    }

    [Fact]
    public void Orders_results_by_total_price_ascending()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        // The fixture deliberately lists the more expensive offer first.
        Assert.Equal(48800.25m, results[0].Fare.TotalPrice);
        Assert.Equal(60500.50m, results[1].Fare.TotalPrice);
    }

    [Fact]
    public void Assigns_flight_ids_from_supplier_uid_and_one_based_index()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        Assert.Equal("XX00001-1", results[0].FlightId);
        Assert.Equal("XX00001-2", results[1].FlightId);
    }

    [Fact]
    public void Preserves_currency_and_does_not_round_amounts()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        Assert.Equal("XXX", results[0].Fare.Currency);
        Assert.Equal(39000.00m, results[0].Fare.BaseFare);
        Assert.Equal(9800.25m, results[0].Fare.Taxes);
    }

    [Fact]
    public void Divides_total_across_passengers_for_per_passenger_fare()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request(adults: 2));

        // Total is the all-passenger figure; per-passenger must not be confused with it.
        Assert.Equal(48800.25m, results[0].Fare.TotalPrice);
        Assert.Equal(24400.125m, results[0].Fare.PerPassengerTotal);
    }

    [Fact]
    public void Maps_segments_and_resolves_cabin_codes()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        var segment = Assert.Single(results[0].Segments);
        Assert.Equal("XX", segment.MarketingCarrier);
        Assert.Equal("716", segment.FlightNumber);
        Assert.Equal("AAA", segment.Origin);
        Assert.Equal("C", segment.CabinClass);
    }

    [Fact]
    public void Maps_baggage_allowances()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-oneway.json"), Request());

        Assert.Equal("40KG", results[0].Baggage.CheckedAllowance);
        Assert.Equal("10KG", results[0].Baggage.CabinAllowance);
    }

    [Fact]
    public void Returns_empty_list_for_error_response_rather_than_throwing()
    {
        var results = Mapper.Map(FixtureLoader.Load<SupplierSearchResponse>("search-error.json"), Request());

        Assert.Empty(results);
    }

    [Fact]
    public void Handles_absent_offers_collection_without_throwing()
    {
        var results = Mapper.Map(new SupplierSearchResponse { Status = "OK", Offers = null }, Request());

        Assert.Empty(results);
    }

    [Fact]
    public void Handles_offer_with_no_segments_or_price()
    {
        var response = new SupplierSearchResponse { Status = "OK", Offers = [new SupplierOffer { OfferId = "X" }] };

        var result = Assert.Single(Mapper.Map(response, Request()));
        Assert.Empty(result.Segments);
        Assert.Equal(0m, result.Fare.TotalPrice);
        Assert.Equal(string.Empty, result.Fare.Currency);
    }
}
