using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.Models.Messages;
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
            OutputConfig = new OutputConfig { Effort = AnthropicClientProvider.ParseEffort(settings.Effort) },
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

        var text = AnthropicClientProvider.ExtractText(response);

        // max_tokens'ta kesilen cevap yarım cümle olur; kullanıcıya göstermeyiz.
        if (string.IsNullOrEmpty(text) || response.StopReason == "max_tokens")
        {
            _logger.LogWarning("Character reply unusable. StopReason: {StopReason}, TextLength: {Length}",
                response.StopReason, text.Length);
            return CharacterReply.Unavailable(tokensUsed);
        }

        return new CharacterReply(AiCallOutcome.Success, text, tokensUsed);
    }
}
