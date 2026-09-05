namespace SupplierService.Shared.DTOs;

/// <summary>
/// Every client-facing response subclasses this. Shape is set exclusively by
/// <see cref="Http.ResponseBuilder"/> - a service never assigns these by hand.
/// </summary>
public abstract class BaseResponse
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string TrackingId { get; set; } = string.Empty;
}
