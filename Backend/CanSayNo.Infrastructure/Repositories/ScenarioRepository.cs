using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class ScenarioRepository : RepositoryBase<Scenario, string>, IScenarioRepository
{
    private readonly ApplicationDbContext _context;

    public ScenarioRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Scenario>> GetActiveScenariosAsync(
        string language,
        ContextId? contextId,
        bool? isFreeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Scenarios
            .AsNoTracking()
            .Where(s => s.IsActive && s.Language == language);

        if (contextId.HasValue)
            query = query.Where(s => s.ContextId == contextId.Value);

        if (isFreeOnly == true)
            query = query.Where(s => s.IsFree);

        return await query
            .OrderBy(s => s.ContextId)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Scenario?> GetActiveByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Scenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);
    }
}
