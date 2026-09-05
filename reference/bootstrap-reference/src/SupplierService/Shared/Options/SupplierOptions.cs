namespace SupplierService.Shared.Options;

/// <summary>
/// Environment-varying values only. Protocol constants belong in a Constants class -
/// see docs/coding-standards.md section 3.
/// </summary>
public sealed class SupplierOptions
{
    public const string SectionName = "Supplier";

    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;

    /// <summary>
    /// Full base path including any environment segment. Test and production often differ
    /// by path alone (e.g. /v1/test/ vs /v1/) - that is configuration, never a code branch.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string TokenUrl { get; set; } = string.Empty;
    public int TokenLifetimeSeconds { get; set; } = 300;

    /// <summary>Expire cached tokens early so none expires mid-request.</summary>
    public int TokenSafetyMarginSeconds { get; set; } = 60;

    public int TimeoutSeconds { get; set; } = 30;
}
