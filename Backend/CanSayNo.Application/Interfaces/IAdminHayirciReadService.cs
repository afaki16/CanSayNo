using CanSayNo.Application.Features.Admin.Hayirci;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Application.Interfaces;

/// <summary>
/// Admin paneli için Hayırcı okuma sorguları (salt okunur, AsNoTracking). Birden çok
/// tabloyu (Users, Sessions, Subscriptions, UserStats, Scenarios) birleştiren raporlar
/// repository'leri şişirmesin diye burada toplanır. Yazma işlemleri UnitOfWork ile yapılır.
/// </summary>
public interface IAdminHayirciReadService
{
    /// <summary>Pasifler dahil tüm senaryolar, ContextId + SortOrder sırasında.</summary>
    Task<IReadOnlyList<Scenario>> GetAllScenariosAsync(CancellationToken cancellationToken = default);

    /// <summary>Senaryo başına oturum/değerlendirme sayısı ve ortalama puan (tüm kullanıcılar).</summary>
    Task<IReadOnlyDictionary<string, AdminScenarioUsageDto>> GetScenarioUsageAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Kullanıcı yoksa null.</summary>
    Task<AdminUserOverviewDto?> GetUserOverviewAsync(
        int userId, DateTime utcNow, CancellationToken cancellationToken = default);

    Task<AdminDashboardDto> GetDashboardAsync(
        DateTime utcNow, CancellationToken cancellationToken = default);
}
