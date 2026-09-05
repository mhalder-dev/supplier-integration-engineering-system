namespace SupplierService.Shared.Constants;

/// <summary>Status numbers come from here only - never inline. docs/coding-standards.md section 3.</summary>
public static class ServiceStatusCodes
{
    public const int Ok = 200;
    public const int NoContent = 204;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
    public const int InternalServerError = 500;
    public const int BadGateway = 502;
    public const int GatewayTimeout = 504;
}
