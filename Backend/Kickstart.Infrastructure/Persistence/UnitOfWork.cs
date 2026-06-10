using Microsoft.EntityFrameworkCore.Storage;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    // Repository fields with thread-safe lazy initialization
    private readonly Lazy<IUserRepository> _users;
    private readonly Lazy<IRoleRepository> _roles;
    private readonly Lazy<IPermissionRepository> _permissions;
    private readonly Lazy<IRefreshTokenRepository> _refreshTokens;
    private readonly Lazy<IPasswordResetTokenRepository> _passwordResetTokens;
    private readonly Lazy<IUserVerificationTokenRepository> _userVerificationTokens;
    private readonly Lazy<ITenantRepository> _tenants;
    private readonly Lazy<ILoginAttemptRepository> _loginAttempts;

    public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        // Lazy repository'leri initialize et
        _users = new Lazy<IUserRepository>(() => _serviceProvider.GetRequiredService<IUserRepository>());
        _roles = new Lazy<IRoleRepository>(() => _serviceProvider.GetRequiredService<IRoleRepository>());
        _permissions = new Lazy<IPermissionRepository>(() => _serviceProvider.GetRequiredService<IPermissionRepository>());
        _refreshTokens = new Lazy<IRefreshTokenRepository>(() => _serviceProvider.GetRequiredService<IRefreshTokenRepository>());
        _passwordResetTokens = new Lazy<IPasswordResetTokenRepository>(() => _serviceProvider.GetRequiredService<IPasswordResetTokenRepository>());
        _userVerificationTokens = new Lazy<IUserVerificationTokenRepository>(() => _serviceProvider.GetRequiredService<IUserVerificationTokenRepository>());
        _tenants = new Lazy<ITenantRepository>(() => _serviceProvider.GetRequiredService<ITenantRepository>());
        _loginAttempts = new Lazy<ILoginAttemptRepository>(() => _serviceProvider.GetRequiredService<ILoginAttemptRepository>());

    }

    // Repository properties - only created when accessed
    public IUserRepository Users => _users.Value;
    public IRoleRepository Roles => _roles.Value;
    public IPermissionRepository Permissions => _permissions.Value;
    public IRefreshTokenRepository RefreshTokens => _refreshTokens.Value;
    public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokens.Value;
    public IUserVerificationTokenRepository UserVerificationTokens => _userVerificationTokens.Value;
    public ITenantRepository Tenants => _tenants.Value;
    public ILoginAttemptRepository LoginAttempts => _loginAttempts.Value;




    /// <summary>
    /// Save all changes made in this unit of work
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begin a new transaction
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction is currently active.");
            }

            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Execute a function within a transaction
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        try
        {
            await BeginTransactionAsync(cancellationToken);
            var result = await operation();
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Execute an action within a transaction
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        try
        {
            await BeginTransactionAsync(cancellationToken);
            await operation();
            await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Async disposal implementation
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core async disposal logic
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _context.DisposeAsync();
        _disposed = true;
    }

    /// <summary>
    /// Synchronous disposal for IDisposable compatibility
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up
    /// </summary>
    ~UnitOfWork()
    {
        DisposeAsync().AsTask().Wait();
    }
}
