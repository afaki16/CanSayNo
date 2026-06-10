using Kickstart.Domain.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing repositories and transactions
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Repository properties
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPasswordResetTokenRepository PasswordResetTokens { get; }
    IUserVerificationTokenRepository UserVerificationTokens { get; }
    ITenantRepository Tenants { get; }
    ILoginAttemptRepository LoginAttempts { get; }
    // Core operations
    /// <summary>
    /// Save all changes made in this unit of work
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Transaction management
    /// <summary>
    /// Begin a new transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transaction instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already in progress</exception>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when no transaction is currently active</exception>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Transaction utilities
    /// <summary>
    /// Execute a function within a transaction
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already in progress</exception>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action within a transaction
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already in progress</exception>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
