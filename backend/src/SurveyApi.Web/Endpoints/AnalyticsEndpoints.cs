using SurveyApi.Application.Services;

namespace SurveyApi.Web.Endpoints;

/// <summary>
/// Endpoints for survey analytics and result aggregation.
/// </summary>
public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/surveys/{surveyId:guid}/analytics")
            .RequireAuthorization()
            .WithTags("Analytics");

        // GET: Summary analytics for a survey
        group.MapGet("summary", async (
            Guid surveyId,
            AnalyticsService service,
            CancellationToken ct) =>
        {
            var summary = await service.GetSummaryAsync(surveyId, ct);
            return summary is not null ? Results.Ok(summary) : Results.NotFound();
        })
        .WithSummary("Get analytics summary for a survey");
    }
}
