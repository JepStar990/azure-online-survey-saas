namespace SurveyApi.Application.DTOs;

/// <summary>
/// Request to create a new survey with questions.
/// </summary>
public class SurveyCreateRequest
{
    /// <summary>Survey title (1-500 characters, required).</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional description (max 2000 characters).</summary>
    public string? Description { get; init; }

    /// <summary>Questions to include in the survey. Must have at least one.</summary>
    public List<QuestionCreateRequest> Questions { get; init; } = new();

    /// <summary>Optional survey settings.</summary>
    public SurveySettingsRequest? Settings { get; init; }
}

/// <summary>Request to create a question within a survey.</summary>
public class QuestionCreateRequest
{
    public string Text { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = "TextShort";
    public bool IsRequired { get; init; } = true;
    public int SortOrder { get; init; }
    public QuestionSettingsRequest? Settings { get; init; }
    public List<QuestionOptionRequest> Options { get; init; } = new();
}

/// <summary>Request to create/update a question option.</summary>
public class QuestionOptionRequest
{
    public string Text { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string? Value { get; init; }
}

/// <summary>Survey settings in create/update requests.</summary>
public class SurveySettingsRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool AllowAnonymous { get; init; }
    public int? ResponseLimit { get; init; }
    public string? ThankYouMessage { get; init; }
    public bool ShowProgressBar { get; init; } = true;
    public bool RandomizeQuestions { get; init; }
}

/// <summary>Question settings in create/update requests.</summary>
public class QuestionSettingsRequest
{
    public int MinRating { get; init; } = 1;
    public int MaxRating { get; init; } = 5;
    public string? MinLabel { get; init; }
    public string? MaxLabel { get; init; }
    public int? MaxLength { get; init; }
    public string? Placeholder { get; init; }
    public bool RandomizeOptions { get; init; }
    public bool AllowOther { get; init; }
}
