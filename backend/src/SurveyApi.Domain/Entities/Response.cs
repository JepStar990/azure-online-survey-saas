namespace SurveyApi.Domain.Entities;

/// <summary>
/// A single response submission for a survey.
/// Tracks metadata about who submitted and when, plus the collection of answer values.
/// </summary>
public class Response
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The survey this response belongs to.</summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Azure AD object ID of the authenticated respondent.
    /// Null for anonymous responses.
    /// </summary>
    public Guid? RespondentId { get; set; }

    /// <summary>Status of this response: InProgress (draft) or Submitted.</summary>
    public ResponseStatus Status { get; set; } = ResponseStatus.InProgress;

    /// <summary>When the respondent first opened the survey.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the respondent submitted the final response.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>IP address of the respondent, for rate limiting and abuse detection.</summary>
    [MaxLength(45)] // Supports IPv6
    public string? IpAddress { get; set; }

    /// <summary>User agent string of the respondent's browser.</summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // --- Navigation ---

    [ForeignKey(nameof(SurveyId))]
    public Survey? Survey { get; set; }

    /// <summary>The answer values collected for this response.</summary>
    public List<ResponseAnswer> Answers { get; set; } = new();
}

/// <summary>
/// Status of a survey response.
/// </summary>
public enum ResponseStatus
{
    /// <summary>Response has been started but not yet submitted (draft).</summary>
    InProgress = 0,

    /// <summary>Response has been finalized and submitted.</summary>
    Submitted = 1
}
