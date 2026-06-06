using SurveyApi.Domain.Entities;

namespace SurveyApi.Application.Interfaces;

/// <summary>
/// Repository for survey aggregate operations.
/// Abstracts data access from business logic.
/// </summary>
public interface ISurveyRepository
{
    /// <summary>Retrieve a paginated list of surveys, newest first.</summary>
    Task<PagedResult<Survey>> GetSurveysAsync(int page, int pageSize, SurveyStatus? statusFilter, CancellationToken ct = default);

    /// <summary>Retrieve a single survey with its questions and options.</summary>
    Task<Survey?> GetByIdWithQuestionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Retrieve a published survey by its public link ID (for respondents).</summary>
    Task<Survey?> GetByPublicLinkIdAsync(string publicLinkId, CancellationToken ct = default);

    /// <summary>Create a new survey.</summary>
    Task<Survey> CreateAsync(Survey survey, CancellationToken ct = default);

    /// <summary>Update an existing survey (title, description, questions).</summary>
    Task UpdateAsync(Survey survey, CancellationToken ct = default);

    /// <summary>Delete a survey and all related data.</summary>
    Task DeleteAsync(Survey survey, CancellationToken ct = default);

    /// <summary>Get the total response count for a survey.</summary>
    Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken ct = default);
}
