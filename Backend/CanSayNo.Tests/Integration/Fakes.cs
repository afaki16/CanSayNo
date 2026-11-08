using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;

namespace CanSayNo.Tests.Integration;

/// <summary>Karakter her rauntta sabit bir cevap verir; gelen prompt'lar incelenmek üzere saklanır.</summary>
public sealed class FakeCharacterChatService : ICharacterChatService
{
    public const string ReplyText = "Hmm, emin misin? Bir düşün istersen.";
    public const int TokensPerReply = 100;

    public List<CharacterPrompt> Prompts { get; } = [];

    public Task<CharacterReply> GetReplyAsync(CharacterPrompt prompt, CancellationToken cancellationToken = default)
    {
        lock (Prompts) Prompts.Add(prompt);
        return Task.FromResult(new CharacterReply(AiCallOutcome.Success, ReplyText, TokensPerReply));
    }
}

/// <summary>
/// Her eksene 80 verir; bir eksene aralık dışı (150) değer koyar — sunucunun 0-100'e
/// sıkıştırdığını doğrulamak için. Ortalama: (80*4 + 100) / 5 = 84.
/// </summary>
public sealed class FakeSessionEvaluator : ISessionEvaluator
{
    public const int ExpectedTotalScore = 84;

    public int Calls;

    public Task<EvaluationReply> EvaluateAsync(EvaluationPrompt prompt, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Calls);

        var keys = EvaluationPromptBuilder.CriterionKeys.Keys.ToList();
        var scores = keys.ToDictionary(k => k, _ => 80);
        scores[keys[0]] = 150;

        var raw = new RawEvaluation(
            scores,
            ["Net bir hayır dedin."],
            ["Daha kısa olabilirdin."],
            "İyi iş.",
            "Hayır, bu sefer yapamayacağım.");
        return Task.FromResult(new EvaluationReply(AiCallOutcome.Success, raw, 500));
    }
}
