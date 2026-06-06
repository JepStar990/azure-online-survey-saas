namespace SurveyApi.Domain.ValueObjects;

/// <summary>
/// Configuration settings for a survey, stored as JSON in the database.
/// Controls visibility window, anonymity, response limits, and post-submission messaging.
/// </summary>
public class SurveySettings
{
    /// <summary>Optional start date; survey is hidden from respondents before this time.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Optional end date; survey stops accepting responses after this time.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>When true, responses can be submitted without authentication.</summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>Maximum number of responses before the survey auto-closes. Null = unlimited.</summary>
    public int? ResponseLimit { get; set; }

    /// <summary>Message displayed to respondents after successful submission.</summary>
    public string? ThankYouMessage { get; set; } = "Thank you for completing this survey!";

    /// <summary>When true, shows a progress indicator to respondents.</summary>
    public bool ShowProgressBar { get; set; } = true;

    /// <summary>When true, randomizes the order of questions for each respondent.</summary>
    public bool RandomizeQuestions { get; set; }

    /// <summary>When true, allows respondents to save a draft and return later.</summary>
    public bool AllowSaveDraft { get; set; }

    /// <summary>Whether the survey is currently accepting responses (derived from status + dates).</summary>
    public bool IsOpen(SurveyStatus status)
    {
        if (status != SurveyStatus.Published) return false;
        var now = DateTime.UtcNow;
        if (StartDate.HasValue && now < StartDate.Value) return false;
        if (EndDate.HasValue && now > EndDate.Value) return false;
        return true;
    }
}
