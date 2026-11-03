using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using CanSayNo.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Services.Ai;

/// <summary>
/// Paylaşılan AnthropicClient (singleton; thread-safe, HTTP bağlantılarını yeniden kullanır)
/// ve iki AI servisinin ortak çağrı/hata işleme mantığı. Anahtar yoksa <see cref="Client"/> null'dır.
/// </summary>
public sealed class AnthropicClientProvider
{
    private readonly ILogger<AnthropicClientProvider> _logger;

    public AnthropicClient? Client { get; }

    public AnthropicSettings Settings { get; }

    public AnthropicClientProvider(AnthropicSettings settings, ILogger<AnthropicClientProvider> logger)
    {
        Settings = settings;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            _logger.LogWarning(
                "Anthropic:ApiKey is not configured. Character replies and evaluations will return aiUnavailable. " +
                "Set it with: dotnet user-secrets set \"Anthropic:ApiKey\" \"<key>\" --project CanSayNo.API");
        }
        else
        {
            Client = new AnthropicClient { ApiKey = settings.ApiKey };
        }
    }

    /// <summary>
    /// İsteği gönderir; hata durumunda null döner ve sebebi loglar. Tekrar denemeyi SDK yapar
    /// (429/5xx/bağlantı için varsayılan 2 deneme); buraya gelen hata kullanıcıya 503 olarak döner.
    /// </summary>
    public async Task<Message?> TryCreateAsync(MessageCreateParams parameters, string operation, CancellationToken cancellationToken)
    {
        if (Client is null)
            return null;

        try
        {
            return await Client.Messages.Create(parameters, cancellationToken);
        }
        catch (AnthropicRateLimitException ex)
        {
            _logger.LogWarning(ex, "Anthropic rate limit hit during {Operation}", operation);
        }
        catch (Anthropic5xxException ex)
        {
            _logger.LogWarning(ex, "Anthropic server error during {Operation}", operation);
        }
        catch (AnthropicIOException ex)
        {
            _logger.LogWarning(ex, "Anthropic connection error during {Operation}", operation);
        }
        catch (AnthropicApiException ex)
        {
            // 400/401/403/404: yapılandırma ya da istek şekli hatası — tekrar denemek düzeltmez.
            _logger.LogError(ex, "Anthropic request rejected during {Operation}", operation);
        }

        return null;
    }

    public int CountTokens(Message response, string operation)
    {
        var usage = response.Usage;

        _logger.LogInformation(
            "Anthropic {Operation}. StopReason: {StopReason}, Input: {Input}, Output: {Output}, CacheWrite: {CacheWrite}, CacheRead: {CacheRead}",
            operation, response.StopReason, usage.InputTokens, usage.OutputTokens,
            usage.CacheCreationInputTokens, usage.CacheReadInputTokens);

        return (int)(usage.InputTokens
                     + usage.OutputTokens
                     + (usage.CacheCreationInputTokens ?? 0)
                     + (usage.CacheReadInputTokens ?? 0));
    }

    /// <summary>Metin bloklarını birleştirir. Bloklar tipe göre okunur: cevap thinking ile başlayabilir.</summary>
    public static string ExtractText(Message response) =>
        string.Join("\n", response.Content.Select(b => b.Value).OfType<TextBlock>().Select(b => b.Text)).Trim();

    public static Effort ParseEffort(string value) => value.Trim().ToLowerInvariant() switch
    {
        "medium" => Effort.Medium,
        "high" => Effort.High,
        _ => Effort.Low
    };
}
