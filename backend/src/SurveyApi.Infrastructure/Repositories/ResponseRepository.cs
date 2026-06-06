using Microsoft.EntityFrameworkCore;
using SurveyApi.Application.Interfaces;
using SurveyApi.Domain.Entities;
using SurveyApi.Infrastructure.Data;

namespace SurveyApi.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IResponseRepository.
/// </summary>
public class ResponseRepository : IResponseRepository
{
    private readonly AppDbContext _db;

    public ResponseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Response> SubmitAsync(Response response, CancellationToken ct = default)
    {
        _db.Responses.Add(response);
        await _db.SaveChangesAsync(ct);
        return response;
    }

    public async Task<PagedResult<Response>> GetResponsesAsync(
        Guid surveyId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Responses
            .Include(r => r.Answers)
            .Where(r => r.SurveyId == surveyId && r.Status == ResponseStatus.Submitted);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Response>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Response?> GetByIdWithAnswersAsync(Guid responseId, CancellationToken ct = default)
    {
        return await _db.Responses
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.Id == responseId, ct);
    }

    public async Task<int> GetSubmittedCountAsync(Guid surveyId, CancellationToken ct = default)
    {
        return await _db.Responses
            .CountAsync(r => r.SurveyId == surveyId && r.Status == ResponseStatus.Submitted, ct);
    }

    public async Task<bool> HasReachedResponseLimitAsync(Guid surveyId, int limit, CancellationToken ct = default)
    {
        var count = await _db.Responses
            .CountAsync(r => r.SurveyId == surveyId && r.Status == ResponseStatus.Submitted, ct);
        return count >= limit;
    }
}
