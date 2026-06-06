using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SurveyApi.Infrastructure.Data;

namespace SurveyApi.IntegrationTests;

/// <summary>
/// Shared test fixture that provides an HttpClient configured against the API
/// with an in-memory database for isolated integration testing.
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public HttpClient Client { get; }

    public IntegrationTestFixture()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real DbContext with an in-memory one for test isolation
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase($"SurveyTestDb_{Guid.NewGuid():N}"));

                // Build the service provider to ensure the DB is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        });

        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}
