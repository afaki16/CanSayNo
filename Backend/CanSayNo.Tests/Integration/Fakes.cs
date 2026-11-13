using System.Collections.Concurrent;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// Karakter her rauntta sabit, iki balonlu bir cevap verir (ruh hali: ısrarcı); gelen prompt'lar
/// incelenmek üzere saklanır.
/// </summary>
public sealed class FakeCharacterChatService : ICharacterChatService
{
    public static readonly string[] ReplyParts = ["Hmm, emin misin?", "Bir düşün istersen."];
    public static readonly string ReplyText = CharacterReplyFormat.Join(ReplyParts);
    public const CharacterMood ReplyMood = CharacterMood.Insistent;
    public const int TokensPerReply = 100;

    public List<CharacterPrompt> Prompts { get; } = [];

    public Task<CharacterReply> GetReplyAsync(CharacterPrompt prompt, CancellationToken cancellationToken = default)
    {
        lock (Prompts) Prompts.Add(prompt);
        return Task.FromResult(new CharacterReply(AiCallOutcome.Success, ReplyText, TokensPerReply, ReplyParts, ReplyMood));
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

/// <summary>E-posta gönderilmez; tek seferlik kodlar adrese göre saklanır (şifre sıfırlama testleri okur).</summary>
public sealed class FakeEmailService : IEmailService
{
    public ConcurrentDictionary<string, string> PasswordCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

    private static Task<Result> Ok() => Task.FromResult(Result.Success());

    public Task<Result> SendPasswordCodeAsync(string email, string firstName, string code, int validMinutes)
    {
        PasswordCodes[email] = code;
        return Ok();
    }

    public ConcurrentDictionary<string, string> RegistrationCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<Result> SendRegistrationCodeAsync(string email, string firstName, string code, int validMinutes)
    {
        RegistrationCodes[email] = code;
        return Ok();
    }

    public Task<Result> SendEmailAsync(string to, string subject, string body) => Ok();
    public Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink) => Ok();
    public Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink) => Ok();
    public Task<Result> SendEmailChangeVerificationAsync(string newEmail, string firstName, string confirmLink, int validHours) => Ok();
    public Task<Result> SendEmailChangedNoticeAsync(string oldEmail, string firstName, string newEmail) => Ok();
    public Task<Result> SendEmailChangeCodeAsync(string newEmail, string firstName, string code, int validMinutes) => Ok();
    public Task<Result> SendWelcomeEmailAsync(string email, string userName) => Ok();
    public Task<Result> SendBruteForceAlertAsync(string email, string firstName, string ipAddress, int failureCount, int lockoutMinutes) => Ok();
}
