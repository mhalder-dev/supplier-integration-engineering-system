using SupplierService.Features.Search.DTOs.Client.Request;
using SupplierService.Features.Search.Validator;
using SupplierService.Shared.DTOs;

namespace SupplierService.Tests;

/// <summary>Layer 3 - validator tests. Cheap, and they catch the null-reference class of failure.</summary>
public class ClientSearchRequestValidatorTests
{
    private static readonly ClientSearchRequestValidator Validator = new();

    private static ClientSearchRequest Valid() => new()
    {
        TrackingId = "TRK-1",
        JourneyType = "OneWay",
        Adults = 1,
        Segments = [new SearchSegment
        {
            Origin = "AAA",
            Destination = "BBB",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30))
        }],
        Supplier = new SupplierCredential { ShortCode = "XX", ClientId = "id", ClientSecret = "secret" }
    };

    [Fact]
    public void Accepts_a_well_formed_request() => Assert.True(Validator.Validate(Valid()).IsValid);

    [Fact]
    public void Rejects_missing_tracking_id()
    {
        var request = Valid();
        request.TrackingId = string.Empty;
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_zero_adults()
    {
        var request = Valid();
        request.Adults = 0;
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_more_infants_than_adults()
    {
        var request = Valid();
        request.Adults = 1;
        request.Infants = 2;
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_empty_segments()
    {
        var request = Valid();
        request.Segments = [];
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_departure_date_in_the_past()
    {
        var request = Valid();
        request.Segments[0].DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_origin_equal_to_destination()
    {
        var request = Valid();
        request.Segments[0].Destination = request.Segments[0].Origin;
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_missing_supplier_credentials()
    {
        var request = Valid();
        request.Supplier.ClientSecret = string.Empty;
        Assert.False(Validator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData("DA")]
    [InlineData("DACX")]
    public void Rejects_airport_codes_that_are_not_three_letters(string code)
    {
        var request = Valid();
        request.Segments[0].Origin = code;
        Assert.False(Validator.Validate(request).IsValid);
    }
}
