using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SurveyApi.Models;

public class SurveyCrudTests
{
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace default authentication with test authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        });
    }

    [Fact]
    public async Task Create_Update_Delete_Survey_Workflow()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();

        var newSurvey = new Survey { Title = "Integration Create Test", Description = "Desc", Questions = new List<Question>{ new Question{ Text = "Q1", Type = "text" } } };

        // Create
        var createRes = await client.PostAsJsonAsync("/api/surveys", newSurvey);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createRes.Content.ReadFromJsonAsync<Survey>();
        created.Should().NotBeNull();
        created!.Title.Should().Be(newSurvey.Title);

        // Update
        created.Title = "Updated Title";
        var updateRes = await client.PutAsJsonAsync($"/api/surveys/{created.Id}", created);
        updateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read
        var getRes = await client.GetAsync($"/api/surveys/{created.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getRes.Content.ReadFromJsonAsync<Survey>();
        fetched!.Title.Should().Be("Updated Title");

        // Delete
        var delRes = await client.DeleteAsync($"/api/surveys/{created.Id}");
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await client.GetAsync($"/api/surveys/{created.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
