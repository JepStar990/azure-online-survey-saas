using SurveyApi.Models;

namespace SurveyApi.Data;

public static class SeedData
{
    public static void EnsureSeedData(SurveyDbContext db)
    {
        if (db.Surveys.Any()) return;

        var s = new Survey
        {
            Title = "Customer Satisfaction",
            Description = "Quarterly customer satisfaction survey",
            Questions = new List<Question>
            {
                new Question { Text = "How satisfied are you with our product?", Type = "rating" },
                new Question { Text = "What can we improve?", Type = "text" }
            }
        };

        db.Surveys.Add(s);
        db.SaveChanges();
    }
}
