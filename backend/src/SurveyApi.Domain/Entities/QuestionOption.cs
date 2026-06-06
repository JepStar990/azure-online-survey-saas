namespace SurveyApi.Domain.Entities;

/// <summary>
/// An option within a choice-based question (SingleChoice, MultipleChoice, Dropdown, Ranking).
/// </summary>
public class QuestionOption
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The question this option belongs to.</summary>
    public Guid QuestionId { get; set; }

    /// <summary>Display text for this option.</summary>
    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Display order within the options list.</summary>
    public int SortOrder { get; set; }

    /// <summary>Optional machine-readable value for scoring or analysis (e.g., "1", "satisfied").</summary>
    [MaxLength(100)]
    public string? Value { get; set; }

    // --- Navigation ---

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
}
