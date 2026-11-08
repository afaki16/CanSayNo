using System;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

/// <summary>
/// Günlük kullanım sayaçları. Composite PK (UserId, Date) olduğu için generic
/// IRepository (tek anahtarlı) yerine kendi metotlarını sunar.
/// </summary>
public interface IDailyUsageRepository
{
    /// <summary>
    /// Kullanıcının verilen gündeki kaydını tracked olarak döndürür (güncellenebilir). Yoksa null.
    /// </summary>
    Task<DailyUsage?> GetAsync(int userId, DateOnly date, CancellationToken cancellationToken = default);

    Task AddAsync(DailyUsage dailyUsage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Var olan kaydın oturum sayacını tek bir UPDATE ile artırır, yalnızca sayaç
    /// <paramref name="limit"/>'in altındaysa. Eşzamanlı istekler sınırı aşamaz
    /// (oku-artır-yaz'da hepsi aynı değeri okuyup geçebiliyordu). Artırdıysa true.
    /// Change tracker'ı atlar; anında DB'ye yazılır.
    /// </summary>
    Task<bool> TryIncrementSessionsAsync(
        int userId, DateOnly date, int limit, CancellationToken cancellationToken = default);
}
