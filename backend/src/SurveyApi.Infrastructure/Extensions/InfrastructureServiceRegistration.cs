using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SurveyApi.Application.Interfaces;
using SurveyApi.Infrastructure.Auth;
using SurveyApi.Infrastructure.Data;
using SurveyApi.Infrastructure.Repositories;

namespace SurveyApi.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services in the DI container.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Register all Infrastructure layer services: DbContext, repositories, and auth helpers.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var useInMemory = configuration.GetValue<bool?>("UseInMemory") ?? false;
        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase("SurveyDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "DefaultConnection is not configured. Set the connection string or enable UseInMemory.");

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(connectionString, sqlOpt =>
                {
                    sqlOpt.EnableRetryOnFailure(maxRetryCount: 3);
                    sqlOpt.CommandTimeout(30);
                }));
        }

        // Repositories
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<IResponseRepository, ResponseRepository>();

        // Auth
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();

        return services;
    }
}
