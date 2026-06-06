using System.Net;
using System.Text.Json;

namespace SurveyApi.Web.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns RFC 7807 Problem Details responses.
/// Prevents sensitive error details from leaking to clients in production.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — not a server error, don't log
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (InvalidOperationException ex)
        {
            // Business rule violation — return 400
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";
            await WriteProblemDetails(context, "Bad Request", ex.Message, 400);
        }
        catch (Exception ex)
        {
            // Unexpected error — log and return 500
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.";
            await WriteProblemDetails(context, "Internal Server Error", detail, 500);
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, string title, string detail, int status)
    {
        var problem = new
        {
            type = $"https://httpstatuses.com/{status}",
            title,
            status,
            detail,
            instance = context.Request.Path.Value,
            timestamp = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
