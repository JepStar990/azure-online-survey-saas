using Microsoft.AspNetCore.Mvc;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Services;
using SurveyApi.Application.Validators;
using SurveyApi.Infrastructure.Auth;

namespace SurveyApi.Web.Endpoints;

/// <summary>
/// Endpoints for survey CRUD operations and lifecycle management.
/// </summary>
public static class SurveyEndpoints
{
    public static void MapSurveyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/surveys")
            .RequireAuthorization()
            .WithTags("Surveys");

        // --- GET: List surveys (paginated) ---
        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? status,
            SurveyService service,
            CancellationToken ct) =>
        {
            page = Math.Max(1, page == 0 ? 1 : page);
            pageSize = Math.Clamp(pageSize == 0 ? 20 : pageSize, 1, 100);

            Domain.Enums.SurveyStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Domain.Enums.SurveyStatus>(status, true, out var parsed))
                statusFilter = parsed;

            var result = await service.GetSurveysAsync(page, pageSize, statusFilter, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireSurveyViewer")
        .WithSummary("List surveys");

        // --- GET: Single survey by ID ---
        group.MapGet("/{id:guid}", async (
            Guid id,
            SurveyService service,
            CancellationToken ct) =>
        {
            var survey = await service.GetByIdAsync(id, ct);
            return survey is not null ? Results.Ok(survey) : Results.NotFound();
        })
        .RequireAuthorization("RequireSurveyViewer")
        .WithSummary("Get survey by ID");

        // --- POST: Create a new survey ---
        group.MapPost("/", async (
            [FromBody] SurveyCreateRequest request,
            SurveyCreateValidator validator,
            SurveyService service,
            CurrentUserService currentUser,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var userId = currentUser.UserId;
            if (userId is null)
                return Results.Unauthorized();

            var survey = await service.CreateAsync(request, userId.Value, ct);
            return Results.Created($"/api/v1/surveys/{survey.Id}", survey);
        })
        .RequireAuthorization("RequireSurveyCreator")
        .WithSummary("Create a new survey");

        // --- PUT: Update an existing survey ---
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] SurveyUpdateRequest request,
            SurveyCreateValidator validator,
            SurveyService service,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(new SurveyCreateRequest
            {
                Title = request.Title,
                Description = request.Description,
                Questions = request.Questions,
                Settings = request.Settings
            }, ct);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var updated = await service.UpdateAsync(id, request, ct);
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        })
        .RequireAuthorization("RequireSurveyCreator")
        .WithSummary("Update a survey");

        // --- DELETE: Delete a survey ---
        group.MapDelete("/{id:guid}", async (
            Guid id,
            SurveyService service,
            CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("RequireSurveyCreator")
        .WithSummary("Delete a survey");

        // --- POST: Publish a survey ---
        group.MapPost("/{id:guid}/publish", async (
            Guid id,
            SurveyService service,
            CancellationToken ct) =>
        {
            try
            {
                var published = await service.PublishAsync(id, ct);
                return published is not null ? Results.Ok(published) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("RequireSurveyCreator")
        .WithSummary("Publish a survey");

        // --- POST: Close a survey ---
        group.MapPost("/{id:guid}/close", async (
            Guid id,
            SurveyService service,
            CancellationToken ct) =>
        {
            var closed = await service.CloseAsync(id, ct);
            return closed is not null ? Results.Ok(closed) : Results.NotFound();
        })
        .RequireAuthorization("RequireSurveyCreator")
        .WithSummary("Close a survey");
    }
}
