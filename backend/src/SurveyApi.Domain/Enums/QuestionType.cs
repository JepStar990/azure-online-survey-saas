namespace SurveyApi.Domain.Enums;

/// <summary>
/// Supported question types for survey questions.
/// </summary>
public enum QuestionType
{
    /// <summary>Select one option from a list.</summary>
    SingleChoice = 1,

    /// <summary>Select multiple options from a list.</summary>
    MultipleChoice = 2,

    /// <summary>Star or numeric rating scale (e.g., 1-5).</summary>
    Rating = 3,

    /// <summary>Net Promoter Score (0-10).</summary>
    Nps = 4,

    /// <summary>Short free-text response (single line).</summary>
    TextShort = 5,

    /// <summary>Long free-text response (multi-line).</summary>
    TextLong = 6,

    /// <summary>Date picker.</summary>
    Date = 7,

    /// <summary>Dropdown select.</summary>
    Dropdown = 8,

    /// <summary>Rank options in order of preference.</summary>
    Ranking = 9,

    /// <summary>File/image upload.</summary>
    FileUpload = 10
}
