using System.Net;
using System.Text.Json;
using MXHRM.Api.Common;
using Microsoft.EntityFrameworkCore;

namespace MXHRM.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex,
                StatusCodes.Status401Unauthorized,
                ErrorCodes.Unauthorized,
                "Unauthorized.");
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex,
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex,
                StatusCodes.Status404NotFound,
                ErrorCodes.NotFound,
                ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex,
                StatusCodes.Status409Conflict,
                ErrorCodes.Conflict,
                "The record was modified by another user. Please reload and try again.");
        }
        catch (Exception ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex,
                StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private async Task WriteErrorResponseAsync(
        HttpContext context,
        Exception exception,
        int statusCode,
        string code,
        string message,
        object? details = null)
    {
        _logger.LogError(
            exception,
            "Request failed with status code {StatusCode} and error code {ErrorCode}",
            statusCode,
            code);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Code = code,
            Message = message,
            Details = details,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
