using System.Net;
using System.Text.Json;

namespace SdiApiGateway.Middleware;

/// <summary>
/// Global exception handling middleware — replaces Spring Boot @RestControllerAdvice.
/// </summary>
public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (InterviewException ex)
        {
            _logger.LogError("Interview error: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = ex.Message,
                timestamp = DateTime.UtcNow.ToString("o"),
                status = 400
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred",
                timestamp = DateTime.UtcNow.ToString("o"),
                status = 500
            }));
        }
    }
}

public class InterviewException : Exception
{
    public InterviewException(string message) : base(message) { }
    public InterviewException(string message, Exception inner) : base(message, inner) { }
}
