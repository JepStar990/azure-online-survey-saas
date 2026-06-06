using SurveyApi.Domain.Entities;
using SurveyApi.Domain.Enums;
using SurveyApi.Domain.ValueObjects;

namespace SurveyApi.Infrastructure.Data;

/// <summary>
/// Seeds the database with demo surveys for development and testing.
/// Only runs when the database has no existing surveys.
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeedDataAsync(AppDbContext db)
    {
        if (db.Surveys.Any())
            return;

        var surveys = new List<Survey>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Customer Satisfaction Q1 2026",
                Description = "Quarterly customer satisfaction survey to gauge product sentiment.",
                Status = SurveyStatus.Published,
                PublicLinkId = GenerateLinkId(),
                PublishedAt = DateTime.UtcNow.AddDays(-7),
                Settings = new SurveySettings
                {
                    AllowAnonymous = true,
                    ShowProgressBar = true,
                    ThankYouMessage = "Thanks for helping us improve!"
                },
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "How satisfied are you with our product overall?",
                        Type = QuestionType.Rating,
                        SortOrder = 0,
                        IsRequired = true,
                        Settings = new QuestionSettings { MinRating = 1, MaxRating = 5, MinLabel = "Very dissatisfied", MaxLabel = "Very satisfied" }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "Which features do you use most?",
                        Type = QuestionType.MultipleChoice,
                        SortOrder = 1,
                        IsRequired = true,
                        Options = new List<QuestionOption>
                        {
                            new() { Id = Guid.NewGuid(), Text = "Dashboard", SortOrder = 0 },
                            new() { Id = Guid.NewGuid(), Text = "Reporting", SortOrder = 1 },
                            new() { Id = Guid.NewGuid(), Text = "API Integrations", SortOrder = 2 },
                            new() { Id = Guid.NewGuid(), Text = "Mobile App", SortOrder = 3 }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "What could we improve?",
                        Description = "Your honest feedback helps us prioritize.",
                        Type = QuestionType.TextLong,
                        SortOrder = 2,
                        IsRequired = false
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Employee Engagement Pulse",
                Description = "Monthly pulse check on team morale and engagement.",
                Status = SurveyStatus.Published,
                PublicLinkId = GenerateLinkId(),
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                Settings = new SurveySettings
                {
                    AllowAnonymous = false,
                    ShowProgressBar = true,
                    ThankYouMessage = "Your voice matters. Thank you!"
                },
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "How likely are you to recommend our company as a great place to work?",
                        Type = QuestionType.Nps,
                        SortOrder = 0,
                        IsRequired = true,
                        Settings = new QuestionSettings { MinRating = 0, MaxRating = 10, MinLabel = "Not at all likely", MaxLabel = "Extremely likely" }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "What's one thing we should start doing?",
                        Type = QuestionType.TextShort,
                        SortOrder = 1,
                        IsRequired = false
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product Launch Feedback",
                Description = "Collecting early feedback on the new feature release.",
                Status = SurveyStatus.Draft,
                Settings = new SurveySettings { AllowAnonymous = true, ShowProgressBar = true },
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "Which plan do you intend to subscribe to?",
                        Type = QuestionType.SingleChoice,
                        SortOrder = 0,
                        IsRequired = true,
                        Options = new List<QuestionOption>
                        {
                            new() { Id = Guid.NewGuid(), Text = "Free", SortOrder = 0, Value = "free" },
                            new() { Id = Guid.NewGuid(), Text = "Pro ($29/mo)", SortOrder = 1, Value = "pro" },
                            new() { Id = Guid.NewGuid(), Text = "Enterprise ($99/mo)", SortOrder = 2, Value = "enterprise" }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "When do you plan to start using the new features?",
                        Type = QuestionType.Date,
                        SortOrder = 1,
                        IsRequired = false
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Website Redesign Feedback",
                Description = "We'd love your thoughts on the new homepage design.",
                Status = SurveyStatus.Published,
                PublicLinkId = GenerateLinkId(),
                PublishedAt = DateTime.UtcNow.AddDays(-14),
                Settings = new SurveySettings
                {
                    AllowAnonymous = true,
                    ShowProgressBar = true,
                    ResponseLimit = 500,
                    ThankYouMessage = "We appreciate your feedback on the new design!"
                },
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "How would you rate the new homepage design?",
                        Type = QuestionType.Rating,
                        SortOrder = 0,
                        IsRequired = true,
                        Settings = new QuestionSettings { MinRating = 1, MaxRating = 5 }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "What do you like most about the new design?",
                        Type = QuestionType.TextLong,
                        SortOrder = 1,
                        IsRequired = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = "How did you hear about the redesign?",
                        Type = QuestionType.Dropdown,
                        SortOrder = 2,
                        IsRequired = false,
                        Options = new List<QuestionOption>
                        {
                            new() { Id = Guid.NewGuid(), Text = "Email newsletter", SortOrder = 0 },
                            new() { Id = Guid.NewGuid(), Text = "Social media", SortOrder = 1 },
                            new() { Id = Guid.NewGuid(), Text = "In-app notification", SortOrder = 2 },
                            new() { Id = Guid.NewGuid(), Text = "Friend or colleague", SortOrder = 3 }
                        }
                    }
                }
            }
        };

        db.Surveys.AddRange(surveys);
        await db.SaveChangesAsync();
    }

    /// <summary>Generate a short, URL-safe public link identifier for seed surveys.</summary>
    private static string GenerateLinkId()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = Guid.NewGuid().ToByteArray();
        var result = new char[8];
        for (int i = 0; i < 8; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}
