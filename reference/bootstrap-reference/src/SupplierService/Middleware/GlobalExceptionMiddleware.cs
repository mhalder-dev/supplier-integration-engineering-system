using System.Text.Json;
using SupplierService.Shared.Constants;

namespace SupplierService.Middleware;

/// <summary>
/// The single place an unhandled exception becomes a response. Feature services return
/// error envelopes rather than throwing; this catches what escapes.
/// </summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);

            if (context.Response.HasStarted) throw;

            context.Response.Clear();
            context.Response.StatusCode = ServiceStatusCodes.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                isSuccess = false,
                statusCode = ServiceStatusCodes.InternalServerError,
                message = ServiceMessages.UnhandledException
                // Deliberately no exception detail: it can carry credentials or PII.
            }));
        }
    }
}
