using FluentAssertions;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Validators;

namespace SurveyApi.UnitTests.Validators;

public class SurveyCreateValidatorTests
{
    private readonly SurveyCreateValidator _validator = new();

    [Fact]
    public void Valid_survey_passes_validation()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Customer Feedback Survey",
            Description = "Help us improve",
            Questions = new List<QuestionCreateRequest>
            {
                new()
                {
                    Text = "How satisfied are you?",
                    Type = "Rating",
                    IsRequired = true,
                    SortOrder = 0,
                    Settings = new QuestionSettingsRequest { MinRating = 1, MaxRating = 5 }
                }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_title_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "",
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "Q1", Type = "TextShort", IsRequired = true, SortOrder = 0 }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Title_exceeding_500_chars_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = new string('X', 501),
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "Q1", Type = "TextShort", IsRequired = true, SortOrder = 0 }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void No_questions_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test Survey",
            Questions = new List<QuestionCreateRequest>()
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Questions");
    }

    [Fact]
    public void More_than_100_questions_fails()
    {
        var questions = Enumerable.Range(0, 101).Select(i =>
            new QuestionCreateRequest { Text = $"Q{i}", Type = "TextShort", IsRequired = true, SortOrder = i }
        ).ToList();

        var request = new SurveyCreateRequest { Title = "Test", Questions = questions };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Question_without_text_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test",
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "", Type = "TextShort", IsRequired = true, SortOrder = 0 }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_question_type_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test",
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "Q1", Type = "InvalidType", IsRequired = true, SortOrder = 0 }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Choice_question_with_fewer_than_2_options_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test",
            Questions = new List<QuestionCreateRequest>
            {
                new()
                {
                    Text = "Pick one",
                    Type = "SingleChoice",
                    IsRequired = true,
                    SortOrder = 0,
                    Options = new List<QuestionOptionRequest>
                    {
                        new() { Text = "Only option", SortOrder = 0 }
                    }
                }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test",
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "Q1", Type = "TextShort", IsRequired = true, SortOrder = 0 }
            },
            Settings = new SurveySettingsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
