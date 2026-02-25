using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SurveyApi.Data;
using SurveyApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Survey API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, new List<string>() }
    });
});

// Authentication (Azure AD)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAd:Authority"] ?? string.Empty;
        options.Audience = builder.Configuration["AzureAd:Audience"] ?? string.Empty;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false
        };
    });
builder.Services.AddAuthorization();

// Configure EF Core: prefer real SQL when configured, otherwise use InMemory for local/dev
var useInMemory = builder.Configuration.GetValue<bool?>("UseInMemory") ?? builder.Environment.IsDevelopment();
if (useInMemory)
{
    builder.Services.AddDbContext<SurveyDbContext>(opt => opt.UseInMemoryDatabase("SurveysDb"));
}
else
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(conn)) throw new InvalidOperationException("DefaultConnection is not configured.");
    builder.Services.AddDbContext<SurveyDbContext>(opt => opt.UseSqlServer(conn));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Ensure DB created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsureSeedData(db);
}

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

// List surveys (allow anonymous in development or when configured)
var allowAnonymousRead = builder.Configuration.GetValue<bool?>("AllowAnonymousRead") ?? app.Environment.IsDevelopment();
var surveysEndpoint = app.MapGet("/api/surveys", async (SurveyDbContext db) =>
{
    var list = await db.Surveys.Include(s => s.Questions).ToListAsync();
    return Results.Ok(list);
});
if (!allowAnonymousRead) surveysEndpoint.RequireAuthorization();

app.MapGet("/api/surveys/{id}", async (int id, SurveyDbContext db) =>
{
    var survey = await db.Surveys.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == id);
    return survey is not null ? Results.Ok(survey) : Results.NotFound();
}).RequireAuthorization();

app.MapPost("/api/surveys", async (Survey survey, SurveyDbContext db) =>
{
    survey.CreatedAt = DateTime.UtcNow;
    db.Surveys.Add(survey);
    await db.SaveChangesAsync();
    return Results.Created($"/api/surveys/{survey.Id}", survey);
}).RequireAuthorization();

app.MapPut("/api/surveys/{id}", async (int id, Survey updated, SurveyDbContext db) =>
{
    var existing = await db.Surveys.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == id);
    if (existing is null) return Results.NotFound();
    existing.Title = updated.Title;
    existing.Description = updated.Description;
    // naive replace questions
    db.Questions.RemoveRange(existing.Questions);
    existing.Questions = updated.Questions;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/api/surveys/{id}", async (int id, SurveyDbContext db) =>
{
    var existing = await db.Surveys.FindAsync(id);
    if (existing is null) return Results.NotFound();
    db.Surveys.Remove(existing);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

// Make the implicit Program class available for integration tests
public partial class Program { }
