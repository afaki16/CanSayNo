using System;
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

    /// <summary>
    /// Kullanıcıya ait oturumu senaryosuyla birlikte döndürür; başka kullanıcının
    /// oturumu için null (varlığını sızdırmamak için NotFound gibi ele alınır).
    /// </summary>
    /// <param name="asNoTracking">Query handler'larda true; güncellenecekse false.</param>
    Task<Session?> GetUserSessionAsync(
        System.Guid sessionId,
        int userId,
        bool asNoTracking,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının bu senaryoda açık ve henüz hiç mesaj göndermediği (CurrentRound == 0)
    /// en son oturumu. Pratik ekranı yeniden açıldığında günlük limiti yakmadan
    /// aynı oturuma devam etmek için kullanılır. Tracked döner.
    /// </summary>
    Task<Session?> GetUnstartedActiveSessionAsync(
        int userId,
        string scenarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının oturumlarını en yeniden eskiye, senaryosuyla birlikte sayfalı döndürür.
    /// </summary>
    /// <summary>
    /// Kullanıcının verilen andan sonra tamamlanıp DEĞERLENDİRİLMİŞ (TotalScore dolu) oturumlarının
    /// tamamlanma zamanları (UTC). Haftalık aktivite için; seri ve XP de değerlendirmede yazıldığı
    /// için aynı ölçüt kullanılır.
    /// </summary>
    Task<IReadOnlyList<DateTime>> GetEvaluatedCompletionTimesAsync(
        int userId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Session> Items, int TotalCount)> GetUserSessionsPagedAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
