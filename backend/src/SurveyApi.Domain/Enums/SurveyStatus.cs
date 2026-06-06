namespace SurveyApi.Domain.Enums;

/// <summary>
/// Lifecycle status of a survey.
/// </summary>
public enum SurveyStatus
{
    /// <summary>Survey is being edited and not visible to respondents.</summary>
    Draft = 0,

    /// <summary>Survey is live and accepting responses.</summary>
    Published = 1,

    /// <summary>Survey is no longer accepting responses.</summary>
    Closed = 2
}
