namespace SurveyApi.Domain.Entities;

/// <summary>
/// A single question within a survey.
/// Each question has a type that determines how it is rendered and what kind of answer it accepts.
/// </summary>
public class Question
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The survey this question belongs to.</summary>
    public Guid SurveyId { get; set; }

    /// <summary>The question text displayed to the respondent.</summary>
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional sub-text providing context or instructions.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Determines the input type and validation rules.</summary>
    public QuestionType Type { get; set; } = QuestionType.TextShort;

    /// <summary>Display order within the survey (0-based).</summary>
    public int SortOrder { get; set; }

    /// <summary>When true, the respondent must answer this question before submitting.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Type-specific configuration stored as JSON.</summary>
    public QuestionSettings Settings { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation properties ---

    /// <summary>Parent survey.</summary>
    [ForeignKey(nameof(SurveyId))]
    public Survey? Survey { get; set; }

    /// <summary>Options for choice-based questions (SingleChoice, MultipleChoice, Dropdown, Ranking).</summary>
    public List<QuestionOption> Options { get; set; } = new();
}
