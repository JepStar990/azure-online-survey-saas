using Microsoft.AspNetCore.Mvc;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Services;
using SurveyApi.Application.Validators;

namespace SurveyApi.Web.Endpoints;

/// <summary>
/// Endpoints for submitting and retrieving survey responses.
/// Public submission endpoints are rate-limited. Authenticated read endpoints
/// require SurveyViewer role or higher.
/// </summary>
public static class ResponseEndpoints
{
    public static void MapResponseEndpoints(this WebApplication app)
    {
        // --- Public response submission (by public link) ---
        var publicGroup = app.MapGroup("/api/v1/s/{publicLinkId}")
            .WithTags("Responses");

        // GET: Get survey for taking (public)
        publicGroup.MapGet("/", async (
            string publicLinkId,
            SurveyService service,
            CancellationToken ct) =>
        {
            var survey = await service.GetByPublicLinkAsync(publicLinkId, ct);
            return survey is not null ? Results.Ok(survey) : Results.NotFound();
        })
        .AllowAnonymous()
        .WithSummary("Get a published survey by public link (for respondents)");

        // POST: Submit a response (public, rate-limited)
        publicGroup.MapPost("/responses", async (
            string publicLinkId,
            [FromBody] ResponseSubmitRequest request,
            ResponseSubmitValidator validator,
            ResponseService service,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            try
            {
                // Extract respondent info: authenticated user or anonymous
                var userIdClaim = httpContext.User.FindFirst(
                    "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                    ?? httpContext.User.FindFirst("oid")?.Value;

                Guid? respondentId = userIdClaim is not null && Guid.TryParse(userIdClaim, out var g) ? g : null;

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var response = await service.SubmitAsync(
                    publicLinkId, request, respondentId, ipAddress, userAgent, ct);

                return Results.Created($"/api/v1/responses/{response.Id}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .AllowAnonymous()
        .WithSummary("Submit a survey response")
        // Rate limit: 30 requests per minute per IP for anonymous submissions
        .RequireRateLimiting("AnonymousResponseRateLimit");

        // --- Authenticated response retrieval ---
        var authGroup = app.MapGroup("/api/v1/surveys/{surveyId:guid}/responses")
            .RequireAuthorization()
            .WithTags("Responses");

        // GET: List responses for a survey (paginated)
        authGroup.MapGet("/", async (
            Guid surveyId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ResponseService service,
            CancellationToken ct) =>
        {
            page = Math.Max(1, page == 0 ? 1 : page);
            pageSize = Math.Clamp(pageSize == 0 ? 20 : pageSize, 1, 100);

            var result = await service.GetResponsesAsync(surveyId, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithSummary("List responses for a survey");

        // GET: Single response by ID
        authGroup.MapGet("/{responseId:guid}", async (
            Guid surveyId,
            Guid responseId,
            ResponseService service,
            CancellationToken ct) =>
        {
            var response = await service.GetByIdAsync(responseId, ct);
            if (response is null) return Results.NotFound();
            // Verify the response belongs to the specified survey
            if (response.SurveyId != surveyId) return Results.NotFound();
            return Results.Ok(response);
        })
        .WithSummary("Get an individual response");
    }
}
