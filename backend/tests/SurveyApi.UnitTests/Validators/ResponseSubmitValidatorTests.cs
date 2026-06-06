using FluentAssertions;
using SurveyApi.Application.DTOs;
using SurveyApi.Application.Validators;

namespace SurveyApi.UnitTests.Validators;

public class ResponseSubmitValidatorTests
{
    private readonly ResponseSubmitValidator _validator = new();

    [Fact]
    public void Valid_submission_passes()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new() { QuestionId = Guid.NewGuid(), Value = "Great product!" }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_answers_fails()
    {
        var request = new ResponseSubmitRequest { Answers = new List<AnswerRequest>() };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Answer_without_any_value_fails()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new() { QuestionId = Guid.NewGuid() } // No value, no rating, no selected options
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rating_out_of_range_fails()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new() { QuestionId = Guid.NewGuid(), RatingValue = 11 } // 0–10 only
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_rating_passes()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new() { QuestionId = Guid.NewGuid(), RatingValue = 8 }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_question_id_fails()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new() { QuestionId = Guid.Empty, Value = "test" }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Too_many_selected_options_fails()
    {
        var request = new ResponseSubmitRequest
        {
            Answers = new List<AnswerRequest>
            {
                new()
                {
                    QuestionId = Guid.NewGuid(),
                    SelectedOptionIds = Enumerable.Range(0, 51).Select(_ => Guid.NewGuid()).ToList()
                }
            }
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
