using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

public interface IScenarioRepository : IRepository<Scenario, string>
{
    /// <summary>
    /// Verilen dildeki aktif (IsActive) senaryoları opsiyonel filtrelerle, ContextId + SortOrder
    /// sırasında döndürür. Read-only (AsNoTracking).
    /// </summary>
    Task<IReadOnlyList<Scenario>> GetActiveScenariosAsync(
        string language,
        ContextId? contextId,
        bool? isFreeOnly,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ID'ye göre yalnızca aktif senaryoyu döndürür. Pasif/silinmiş içerik için null.
    /// Read-only (AsNoTracking).
    /// </summary>
    Task<Scenario?> GetActiveByIdAsync(string id, CancellationToken cancellationToken = default);
}
