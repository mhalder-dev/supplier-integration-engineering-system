using FluentValidation;
using SupplierService.Features.Search.DTOs.Client.Request;

namespace SupplierService.Features.Search.Validator;

/// <summary>
/// Runs before any field is read - the upstream request is untrusted
/// (docs/architecture.md section 3). Auto-registers via assembly scan.
/// </summary>
public sealed class ClientSearchRequestValidator : AbstractValidator<ClientSearchRequest>
{
    public ClientSearchRequestValidator()
    {
        RuleFor(x => x.TrackingId).NotEmpty();
        RuleFor(x => x.JourneyType).NotEmpty();
        RuleFor(x => x.Adults).GreaterThan(0);
        RuleFor(x => x.Children).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Infants).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Infants)
            .LessThanOrEqualTo(x => x.Adults)
            .WithMessage("Infants cannot outnumber adults.");

        RuleFor(x => x.Segments).NotEmpty();
        RuleForEach(x => x.Segments).ChildRules(segment =>
        {
            segment.RuleFor(s => s.Origin).NotEmpty().Length(3);
            segment.RuleFor(s => s.Destination).NotEmpty().Length(3);
            segment.RuleFor(s => s.Destination)
                .NotEqual(s => s.Origin)
                .WithMessage("Origin and destination must differ.");
            segment.RuleFor(s => s.DepartureDate)
                .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Departure date cannot be in the past.");
        });

        // Null-guard the nested credential before any field of it is read.
        RuleFor(x => x.Supplier).NotNull();
        When(x => x.Supplier is not null, () =>
        {
            RuleFor(x => x.Supplier.ShortCode).NotEmpty();
            RuleFor(x => x.Supplier.ClientId).NotEmpty();
            RuleFor(x => x.Supplier.ClientSecret).NotEmpty();
        });
    }
}
