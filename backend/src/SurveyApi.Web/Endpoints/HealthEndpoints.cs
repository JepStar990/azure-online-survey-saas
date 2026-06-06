using Microsoft.EntityFrameworkCore;
using SurveyApi.Infrastructure.Data;

namespace SurveyApi.Web.Endpoints;

/// <summary>
/// Health check endpoints for monitoring and load balancer probes.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1");

        // Basic liveness probe — no dependencies
        group.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        }))
        .AllowAnonymous()
        .WithTags("Health");

        // Readiness probe — checks database connectivity
        group.MapGet("/health/ready", async (AppDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok(new { status = "Ready", database = "Connected" })
                    : Results.StatusCode(503);
            }
            catch
            {
                return Results.StatusCode(503);
            }
        })
        .AllowAnonymous()
        .WithTags("Health");
    }
}
