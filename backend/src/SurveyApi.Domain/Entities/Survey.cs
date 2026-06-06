namespace SurveyApi.Domain.Entities;

/// <summary>
/// Represents a survey created by a user.
/// A survey contains questions and collects responses.
/// </summary>
public class Survey
{
    /// <summary>Unique identifier. Uses GUID for multi-tenancy readiness.</summary>
    public Guid Id { get; set; }

    /// <summary>Survey title shown to respondents and in listings.</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional longer description explaining the survey purpose.</summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Current lifecycle status.</summary>
    public SurveyStatus Status { get; set; } = SurveyStatus.Draft;

    /// <summary>
    /// Short, URL-safe identifier used to create a public shareable link.
    /// Generated automatically when the survey is published.
    /// </summary>
    [MaxLength(12)]
    public string? PublicLinkId { get; set; }

    /// <summary>
    /// The Azure AD object ID of the user who created this survey.
    /// Used for ownership checks.
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>Survey configuration stored as JSON (dates, anonymity, limits, etc.).</summary>
    public SurveySettings Settings { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // --- Navigation properties ---

    /// <summary>Questions belonging to this survey, in display order.</summary>
    public List<Question> Questions { get; set; } = new();

    /// <summary>Responses submitted for this survey.</summary>
    public List<Response> Responses { get; set; } = new();
}
