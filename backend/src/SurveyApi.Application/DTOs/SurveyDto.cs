using SurveyApi.Domain.Entities;
using SurveyApi.Domain.Enums;

namespace SurveyApi.Application.DTOs;

/// <summary>
/// Flat DTO returned to API consumers. Strips navigation properties.
/// </summary>
public class SurveyDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public SurveyStatus Status { get; init; }
    public string? PublicLinkId { get; init; }
    public string? PublicLinkUrl { get; init; } // Full URL constructed by the API
    public SurveySettingsDto Settings { get; init; } = new();
    public List<QuestionDto> Questions { get; init; } = new();
    public int ResponseCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}

/// <summary>Flattened survey settings for API consumers.</summary>
public class SurveySettingsDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool AllowAnonymous { get; init; }
    public int? ResponseLimit { get; init; }
    public string? ThankYouMessage { get; init; }
    public bool ShowProgressBar { get; init; } = true;
    public bool RandomizeQuestions { get; init; }
    public bool IsOpen { get; init; }
}
