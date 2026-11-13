using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Interfaces;

/// <summary>
/// Senaryo karakterinin bir sonraki cevabını üreten LLM servisi. Prompt'un içeriği
/// Application'da (<see cref="Features.Sessions.Common.CharacterPromptBuilder"/>) kurulur;
/// bu servis yalnızca sağlayıcıya iletir. API anahtarı sadece backend'dedir.
/// </summary>
public interface ICharacterChatService
{
    Task<CharacterReply> GetReplyAsync(CharacterPrompt prompt, CancellationToken cancellationToken = default);
}

/// <param name="Rules">Tüm senaryolarda aynı kural bloğu (önbelleğe alınır).</param>
/// <param name="Scenario">Senaryoya özel blok — oturum boyunca değişmez (önbelleğe alınır).</param>
/// <param name="Round">O rauntun talimatı — her raunt değişir, önbellek noktasından sonra gelir.</param>
/// <param name="Turns">Konuşma; ilk eleman kullanıcıdır, roller sırayla değişir, son eleman kullanıcıdır.</param>
public sealed record CharacterPrompt(
    string Rules,
    string Scenario,
    string Round,
    IReadOnlyList<CharacterTurn> Turns);

public sealed record CharacterTurn(bool IsUser, string Text);

public enum AiCallOutcome
{
    Success,
    /// <summary>Model mesaja cevap vermeyi reddetti (stop_reason = refusal).</summary>
    Refused,
    /// <summary>Sağlayıcıya ulaşılamadı, anahtar yok, boş/kesik cevap vb.</summary>
    Unavailable
}

/// <param name="Text">Balonların birleşimi — geçmişe ve değerlendirmeye bu girer.</param>
/// <param name="Parts">Ayrı balonlar (1-3); başarısız cevapta boş.</param>
/// <param name="Mood">Karakterin ruh hali; başarısız cevapta null.</param>
public sealed record CharacterReply(
    AiCallOutcome Outcome,
    string Text,
    int TokensUsed,
    IReadOnlyList<string>? Parts = null,
    CharacterMood? Mood = null)
{
    public static CharacterReply Unavailable(int tokensUsed = 0) =>
        new(AiCallOutcome.Unavailable, string.Empty, tokensUsed);
}
