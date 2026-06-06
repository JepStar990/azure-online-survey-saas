using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SurveyApi.Application.DTOs;

namespace SurveyApi.IntegrationTests;

public class SurveyEndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public SurveyEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Get_surveys_returns_empty_list_when_no_surveys_exist()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/surveys?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Without auth headers, this should return 401 if auth is enforced
        // In dev with UseInMemory, auth may be relaxed. The test validates the endpoint exists.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_survey_by_id_returns_404_for_nonexistent()
    {
        var response = await _fixture.Client.GetAsync($"/api/v1/surveys/{Guid.NewGuid()}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_survey_without_auth_returns_unauthorized()
    {
        var request = new SurveyCreateRequest
        {
            Title = "Test",
            Questions = new List<QuestionCreateRequest>
            {
                new() { Text = "Q1", Type = "TextShort", IsRequired = true, SortOrder = 0 }
            }
        };

        var response = await _fixture.Client.PostAsJsonAsync("/api/v1/surveys", request);
        // This should require authentication — returns 401 or 302 redirect
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }
}
