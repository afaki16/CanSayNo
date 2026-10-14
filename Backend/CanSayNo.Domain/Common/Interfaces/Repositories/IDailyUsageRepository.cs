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
}
