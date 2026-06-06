namespace SurveyApi.Web.Endpoints;

/// <summary>
/// Endpoints for managing questions within a survey.
/// Currently question management is handled through the survey update endpoint
/// (full replacement of questions). This file provides individual question operations
/// for future use (Phase 3 drag-and-drop reorder, etc.).
/// </summary>
public static class QuestionEndpoints
{
    public static void MapQuestionEndpoints(this WebApplication app)
    {
        // Question CRUD is currently managed via the survey PUT endpoint
        // which does a full replacement of the questions list.
        // Individual question endpoints will be added in Phase 3 for:
        // - Drag-and-drop reorder
        // - Individual question add/remove
        // - Conditional logic configuration

        var group = app.MapGroup("/api/v1/surveys/{surveyId:guid}/questions")
            .RequireAuthorization()
            .WithTags("Questions");

        // Placeholder for future individual question management
        group.MapGet("/", (Guid surveyId) =>
        {
            // Redirect to survey GET which includes questions
            return Results.Redirect($"/api/v1/surveys/{surveyId}");
        })
        .AllowAnonymous() // The survey endpoint handles auth
        .WithSummary("Get questions for a survey (redirects to survey endpoint)");
    }
}
