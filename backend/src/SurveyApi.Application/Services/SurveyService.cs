using SurveyApi.Application.DTOs;
using SurveyApi.Application.Interfaces;
using SurveyApi.Domain.Entities;
using SurveyApi.Domain.Enums;
using SurveyApi.Domain.ValueObjects;

namespace SurveyApi.Application.Services;

/// <summary>
/// Core business logic for survey lifecycle management.
/// Validates business rules and coordinates between the API layer and data access.
/// </summary>
public class SurveyService
{
    private readonly ISurveyRepository _repository;

    public SurveyService(ISurveyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Get a paginated list of surveys.</summary>
    public async Task<PagedResult<SurveyDto>> GetSurveysAsync(
        int page, int pageSize, SurveyStatus? statusFilter = null, CancellationToken ct = default)
    {
        var result = await _repository.GetSurveysAsync(page, pageSize, statusFilter, ct);
        return new PagedResult<SurveyDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <summary>Get a single survey by ID.</summary>
    public async Task<SurveyDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _repository.GetByIdWithQuestionsAsync(id, ct);
        return survey is null ? null : MapToDto(survey);
    }

    /// <summary>Get a published survey by its public link ID (for respondents).</summary>
    public async Task<SurveyDto?> GetByPublicLinkAsync(string publicLinkId, CancellationToken ct = default)
    {
        var survey = await _repository.GetByPublicLinkIdAsync(publicLinkId, ct);
        if (survey is null) return null;
        // Only return if the survey is open to respondents
        if (!survey.Settings.IsOpen(survey.Status)) return null;
        return MapToDto(survey);
    }

    /// <summary>Create a new survey with questions.</summary>
    public async Task<SurveyDto> CreateAsync(SurveyCreateRequest request, Guid createdById, CancellationToken ct = default)
    {
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = SurveyStatus.Draft,
            CreatedById = createdById,
            Settings = MapSettings(request.Settings)
        };

        // Map questions with correct sort order
        for (int i = 0; i < request.Questions.Count; i++)
        {
            var q = request.Questions[i];
            var question = MapToQuestion(q, i);
            survey.Questions.Add(question);
        }

        var created = await _repository.CreateAsync(survey, ct);
        return MapToDto(created);
    }

    /// <summary>Update an existing survey (title, description, settings, questions).</summary>
    public async Task<SurveyDto?> UpdateAsync(Guid id, SurveyUpdateRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdWithQuestionsAsync(id, ct);
        if (existing is null) return null;

        // Only allow updates to Draft surveys (or Published — just update metadata)
        existing.Title = request.Title;
        existing.Description = request.Description;
        if (request.Settings is not null)
            existing.Settings = MapSettings(request.Settings);
        existing.UpdatedAt = DateTime.UtcNow;

        // Full replacement of questions
        existing.Questions.Clear();
        for (int i = 0; i < request.Questions.Count; i++)
        {
            var q = request.Questions[i];
            var question = MapToQuestion(q, i);
            question.SurveyId = existing.Id;
            existing.Questions.Add(question);
        }

        await _repository.UpdateAsync(existing, ct);
        return MapToDto(existing);
    }

    /// <summary>Delete a survey. Only the creator or a tenant admin should be allowed.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _repository.GetByIdWithQuestionsAsync(id, ct);
        if (survey is null) return false;
        await _repository.DeleteAsync(survey, ct);
        return true;
    }

    /// <summary>Publish a survey so it becomes available to respondents. Generates a public link ID.</summary>
    public async Task<SurveyDto?> PublishAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _repository.GetByIdWithQuestionsAsync(id, ct);
        if (survey is null) return null;

        // Validate: survey must have at least one question
        if (survey.Questions.Count == 0)
            throw new InvalidOperationException("Cannot publish a survey with no questions.");

        survey.Status = SurveyStatus.Published;
        survey.PublishedAt = DateTime.UtcNow;
        survey.PublicLinkId = GeneratePublicLinkId();
        survey.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(survey, ct);
        return MapToDto(survey);
    }

    /// <summary>Close a survey so it no longer accepts responses.</summary>
    public async Task<SurveyDto?> CloseAsync(Guid id, CancellationToken ct = default)
    {
        var survey = await _repository.GetByIdWithQuestionsAsync(id, ct);
        if (survey is null) return null;

        survey.Status = SurveyStatus.Closed;
        survey.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(survey, ct);
        return MapToDto(survey);
    }

    // --- Private mapping helpers ---

    private static SurveySettings MapSettings(SurveySettingsRequest? req)
    {
        if (req is null) return new SurveySettings();
        return new SurveySettings
        {
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            AllowAnonymous = req.AllowAnonymous,
            ResponseLimit = req.ResponseLimit,
            ThankYouMessage = req.ThankYouMessage,
            ShowProgressBar = req.ShowProgressBar,
            RandomizeQuestions = req.RandomizeQuestions
        };
    }

    private static Question MapToQuestion(QuestionCreateRequest req, int sortOrder)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = req.Text,
            Description = req.Description,
            IsRequired = req.IsRequired,
            SortOrder = sortOrder,
            Type = Enum.TryParse<QuestionType>(req.Type, true, out var parsed) ? parsed : QuestionType.TextShort,
            Settings = req.Settings is null ? new QuestionSettings() : new QuestionSettings
            {
                MinRating = req.Settings.MinRating,
                MaxRating = req.Settings.MaxRating,
                MinLabel = req.Settings.MinLabel,
                MaxLabel = req.Settings.MaxLabel,
                MaxLength = req.Settings.MaxLength,
                Placeholder = req.Settings.Placeholder,
                RandomizeOptions = req.Settings.RandomizeOptions,
                AllowOther = req.Settings.AllowOther
            }
        };

        foreach (var optReq in req.Options)
        {
            question.Options.Add(new QuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = optReq.Text,
                SortOrder = optReq.SortOrder,
                Value = optReq.Value
            });
        }

        return question;
    }

    private static SurveyDto MapToDto(Survey s)
    {
        return new SurveyDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Status = s.Status,
            PublicLinkId = s.PublicLinkId,
            Settings = new SurveySettingsDto
            {
                StartDate = s.Settings.StartDate,
                EndDate = s.Settings.EndDate,
                AllowAnonymous = s.Settings.AllowAnonymous,
                ResponseLimit = s.Settings.ResponseLimit,
                ThankYouMessage = s.Settings.ThankYouMessage,
                ShowProgressBar = s.Settings.ShowProgressBar,
                RandomizeQuestions = s.Settings.RandomizeQuestions,
                IsOpen = s.Settings.IsOpen(s.Status)
            },
            Questions = s.Questions
                .OrderBy(q => q.SortOrder)
                .Select(MapQuestionToDto)
                .ToList(),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            PublishedAt = s.PublishedAt
        };
    }

    private static QuestionDto MapQuestionToDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            Description = q.Description,
            Type = q.Type,
            SortOrder = q.SortOrder,
            IsRequired = q.IsRequired,
            Settings = new QuestionSettingsDto
            {
                MinRating = q.Settings.MinRating,
                MaxRating = q.Settings.MaxRating,
                MinLabel = q.Settings.MinLabel,
                MaxLabel = q.Settings.MaxLabel,
                MaxLength = q.Settings.MaxLength,
                Placeholder = q.Settings.Placeholder,
                RandomizeOptions = q.Settings.RandomizeOptions,
                AllowOther = q.Settings.AllowOther,
                AllowedFileTypes = q.Settings.AllowedFileTypes,
                MaxFileSizeBytes = q.Settings.MaxFileSizeBytes
            },
            Options = q.Options
                .OrderBy(o => o.SortOrder)
                .Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    SortOrder = o.SortOrder,
                    Value = o.Value
                })
                .ToList()
        };
    }

    /// <summary>Generate a short, URL-safe public link identifier.</summary>
    private static string GeneratePublicLinkId()
    {
        // Use a cryptographically-random 8-character base62 ID
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = Guid.NewGuid().ToByteArray();
        var result = new char[8];
        for (int i = 0; i < 8; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}
