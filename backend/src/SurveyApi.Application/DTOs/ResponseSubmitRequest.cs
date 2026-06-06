namespace SurveyApi.Application.DTOs;

/// <summary>
/// Request to submit a survey response. Contains answers for each question.
/// </summary>
public class ResponseSubmitRequest
{
    /// <summary>Answers for each question in the survey.</summary>
    public List<AnswerRequest> Answers { get; init; } = new();
}

/// <summary>Answer for a single question.</summary>
public class AnswerRequest
{
    /// <summary>The question being answered.</summary>
    public Guid QuestionId { get; init; }

    /// <summary>Free-text value (TextShort, TextLong, Date, Dropdown).</summary>
    public string? Value { get; init; }

    /// <summary>Selected option GUIDs (MultipleChoice, Ranking).</summary>
    public List<Guid>? SelectedOptionIds { get; init; }

    /// <summary>Numeric rating (Rating, NPS).</summary>
    public int? RatingValue { get; init; }
}
