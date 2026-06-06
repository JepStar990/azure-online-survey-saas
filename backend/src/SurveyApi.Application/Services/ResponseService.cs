using System.Text.Json;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Interfaces;
using SurveyApi.Domain.Entities;

namespace SurveyApi.Application.Services;

/// <summary>
/// Handles response submission and retrieval for surveys.
/// Validates that submissions meet survey requirements and rules.
/// </summary>
public class ResponseService
{
    private readonly IResponseRepository _responseRepository;
    private readonly ISurveyRepository _surveyRepository;

    public ResponseService(IResponseRepository responseRepository, ISurveyRepository surveyRepository)
    {
        _responseRepository = responseRepository;
        _surveyRepository = surveyRepository;
    }

    /// <summary>Submit a response to a survey accessed by public link.</summary>
    public async Task<ResponseDto> SubmitAsync(
        string publicLinkId,
        ResponseSubmitRequest request,
        Guid? respondentId,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct = default)
    {
        // Validate survey exists and is open
        var survey = await _surveyRepository.GetByPublicLinkIdAsync(publicLinkId, ct);
        if (survey is null)
            throw new InvalidOperationException("Survey not found.");

        if (!survey.Settings.IsOpen(survey.Status))
            throw new InvalidOperationException("This survey is no longer accepting responses.");

        // Check response limit
        if (survey.Settings.ResponseLimit.HasValue)
        {
            var hasReached = await _responseRepository.HasReachedResponseLimitAsync(
                survey.Id, survey.Settings.ResponseLimit.Value, ct);
            if (hasReached)
                throw new InvalidOperationException("This survey has reached its response limit.");
        }

        // Validate all required questions are answered
        ValidateRequiredQuestions(survey, request);

        // Build response entity
        var response = new Response
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            RespondentId = respondentId,
            Status = ResponseStatus.Submitted,
            StartedAt = DateTime.UtcNow, // In production, track actual start time
            CompletedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        foreach (var answerReq in request.Answers)
        {
            var answer = new ResponseAnswer
            {
                Id = Guid.NewGuid(),
                ResponseId = response.Id,
                QuestionId = answerReq.QuestionId,
                Value = answerReq.Value,
                SelectedOptionIds = answerReq.SelectedOptionIds is { Count: > 0 }
                    ? JsonSerializer.Serialize(answerReq.SelectedOptionIds)
                    : null,
                RatingValue = answerReq.RatingValue,
                AnsweredAt = DateTime.UtcNow
            };
            response.Answers.Add(answer);
        }

        var saved = await _responseRepository.SubmitAsync(response, ct);
        return MapToDto(saved);
    }

    /// <summary>Get paginated responses for a survey.</summary>
    public async Task<PagedResult<ResponseDto>> GetResponsesAsync(
        Guid surveyId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _responseRepository.GetResponsesAsync(surveyId, page, pageSize, ct);
        return new PagedResult<ResponseDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <summary>Get a single response by ID.</summary>
    public async Task<ResponseDto?> GetByIdAsync(Guid responseId, CancellationToken ct = default)
    {
        var response = await _responseRepository.GetByIdWithAnswersAsync(responseId, ct);
        return response is null ? null : MapToDto(response);
    }

    // --- Private helpers ---

    private static void ValidateRequiredQuestions(Survey survey, ResponseSubmitRequest request)
    {
        var requiredQuestionIds = survey.Questions
            .Where(q => q.IsRequired)
            .Select(q => q.Id)
            .ToHashSet();

        var answeredQuestionIds = request.Answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Value)
                        || a.SelectedOptionIds is { Count: > 0 }
                        || a.RatingValue.HasValue)
            .Select(a => a.QuestionId)
            .ToHashSet();

        var missing = requiredQuestionIds.Except(answeredQuestionIds).ToList();
        if (missing.Count > 0)
        {
            var missingTexts = survey.Questions
                .Where(q => missing.Contains(q.Id))
                .Select(q => q.Text);
            throw new InvalidOperationException(
                $"The following required questions have not been answered: {string.Join(", ", missingTexts)}");
        }
    }

    private static ResponseDto MapToDto(Response r)
    {
        return new ResponseDto
        {
            Id = r.Id,
            SurveyId = r.SurveyId,
            RespondentId = r.RespondentId,
            Status = r.Status,
            StartedAt = r.StartedAt,
            CompletedAt = r.CompletedAt,
            Answers = r.Answers.Select(a => new ResponseAnswerDto
            {
                QuestionId = a.QuestionId,
                Value = a.Value,
                SelectedOptionIds = a.SelectedOptionIds is not null
                    ? JsonSerializer.Deserialize<List<Guid>>(a.SelectedOptionIds)
                    : null,
                RatingValue = a.RatingValue,
                FileUrl = a.FileUrl,
                AnsweredAt = a.AnsweredAt
            }).ToList()
        };
    }
}
