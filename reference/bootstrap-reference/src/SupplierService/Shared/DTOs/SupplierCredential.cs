namespace SupplierService.Shared.DTOs;

/// <summary>
/// Credentials arrive per request from the platform. They are NEVER read from source or
/// committed to config - see docs/coding-standards.md section 11.
/// </summary>
public sealed class SupplierCredential
{
    public string SupplierUId { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SupplierCurrency { get; set; } = string.Empty;

    /// <summary>Cache key for the token store. Never log this.</summary>
    public string CacheKey() => $"{ShortCode}|{ClientId}";
}
