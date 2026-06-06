using System.Text.Json;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Interfaces;

namespace SurveyApi.Application.Services;

/// <summary>
/// Computes analytics and aggregations for survey responses.
/// </summary>
public class AnalyticsService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IResponseRepository _responseRepository;

    public AnalyticsService(ISurveyRepository surveyRepository, IResponseRepository responseRepository)
    {
        _surveyRepository = surveyRepository;
        _responseRepository = responseRepository;
    }

    /// <summary>Generate a summary analytics report for a survey.</summary>
    public async Task<AnalyticsSummaryDto?> GetSummaryAsync(Guid surveyId, CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetByIdWithQuestionsAsync(surveyId, ct);
        if (survey is null) return null;

        var responses = await _responseRepository.GetResponsesAsync(surveyId, 1, int.MaxValue, ct);
        var totalCount = await _responseRepository.GetSubmittedCountAsync(surveyId, ct);

        var questionBreakdowns = survey.Questions
            .OrderBy(q => q.SortOrder)
            .Select(q => BuildQuestionAnalytics(q, responses.Items))
            .ToList();

        return new AnalyticsSummaryDto
        {
            SurveyId = survey.Id,
            SurveyTitle = survey.Title,
            TotalResponses = totalCount,
            TotalQuestions = survey.Questions.Count,
            CompletionRate = responses.TotalCount > 0
                ? (double)totalCount / responses.TotalCount * 100
                : 0,
            QuestionBreakdowns = questionBreakdowns
        };
    }

    private static QuestionAnalyticsDto BuildQuestionAnalytics(
        Domain.Entities.Question question, List<Domain.Entities.Response> responses)
    {
        // Collect answers for this question across all responses
        var answers = responses
            .SelectMany(r => r.Answers)
            .Where(a => a.QuestionId == question.Id)
            .ToList();

        // --- Compute option counts ---
        List<OptionCountDto> optionCounts;
        if (question.Options.Count > 0)
        {
            var counts = new Dictionary<Guid, int>();
            foreach (var opt in question.Options)
                counts[opt.Id] = 0;

            foreach (var answer in answers)
            {
                if (answer.SelectedOptionIds is not null)
                {
                    var selectedIds = JsonSerializer.Deserialize<List<Guid>>(answer.SelectedOptionIds);
                    if (selectedIds is not null)
                    {
                        foreach (var id in selectedIds)
                            if (counts.ContainsKey(id))
                                counts[id]++;
                    }
                }
            }

            var totalSelections = counts.Values.Sum();
            optionCounts = question.Options
                .OrderBy(o => o.SortOrder)
                .Select(o => new OptionCountDto
                {
                    OptionId = o.Id,
                    OptionText = o.Text,
                    Count = counts.GetValueOrDefault(o.Id, 0),
                    Percentage = totalSelections > 0
                        ? Math.Round((double)counts.GetValueOrDefault(o.Id, 0) / totalSelections * 100, 1)
                        : 0
                })
                .ToList();
        }
        else
        {
            optionCounts = new List<OptionCountDto>();
        }

        // --- Compute rating summary ---
        RatingSummaryDto? ratingSummary = null;
        var ratings = answers
            .Where(a => a.RatingValue.HasValue)
            .Select(a => a.RatingValue!.Value)
            .ToList();

        if (ratings.Count > 0)
        {
            var sorted = ratings.OrderBy(r => r).ToList();
            ratingSummary = new RatingSummaryDto
            {
                Average = Math.Round(ratings.Average(), 2),
                Min = ratings.Min(),
                Max = ratings.Max(),
                Median = sorted.Count % 2 == 0
                    ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2
                    : sorted[sorted.Count / 2],
                Distribution = ratings.GroupBy(r => r)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        // --- Text samples ---
        var textSamples = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Value))
            .Select(a => a.Value!)
            .Take(5)
            .ToList();

        // Construct DTO in a single initializer to respect init-only properties
        return new QuestionAnalyticsDto
        {
            QuestionId = question.Id,
            QuestionText = question.Text,
            QuestionType = question.Type.ToString(),
            ResponseCount = answers.Count,
            OptionCounts = optionCounts,
            RatingSummary = ratingSummary,
            TextSamples = textSamples
        };
    }
}
