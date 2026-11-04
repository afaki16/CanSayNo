using System;
using System.Threading;
using System.Threading.Tasks;

namespace CanSayNo.Application.Interfaces;

/// <summary>
/// Pratik oturumu başına karşılıklı dışlama. Aynı oturuma eşzamanlı iki mesaj/değerlendirme
/// isteğinin ikisinin de LLM'e gitmesini ve kaydederken birbirini ezmesini (kayıp mesaj,
/// iki kez yazılan XP) önler.
/// Bellek içi: yalnızca tek API örneğinde geçerli. Yatay ölçeklenirse DB tabanlı kilide geçilmeli.
/// </summary>
public interface ISessionLock
{
    /// <summary>
    /// Kilidi <paramref name="wait"/> kadar bekleyerek almaya çalışır. Alınamazsa null döner.
    /// Dönen nesne dispose edilince kilit bırakılır.
    /// </summary>
    Task<IDisposable?> TryAcquireAsync(Guid sessionId, TimeSpan wait, CancellationToken cancellationToken);
}
