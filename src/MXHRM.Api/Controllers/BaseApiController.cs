using Microsoft.AspNetCore.Mvc;
using MXHRM.Api.Common;
using MXHRM.Application.Common;

namespace MXHRM.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected ObjectResult Error(
        int statusCode,
        string code,
        string message,
        object? details = null)
    {
        return StatusCode(statusCode, new ErrorResponse
        {
            StatusCode = statusCode,
            Code = code,
            Message = message,
            Details = details,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    protected ObjectResult NotFoundError(string message)
    {
        return Error(
            StatusCodes.Status404NotFound,
            ErrorCodes.NotFound,
            message);
    }

    protected ObjectResult BadRequestError(
        string message,
        object? details = null)
    {
        return Error(
            StatusCodes.Status400BadRequest,
            ErrorCodes.BadRequest,
            message,
            details);
    }

    protected ObjectResult ConflictError(string message)
    {
        return Error(
            StatusCodes.Status409Conflict,
            ErrorCodes.Conflict,
            message);
    }

    protected ObjectResult OperationError(OperationResult result)
    {
        return result.ErrorType switch
        {
            OperationErrorType.NotFound => NotFoundError(result.ErrorMessage ?? "Resource not found."),
            _ => BadRequestError(result.ErrorMessage ?? "Request failed.", result.Details)
        };
    }
}
