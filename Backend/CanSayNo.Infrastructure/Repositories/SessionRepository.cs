using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class SessionRepository : RepositoryBase<Session, Guid>, ISessionRepository
{
    private readonly ApplicationDbContext _context;

    public SessionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyDictionary<string, ScenarioSessionAggregate>> GetUserScenarioAggregatesAsync(
        int userId,
        IEnumerable<string>? scenarioIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId);

        // Senaryo ID'lerine göre daralt (liste/detay senaryosunda gereksiz satır taramayı önler).
        var ids = scenarioIds?.Distinct().ToList();
        if (ids is { Count: > 0 })
            query = query.Where(s => ids.Contains(s.ScenarioId));

        // Tek GroupBy → DB tarafında agrega. Her senaryo için tek satır döner (N+1 yok).
        var aggregates = await query
            .GroupBy(s => s.ScenarioId)
            .Select(g => new ScenarioSessionAggregate
            {
                ScenarioId = g.Key,
                AttemptsCount = g.Count(),
                // TotalScore nullable; SQL MAX null'ları yok sayar → henüz skoru olmayan denemeler etkilemez.
                BestScore = g.Max(x => x.TotalScore),
                IsCompleted = g.Any(x => x.Status == SessionStatus.Completed),
                LastAttemptAt = g.Max(x => (DateTime?)x.StartedAt)
            })
            .ToListAsync(cancellationToken);

        return aggregates.ToDictionary(a => a.ScenarioId, a => a);
    }

    public async Task<Session?> GetUserSessionAsync(
        Guid sessionId,
        int userId,
        bool asNoTracking,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Session> query = _context.Sessions.Include(s => s.Scenario);
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(
            s => s.Id == sessionId && s.UserId == userId, cancellationToken);
    }

    public async Task<Session?> GetUnstartedActiveSessionAsync(
        int userId,
        string scenarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Scenario)
            .Where(s => s.UserId == userId
                        && s.ScenarioId == scenarioId
                        && s.Status == SessionStatus.Active
                        && s.CurrentRound == 0)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DateTime>> GetEvaluatedCompletionTimesAsync(
        int userId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                        && s.TotalScore != null
                        && s.CompletedAt != null
                        && s.CompletedAt >= sinceUtc)
            .Select(s => s.CompletedAt!.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<ContextId, int>> GetEvaluatedCountsByContextAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.TotalScore != null)
            .GroupBy(s => s.Scenario.ContextId)
            .Select(g => new { ContextId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ContextId, x => x.Count, cancellationToken);
    }

    public async Task<(IReadOnlyList<Session> Items, int TotalCount)> GetUserSessionsPagedAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(s => s.Scenario)
            .OrderByDescending(s => s.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
