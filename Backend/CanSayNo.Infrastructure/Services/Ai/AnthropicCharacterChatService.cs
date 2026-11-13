using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.Models.Messages;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Services.Ai;

/// <summary>Karakter cevabını Anthropic Messages API'den alır.</summary>
public sealed class AnthropicCharacterChatService : ICharacterChatService
{
    private const string Operation = "character reply";

    private readonly AnthropicClientProvider _anthropic;
    private readonly ILogger<AnthropicCharacterChatService> _logger;

    public AnthropicCharacterChatService(
        AnthropicClientProvider anthropic,
        ILogger<AnthropicCharacterChatService> logger)
    {
        _anthropic = anthropic;
        _logger = logger;
    }

    public async Task<CharacterReply> GetReplyAsync(CharacterPrompt prompt, CancellationToken cancellationToken = default)
    {
        var settings = _anthropic.Settings;

        var parameters = new MessageCreateParams
        {
            Model = settings.Model,
            MaxTokens = settings.MaxTokens,
            // Cevap JSON: 1-3 ayrı mesaj + ruh hali (CharacterReplyFormat). Mobil balonları ayrı ayrı,
            // karakterin temposunda gösterir.
            OutputConfig = new OutputConfig
            {
                Effort = AnthropicClientProvider.ParseEffort(settings.Effort),
                Format = new JsonOutputFormat { Schema = Schema },
            },
            // Önbellek noktası senaryo bloğunun sonunda: kurallar + senaryo oturum boyunca
            // aynı kaldığı için her rauntta önbellekten okunur. Raunt bloğu noktadan sonra gelir.
            System = new List<TextBlockParam>
            {
                new() { Text = prompt.Rules },
                new() { Text = prompt.Scenario, CacheControl = new CacheControlEphemeral() },
                new() { Text = prompt.Round },
            },
            Messages = prompt.Turns
                .Select(t => t.IsUser
                    ? (MessageParam)new UserMessageParam { Content = t.Text }
                    : new AssistantMessageParam { Content = t.Text })
                .ToList(),
        };

        var response = await _anthropic.TryCreateAsync(parameters, Operation, cancellationToken);
        if (response is null)
            return CharacterReply.Unavailable();

        var tokensUsed = _anthropic.CountTokens(response, Operation);

        // Refusal'da içerik okunmaz.
        if (response.StopReason == "refusal")
        {
            _logger.LogWarning("Character reply refused. Category: {Category}", response.StopDetails?.Category);
            return new CharacterReply(AiCallOutcome.Refused, string.Empty, tokensUsed);
        }

        var json = AnthropicClientProvider.ExtractText(response);

        // max_tokens'ta kesilen cevap yarım JSON olur; kullanıcıya göstermeyiz.
        var parsed = response.StopReason == "max_tokens" ? null : CharacterReplyFormat.Parse(json);
        if (parsed is null)
        {
            _logger.LogWarning("Character reply unusable. StopReason: {StopReason}, Length: {Length}",
                response.StopReason, json.Length);
            return CharacterReply.Unavailable(tokensUsed);
        }

        return new CharacterReply(
            AiCallOutcome.Success,
            CharacterReplyFormat.Join(parsed.Parts),
            tokensUsed,
            parsed.Parts,
            parsed.Mood);
    }

    private static readonly Dictionary<string, System.Text.Json.JsonElement> Schema = CharacterReplyFormat.BuildSchema();
}
