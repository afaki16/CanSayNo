using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Models;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

public interface ISessionRepository : IRepository<Session, System.Guid>
{
    /// <summary>
    /// Verilen kullanıcının senaryo bazlı oturum özetlerini TEK sorguda döndürür
    /// (deneme sayısı, en iyi skor, tamamlanma, son deneme zamanı). N+1'den kaçınmak için
    /// GroupBy ile veritabanında agregalanır. Sonuç ScenarioId -> özet sözlüğüdür.
    /// </summary>
    /// <param name="scenarioIds">
    /// Sınırlandırılacak senaryo ID'leri. Null/boş ise kullanıcının tüm oturumları taranır.
    /// </param>
    Task<IReadOnlyDictionary<string, ScenarioSessionAggregate>> GetUserScenarioAggregatesAsync(
        int userId,
        IEnumerable<string>? scenarioIds = null,
        CancellationToken cancellationToken = default);
}
