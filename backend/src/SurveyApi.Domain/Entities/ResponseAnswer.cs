namespace SurveyApi.Domain.Entities;

/// <summary>
/// The answer value for a single question within a survey response.
/// Different fields are populated depending on the question type.
/// </summary>
public class ResponseAnswer
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The parent response.</summary>
    public Guid ResponseId { get; set; }

    /// <summary>The question being answered.</summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Free-text value (used for TextShort, TextLong, Date, Dropdown single-select).
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// JSON array of selected option GUIDs (used for MultipleChoice, Ranking).
    /// </summary>
    public string? SelectedOptionIds { get; set; }

    /// <summary>
    /// Numeric rating value (used for Rating, NPS).
    /// </summary>
    public int? RatingValue { get; set; }

    /// <summary>
    /// URL to an uploaded file (used for FileUpload questions).
    /// </summary>
    [MaxLength(1000)]
    public string? FileUrl { get; set; }

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    // --- Navigation ---

    [ForeignKey(nameof(ResponseId))]
    public Response? Response { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
}
