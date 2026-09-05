namespace SupplierService.Shared.Constants;

/// <summary>Cross-feature message text. Feature-unique text belongs in a private const.</summary>
public static class ServiceMessages
{
    public const string Success = "Success";
    public const string ValidationFailed = "Request validation failed";
    public const string NoSupplierResponse = "No response received from supplier";
    public const string SupplierError = "Supplier returned an error";
    public const string UnhandledException = "An unhandled error occurred";
    public const string AuthenticationFailed = "Supplier authentication failed";
    public const string NoResults = "No results found";
}
