namespace SurveyApi.Application.DTOs;

/// <summary>
/// Request to update an existing survey's metadata and questions.
/// </summary>
public class SurveyUpdateRequest
{
    /// <summary>Updated title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Updated description.</summary>
    public string? Description { get; init; }

    /// <summary>Replacement questions list (full replacement).</summary>
    public List<QuestionCreateRequest> Questions { get; init; } = new();

    /// <summary>Updated settings.</summary>
    public SurveySettingsRequest? Settings { get; init; }
}
