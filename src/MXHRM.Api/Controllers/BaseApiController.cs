using Microsoft.AspNetCore.Mvc;
using MXHRM.Api.Common;

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
}
