using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_endpoint_returns_ok()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Surveys_endpoint_returns_seeded_surveys_in_dev()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/surveys");
        // In our configuration AllowAnonymousRead is true for dev, so expect OK
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("Customer Satisfaction");
    }
}
