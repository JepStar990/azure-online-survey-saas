using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Serilog;
using SurveyApi.Application.Extensions;
using SurveyApi.Infrastructure.Extensions;
using SurveyApi.Web.Endpoints;
using SurveyApi.Web.Middleware;

// --- Configure Serilog early for startup logging ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Logging: Serilog with optional Application Insights sink ---
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SurveyApi"));

    // --- Swagger / OpenAPI with Azure AD OAuth2 ---
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Survey API",
            Version = "v1",
            Description = "Azure AD-integrated survey SaaS API"
        });

        var authority = builder.Configuration["AzureAd:Authority"] ?? string.Empty;
        var scopes = new Dictionary<string, string>
        {
            { $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user", "Access the Survey API" }
        };

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/oauth2/v2.0/authorize"),
                    Scopes = scopes
                }
            }
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                },
                new List<string>()
            }
        });
    });

    // --- Authentication: Azure AD JWT Bearer ---
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["AzureAd:Authority"] ?? string.Empty;
            options.Audience = builder.Configuration["AzureAd:Audience"] ?? string.Empty;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuers = new[]
                {
                    // Home tenant
                    "https://login.microsoftonline.com/f36fb8ee-44de-4d52-b910-fa4826ae3110/v2.0",
                    // Common endpoint for multi-tenant
                    "https://login.microsoftonline.com/common/v2.0"
                },
                ValidAudience = builder.Configuration["AzureAd:Audience"]
            };
            options.MapInboundClaims = false; // Preserve original claim types from Azure AD
        });

    builder.Services.AddAuthorization(options =>
    {
        // Role-based policies using Azure AD app roles
        options.AddPolicy("RequireSurveyCreator", policy =>
            policy.RequireRole("SurveyCreator", "TenantAdmin"));
        options.AddPolicy("RequireTenantAdmin", policy =>
            policy.RequireRole("TenantAdmin"));
        options.AddPolicy("RequireSurveyViewer", policy =>
            policy.RequireRole("SurveyViewer", "SurveyCreator", "TenantAdmin"));
    });

    // --- Application & Infrastructure layers ---
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // --- CORS: strict origins for production ---
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCors", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                          ?? new[] { "http://localhost:5173" };
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // --- Health checks ---
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // --- Middleware pipeline ---
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Survey API v1");
            c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
            c.OAuthUsePkce();
        });
    }

    app.UseCors("DefaultCors");
    app.UseAuthentication();
    app.UseAuthorization();

    // --- Map endpoints ---
    app.MapHealthEndpoints();
    app.MapSurveyEndpoints();
    app.MapQuestionEndpoints();
    app.MapResponseEndpoints();
    app.MapAnalyticsEndpoints();

    // --- Run database migrations in development ---
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SurveyApi.Infrastructure.Data.AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Exposes the Program class for integration testing with WebApplicationFactory.
/// </summary>
public partial class Program { }
