namespace SupplierService.Features.Search.Constants;

/// <summary>Supplier-protocol-fixed values. Not configuration - these do not vary by environment.</summary>
public static class SearchProtocol
{
    public const string StatusOk = "OK";
    public const string DateFormat = "yyyy-MM-dd";
    public const string OffsetDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

    public static class Cabin
    {
        public const string Economy = "Y";
        public const string PremiumEconomy = "W";
        public const string Business = "C";
        public const string First = "F";

        /// <summary>Full code list plus a resolve seam - docs/coding-standards.md section 3.</summary>
        public static string Resolve(string? hint) => hint?.Trim().ToUpperInvariant() switch
        {
            "PREMIUMECONOMY" or "PREMIUM_ECONOMY" or "W" => PremiumEconomy,
            "BUSINESS" or "C" => Business,
            "FIRST" or "F" => First,
            _ => Economy
        };
    }
}
