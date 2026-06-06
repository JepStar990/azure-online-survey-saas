using SurveyApi.Domain.Entities;

namespace SurveyApi.Application.Interfaces;

/// <summary>
/// Repository for survey response operations.
/// </summary>
public interface IResponseRepository
{
    /// <summary>Submit a new response (or update a draft).</summary>
    Task<Response> SubmitAsync(Response response, CancellationToken ct = default);

    /// <summary>Get paginated responses for a survey, newest first.</summary>
    Task<PagedResult<Response>> GetResponsesAsync(Guid surveyId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Get a single response with all answers.</summary>
    Task<Response?> GetByIdWithAnswersAsync(Guid responseId, CancellationToken ct = default);

    /// <summary>Get the count of submitted responses for a survey.</summary>
    Task<int> GetSubmittedCountAsync(Guid surveyId, CancellationToken ct = default);

    /// <summary>Check if a survey has reached its response limit.</summary>
    Task<bool> HasReachedResponseLimitAsync(Guid surveyId, int limit, CancellationToken ct = default);
}
