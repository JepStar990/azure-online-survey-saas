namespace SurveyApi.Domain.ValueObjects;

/// <summary>
/// Type-specific configuration for individual questions, stored as JSON.
/// The meaning of each property depends on the question type.
/// </summary>
public class QuestionSettings
{
    // --- Rating / NPS ---
    /// <summary>Minimum value on a rating scale (default 1).</summary>
    public int MinRating { get; set; } = 1;

    /// <summary>Maximum value on a rating scale (default 5 for Rating, 10 for NPS).</summary>
    public int MaxRating { get; set; } = 5;

    /// <summary>Label for the low end of the scale (e.g., "Not at all likely").</summary>
    public string? MinLabel { get; set; }

    /// <summary>Label for the high end of the scale (e.g., "Extremely likely").</summary>
    public string? MaxLabel { get; set; }

    // --- Text ---
    /// <summary>Maximum character length for text answers (default 500).</summary>
    public int? MaxLength { get; set; } = 500;

    /// <summary>Placeholder text shown in the text input.</summary>
    public string? Placeholder { get; set; }

    // --- Choice / Dropdown ---
    /// <summary>When true, options are displayed in random order per respondent.</summary>
    public bool RandomizeOptions { get; set; }

    /// <summary>When true, adds an "Other" option that allows free-text entry.</summary>
    public bool AllowOther { get; set; }

    // --- File Upload ---
    /// <summary>Allowed file extensions (e.g., ".pdf,.jpg,.png").</summary>
    public string? AllowedFileTypes { get; set; }

    /// <summary>Maximum file size in bytes.</summary>
    public long? MaxFileSizeBytes { get; set; }
}
