using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Interfaces;

namespace CanSayNo.Infrastructure.Services.Hayirci;

/// <summary>
/// Oturum başına SemaphoreSlim. Referans sayılır; son kullanıcı bırakınca sözlükten silinir,
/// böylece biten oturumlar bellekte birikmez. Singleton olarak kaydedilir.
/// </summary>
public sealed class InMemorySessionLock : ISessionLock
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Entry> _entries = new();

    public async Task<IDisposable?> TryAcquireAsync(Guid sessionId, TimeSpan wait, CancellationToken cancellationToken)
    {
        Entry entry;
        lock (_gate)
        {
            if (!_entries.TryGetValue(sessionId, out entry!))
            {
                entry = new Entry();
                _entries[sessionId] = entry;
            }
            entry.References++;
        }

        var acquired = false;
        try
        {
            acquired = await entry.Semaphore.WaitAsync(wait, cancellationToken);
        }
        finally
        {
            if (!acquired)
                Unreference(sessionId, entry);
        }

        return acquired ? new Releaser(this, sessionId, entry) : null;
    }

    private void Unreference(Guid sessionId, Entry entry)
    {
        lock (_gate)
        {
            entry.References--;
            if (entry.References == 0)
            {
                _entries.Remove(sessionId);
                entry.Semaphore.Dispose();
            }
        }
    }

    private sealed class Entry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int References { get; set; }
    }

    private sealed class Releaser : IDisposable
    {
        private readonly InMemorySessionLock _owner;
        private readonly Guid _sessionId;
        private readonly Entry _entry;
        private int _disposed;

        public Releaser(InMemorySessionLock owner, Guid sessionId, Entry entry)
        {
            _owner = owner;
            _sessionId = sessionId;
            _entry = entry;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            _entry.Semaphore.Release();
            _owner.Unreference(_sessionId, _entry);
        }
    }
}
