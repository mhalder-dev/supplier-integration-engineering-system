namespace SupplierService.Shared.Constants;

/// <summary>
/// Canonical feature names - see docs/coding-standards.md section 2 and ADR-0003.
/// Add a member here when adding a feature; never spell the name inline.
/// </summary>
public static class ApiNames
{
    public const string Auth = "Auth";
    public const string Search = "Search";
    public const string Revalidate = "Revalidate";
    public const string Booking = "Booking";
    public const string IssueTicket = "IssueTicket";
    public const string CancelBooking = "CancelBooking";
    public const string RetrieveBooking = "RetrieveBooking";
    public const string Refund = "Refund";
    public const string FareRule = "FareRule";
    public const string ImportPnr = "ImportPnr";
    public const string VoidTicket = "VoidTicket";
}
