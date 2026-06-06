using Microsoft.Extensions.DependencyInjection;
using SurveyApi.Application.Services;
using SurveyApi.Application.Validators;

namespace SurveyApi.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services in the DI container.
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>Register all Application layer services and validators.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<SurveyService>();
        services.AddScoped<ResponseService>();
        services.AddScoped<AnalyticsService>();

        // Validators
        services.AddScoped<SurveyCreateValidator>();
        services.AddScoped<ResponseSubmitValidator>();

        return services;
    }
}
