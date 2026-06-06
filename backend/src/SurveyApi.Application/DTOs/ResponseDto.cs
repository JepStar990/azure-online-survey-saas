using SurveyApi.Domain.Entities;

namespace SurveyApi.Application.DTOs;

/// <summary>
/// Response data returned to API consumers.
/// </summary>
public class ResponseDto
{
    public Guid Id { get; init; }
    public Guid SurveyId { get; init; }
    public Guid? RespondentId { get; init; }
    public ResponseStatus Status { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<ResponseAnswerDto> Answers { get; init; } = new();
}

/// <summary>Answer data within a response.</summary>
public class ResponseAnswerDto
{
    public Guid QuestionId { get; init; }
    public string? Value { get; init; }
    public List<Guid>? SelectedOptionIds { get; init; }
    public int? RatingValue { get; init; }
    public string? FileUrl { get; init; }
    public DateTime AnsweredAt { get; init; }
}
