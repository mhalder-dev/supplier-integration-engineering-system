using SupplierService.Shared.Constants;
using SupplierService.Shared.DTOs;
using SupplierService.Shared.Http;

namespace SupplierService.Shared.Services;

/// <summary>
/// Shared orchestration plumbing. Feature services stay thin: validate, build, call, map.
/// See docs/architecture.md section 3.
/// </summary>
public abstract class SupplierServiceBase(ILogger logger)
{
    /// <summary>Feature name from <see cref="ApiNames"/>. Never a literal.</summary>
    protected abstract string ApiName { get; }

    protected ILogger Logger { get; } = logger;

    protected T Fail<T>(T response, string message, string? error = null,
        int statusCode = ServiceStatusCodes.InternalServerError) where T : BaseResponse
    {
        Logger.LogWarning("{ApiName} failed for {TrackingId}: {Message}",
            ApiName, response.TrackingId, message);
        return ResponseBuilder.Error(response, message, error, statusCode);
    }

    /// <summary>
    /// Four log points per feature - client request, supplier request, supplier response,
    /// client response. See docs/supplier-patterns.md section 8.
    /// Replace the body with the real ILogService queue when wiring a service.
    /// </summary>
    protected Task LogAsync(string stage, string trackingId, string payload)
    {
        Logger.LogInformation("{ApiName}.{Stage} {TrackingId} ({Bytes} bytes)",
            ApiName, stage, trackingId, payload.Length);
        return Task.CompletedTask;
    }
}
