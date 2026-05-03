using System.Net;
using System.Text.Json;
using MXHRM.Api.DTOs.Common;

namespace MXHRM.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;     // Middleware delegate to call the next middleware in the pipeline
    private readonly IWebHostEnvironment _env;  // Environment to determine if we are in development or production
    private readonly ILogger<GlobalExceptionMiddleware> _logger;    // Logger to log exceptions

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IWebHostEnvironment env,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)      // Main method that gets called for each HTTP request
    {
        try
        {
            await _next(context);   // Call the next middleware in the pipeline
        }
        catch (Exception ex)
        {
            // Log the exception with the trace identifier
            _logger.LogError(
                ex,
                "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)  // Method to handle the exception and return a standardized error response
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = ex switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Message = "An unexpected error occurred.",
            Detail = _env.IsDevelopment() ? ex.Message : null,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}