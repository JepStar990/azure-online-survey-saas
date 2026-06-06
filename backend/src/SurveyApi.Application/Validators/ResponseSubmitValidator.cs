using SurveyApi.Application.DTOs;

namespace SurveyApi.Application.Validators;

/// <summary>
/// Validates survey response submissions.
/// Ensures answers reference valid question IDs and contain appropriate data for their type.
/// </summary>
public class ResponseSubmitValidator : AbstractValidator<ResponseSubmitRequest>
{
    public ResponseSubmitValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers).SetValidator(new AnswerRequestValidator());
    }
}

/// <summary>Validates individual answer requests.</summary>
public class AnswerRequestValidator : AbstractValidator<AnswerRequest>
{
    public AnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required for each answer.");

        // At least one answer field must be populated
        RuleFor(x => x)
            .Must(a => !string.IsNullOrWhiteSpace(a.Value)
                       || a.SelectedOptionIds is { Count: > 0 }
                       || a.RatingValue.HasValue)
            .WithMessage("Each answer must provide a value, selected options, or a rating.");

        // Rating value range check (validated against question settings at the service level)
        RuleFor(x => x.RatingValue)
            .InclusiveBetween(0, 10)
            .When(x => x.RatingValue.HasValue)
            .WithMessage("Rating must be between 0 and 10.");

        // Text length check (further validation at service level based on question settings)
        RuleFor(x => x.Value)
            .MaximumLength(5000)
            .When(x => x.Value is not null)
            .WithMessage("Text answers must not exceed 5000 characters.");

        // Option IDs must not be empty GUIDs
        When(x => x.SelectedOptionIds is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.SelectedOptionIds)
                .Must(id => id != Guid.Empty)
                .WithMessage("Invalid option ID.");

            RuleFor(x => x.SelectedOptionIds!.Count)
                .LessThanOrEqualTo(50)
                .WithMessage("Too many options selected.");
        });
    }
}
