using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApi.Data;
using SurveyApi.Models;

public class DbContextTests
{
    [Fact]
    public void Can_add_and_query_survey_in_memory()
    {
        var opts = new DbContextOptionsBuilder<SurveyDbContext>().UseInMemoryDatabase("test-db-1").Options;
        using var db = new SurveyDbContext(opts);
        var s = new Survey { Title = "Unit DB Test" };
        db.Surveys.Add(s);
        db.SaveChanges();

        var item = db.Surveys.FirstOrDefault();
        item.Should().NotBeNull();
        item!.Title.Should().Be("Unit DB Test");
    }
}
