using FluentValidation.Results;
using SupplierService.Shared.Constants;
using SupplierService.Shared.DTOs;

namespace SupplierService.Shared.Http;

/// <summary>
/// The only place a response envelope is shaped. Services and mappers never set
/// IsSuccess / StatusCode / Message by hand - docs/coding-standards.md section 5.
/// </summary>
public static class ResponseBuilder
{
    public static T Init<T>(string trackingId) where T : BaseResponse, new() => new()
    {
        TrackingId = trackingId,
        IsSuccess = true,
        StatusCode = ServiceStatusCodes.Ok,
        Message = ServiceMessages.Success
    };

    public static T Success<T>(T response, string? message = null) where T : BaseResponse
    {
        response.IsSuccess = true;
        response.StatusCode = ServiceStatusCodes.Ok;
        response.Message = message ?? ServiceMessages.Success;
        response.Error = null;
        return response;
    }

    public static T Error<T>(
        T response,
        string message,
        string? error = null,
        int statusCode = ServiceStatusCodes.InternalServerError) where T : BaseResponse
    {
        response.IsSuccess = false;
        response.StatusCode = statusCode;
        response.Message = message;
        response.Error = error;
        return response;
    }

    public static T ValidationError<T>(T response, ValidationResult validation) where T : BaseResponse =>
        Error(
            response,
            ServiceMessages.ValidationFailed,
            string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
            ServiceStatusCodes.BadRequest);

    public static T NoContent<T>(T response, string? message = null) where T : BaseResponse
    {
        response.IsSuccess = true;
        response.StatusCode = ServiceStatusCodes.NoContent;
        response.Message = message ?? ServiceMessages.NoResults;
        return response;
    }
}
