using SurveyApi.Application.DTOs;

namespace SurveyApi.Application.Validators;

/// <summary>
/// Validates survey creation requests, ensuring they contain valid data and at least one question.
/// </summary>
public class SurveyCreateValidator : AbstractValidator<SurveyCreateRequest>
{
    // Valid question type names for string-to-enum parsing
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SingleChoice", "MultipleChoice", "Rating", "Nps",
        "TextShort", "TextLong", "Date", "Dropdown", "Ranking", "FileUpload"
    };

    public SurveyCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Survey title is required.")
            .MaximumLength(500).WithMessage("Survey title must not exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("A survey must have at least one question.")
            .Must(qs => qs.Count <= 100).WithMessage("A survey may have at most 100 questions.");

        RuleForEach(x => x.Questions).SetValidator(new QuestionCreateValidator());

        // If settings are provided, validate them
        When(x => x.Settings is not null, () =>
        {
            RuleFor(x => x.Settings!.ResponseLimit)
                .GreaterThanOrEqualTo(1).When(x => x.Settings!.ResponseLimit.HasValue)
                .WithMessage("Response limit must be at least 1.");

            RuleFor(x => x.Settings)
                .Must(s => !s!.EndDate.HasValue || !s.StartDate.HasValue || s.EndDate > s.StartDate)
                .WithMessage("End date must be after start date.");
        });
    }
}

/// <summary>Validates individual question creation requests.</summary>
public class QuestionCreateValidator : AbstractValidator<QuestionCreateRequest>
{
    public QuestionCreateValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Question type is required.")
            .Must(t => ValidTypes.Contains(t)).WithMessage("Invalid question type.");

        // Choice-based questions must have at least 2 options
        When(x => IsChoiceType(x.Type), () =>
        {
            RuleFor(x => x.Options)
                .Must(opts => opts.Count >= 2)
                .WithMessage("Choice-based questions must have at least 2 options.");

            RuleFor(x => x.Options)
                .Must(opts => opts.Count <= 50)
                .WithMessage("A question may have at most 50 options.");

            RuleForEach(x => x.Options)
                .ChildRules(opt =>
                {
                    opt.RuleFor(o => o.Text)
                        .NotEmpty().WithMessage("Option text is required.")
                        .MaximumLength(500).WithMessage("Option text must not exceed 500 characters.");
                });
        });

        // Rating/NPS must have valid ranges
        When(x => IsRatingType(x.Type), () =>
        {
            RuleFor(x => x.Settings!.MinRating)
                .LessThan(x => x.Settings!.MaxRating)
                .When(x => x.Settings is not null)
                .WithMessage("Min rating must be less than max rating.");
        });
    }

    private static bool IsChoiceType(string type) =>
        type is "SingleChoice" or "MultipleChoice" or "Dropdown" or "Ranking";

    private static bool IsRatingType(string type) =>
        type is "Rating" or "Nps";

    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SingleChoice", "MultipleChoice", "Rating", "Nps",
        "TextShort", "TextLong", "Date", "Dropdown", "Ranking", "FileUpload"
    };
}
