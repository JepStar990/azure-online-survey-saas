using Microsoft.EntityFrameworkCore;
using SurveyApi.Domain.Entities;

namespace SurveyApi.Infrastructure.Data;

/// <summary>
/// Primary database context for the Survey API.
/// Configures entity mappings, relationships, indexes, and JSON columns.
/// Uses EF Core migrations rather than EnsureCreated() for production readiness.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<Response> Responses => Set<Response>();
    public DbSet<ResponseAnswer> ResponseAnswers => Set<ResponseAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Survey ---
        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.PublicLinkId).HasMaxLength(12);
            entity.HasIndex(e => e.PublicLinkId).IsUnique().HasFilter("[PublicLinkId] IS NOT NULL");
            entity.HasIndex(e => e.Status);

            // Store SurveySettings as JSON
            entity.OwnsOne(e => e.Settings, owned =>
            {
                owned.ToJson("SettingsJson");
            });

            entity.HasMany(e => e.Questions)
                .WithOne(q => q.Survey)
                .HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Responses)
                .WithOne(r => r.Survey)
                .HasForeignKey(r => r.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Question ---
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.HasIndex(e => new { e.SurveyId, e.SortOrder });

            // Store QuestionSettings as JSON
            entity.OwnsOne(e => e.Settings, owned =>
            {
                owned.ToJson("SettingsJson");
            });

            entity.HasMany(e => e.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- QuestionOption ---
        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Value).HasMaxLength(100);
            entity.HasIndex(e => new { e.QuestionId, e.SortOrder });
        });

        // --- Response ---
        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasIndex(e => new { e.SurveyId, e.Status });
            entity.HasIndex(e => e.CompletedAt);

            entity.HasMany(e => e.Answers)
                .WithOne(a => a.Response)
                .HasForeignKey(a => a.ResponseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- ResponseAnswer ---
        modelBuilder.Entity<ResponseAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.HasIndex(e => e.ResponseId);
        });
    }
}
