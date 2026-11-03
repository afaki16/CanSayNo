using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

/// <summary>
/// Kullanıcı istatistikleri (1-1, PK = UserId). Generic IRepository'nin int Id'si
/// kullanılmadığı için kendi metotlarını sunar.
/// </summary>
public interface IUserStatsRepository
{
    /// <summary>Kullanıcının istatistiklerini tracked döndürür. Yoksa null.</summary>
    Task<UserStats?> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserStats stats, CancellationToken cancellationToken = default);
}
