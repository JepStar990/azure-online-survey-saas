using Microsoft.EntityFrameworkCore;
using SurveyApi.Application.Interfaces;
using SurveyApi.Domain.Entities;
using SurveyApi.Domain.Enums;
using SurveyApi.Infrastructure.Data;

namespace SurveyApi.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISurveyRepository.
/// </summary>
public class SurveyRepository : ISurveyRepository
{
    private readonly AppDbContext _db;

    public SurveyRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Survey>> GetSurveysAsync(
        int page, int pageSize, SurveyStatus? statusFilter, CancellationToken ct = default)
    {
        var query = _db.Surveys
            .Include(s => s.Questions)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(s => s.Status == statusFilter.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Survey>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Survey?> GetByIdWithQuestionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Surveys
            .Include(s => s.Questions.OrderBy(q => q.SortOrder))
                .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Survey?> GetByPublicLinkIdAsync(string publicLinkId, CancellationToken ct = default)
    {
        return await _db.Surveys
            .Include(s => s.Questions.OrderBy(q => q.SortOrder))
                .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(s => s.PublicLinkId == publicLinkId && s.Status == SurveyStatus.Published, ct);
    }

    public async Task<Survey> CreateAsync(Survey survey, CancellationToken ct = default)
    {
        _db.Surveys.Add(survey);
        await _db.SaveChangesAsync(ct);
        return survey;
    }

    public async Task UpdateAsync(Survey survey, CancellationToken ct = default)
    {
        // EF Core tracks changes via the DbContext, so just save
        _db.Surveys.Update(survey);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Survey survey, CancellationToken ct = default)
    {
        _db.Surveys.Remove(survey);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken ct = default)
    {
        return await _db.Responses
            .CountAsync(r => r.SurveyId == surveyId && r.Status == ResponseStatus.Submitted, ct);
    }
}
