namespace SurveyApi.Application.DTOs;

/// <summary>
/// Aggregated analytics for a survey.
/// </summary>
public class AnalyticsSummaryDto
{
    public Guid SurveyId { get; init; }
    public string SurveyTitle { get; init; } = string.Empty;
    public int TotalResponses { get; init; }
    public int TotalQuestions { get; init; }
    public double CompletionRate { get; init; } // Percentage of started vs submitted
    public List<QuestionAnalyticsDto> QuestionBreakdowns { get; init; } = new();
}

/// <summary>Analytics for a single question.</summary>
public class QuestionAnalyticsDto
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public string QuestionType { get; init; } = string.Empty;
    public int ResponseCount { get; init; }

    /// <summary>For choice-based questions: count per option.</summary>
    public List<OptionCountDto> OptionCounts { get; init; } = new();

    /// <summary>For rating questions: average, min, max, distribution.</summary>
    public RatingSummaryDto? RatingSummary { get; init; }

    /// <summary>For text questions: sample responses.</summary>
    public List<string> TextSamples { get; init; } = new();
}

/// <summary>Response count for a single option in a choice question.</summary>
public class OptionCountDto
{
    public Guid OptionId { get; init; }
    public string OptionText { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percentage { get; init; }
}

/// <summary>Aggregated rating statistics.</summary>
public class RatingSummaryDto
{
    public double Average { get; init; }
    public int Min { get; init; }
    public int Max { get; init; }
    public int Median { get; init; }
    public Dictionary<int, int> Distribution { get; init; } = new(); // value -> count
}
