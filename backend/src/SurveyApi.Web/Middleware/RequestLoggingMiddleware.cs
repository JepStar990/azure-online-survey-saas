using System.Diagnostics;

namespace SurveyApi.Web.Middleware;

/// <summary>
/// Logs every HTTP request with method, path, status code, and duration.
/// Helps with debugging, auditing, and performance monitoring.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);
            sw.Stop();

            var level = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(level, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogWarning("HTTP {Method} {Path} failed after {ElapsedMs}ms",
                method, path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
