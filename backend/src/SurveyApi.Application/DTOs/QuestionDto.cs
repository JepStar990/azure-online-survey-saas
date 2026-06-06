using SurveyApi.Domain.Entities;
using SurveyApi.Domain.Enums;

namespace SurveyApi.Application.DTOs;

/// <summary>
/// Question data returned to API consumers.
/// </summary>
public class QuestionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? Description { get; init; }
    public QuestionType Type { get; init; }
    public int SortOrder { get; init; }
    public bool IsRequired { get; init; }
    public QuestionSettingsDto Settings { get; init; } = new();
    public List<QuestionOptionDto> Options { get; init; } = new();
}

/// <summary>Flattened question settings.</summary>
public class QuestionSettingsDto
{
    public int MinRating { get; init; } = 1;
    public int MaxRating { get; init; } = 5;
    public string? MinLabel { get; init; }
    public string? MaxLabel { get; init; }
    public int? MaxLength { get; init; }
    public string? Placeholder { get; init; }
    public bool RandomizeOptions { get; init; }
    public bool AllowOther { get; init; }
    public string? AllowedFileTypes { get; init; }
    public long? MaxFileSizeBytes { get; init; }
}

/// <summary>Question option data.</summary>
public class QuestionOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string? Value { get; init; }
}
