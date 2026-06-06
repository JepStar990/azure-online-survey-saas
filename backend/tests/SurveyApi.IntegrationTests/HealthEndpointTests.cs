using FluentAssertions;

namespace SurveyApi.IntegrationTests;

public class HealthEndpointTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public HealthEndpointTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_endpoint_returns_200()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/health");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }
}
