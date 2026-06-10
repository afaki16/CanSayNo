using Kickstart.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface ILoginAttemptRepository : IRepository<LoginAttempt, int>
    {
        Task<int> CountRecentFailuresAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default);

        Task<DateTime?> GetLastFailureTimeAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default);

        Task ClearFailuresAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default);
    }
}
