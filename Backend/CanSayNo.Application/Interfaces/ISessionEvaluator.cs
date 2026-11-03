using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CanSayNo.Application.Interfaces;

/// <summary>
/// Tamamlanmış bir pratiği 5 eksende değerlendiren LLM servisi. Prompt içeriği
/// Application'da (<see cref="Features.Sessions.Common.EvaluationPromptBuilder"/>) kurulur.
/// </summary>
public interface ISessionEvaluator
{
    Task<EvaluationReply> EvaluateAsync(EvaluationPrompt prompt, CancellationToken cancellationToken = default);
}

/// <param name="Instructions">Değerlendirme kuralları ve puanlama ölçütü (system).</param>
/// <param name="Material">Senaryo bilgisi + konuşma dökümü (user mesajı).</param>
public sealed record EvaluationPrompt(string Instructions, string Material);

/// <summary>
/// Modelin structured output ile döndürdüğü ham değerlendirme. Puanlar 0-100 aralığına
/// sunucuda sıkıştırılır (şema sayısal sınır desteklemiyor); toplam puanı sunucu hesaplar.
/// </summary>
public sealed record RawEvaluation(
    IReadOnlyDictionary<string, int> Scores,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Improvements,
    string CoachNote,
    string IdealResponseExample);

public sealed record EvaluationReply(AiCallOutcome Outcome, RawEvaluation? Evaluation, int TokensUsed)
{
    public static EvaluationReply Unavailable(int tokensUsed = 0) =>
        new(AiCallOutcome.Unavailable, null, tokensUsed);
}
