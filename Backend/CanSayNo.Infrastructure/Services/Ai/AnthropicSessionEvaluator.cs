using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.Models.Messages;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Services.Ai;

/// <summary>
/// Pratik değerlendirmesini structured output (JSON şeması) ile alır. Şema sayısal sınır
/// desteklemediği için puan aralığı prompt'ta anlatılır, sunucuda sıkıştırılır.
/// </summary>
public sealed class AnthropicSessionEvaluator : ISessionEvaluator
{
    private const string Operation = "session evaluation";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly Dictionary<string, JsonElement> Schema = BuildSchema();

    private readonly AnthropicClientProvider _anthropic;
    private readonly ILogger<AnthropicSessionEvaluator> _logger;

    public AnthropicSessionEvaluator(AnthropicClientProvider anthropic, ILogger<AnthropicSessionEvaluator> logger)
    {
        _anthropic = anthropic;
        _logger = logger;
    }

    public async Task<EvaluationReply> EvaluateAsync(EvaluationPrompt prompt, CancellationToken cancellationToken = default)
    {
        var settings = _anthropic.Settings;

        var parameters = new MessageCreateParams
        {
            Model = settings.Model,
            MaxTokens = settings.EvaluationMaxTokens,
            OutputConfig = new OutputConfig
            {
                Effort = AnthropicClientProvider.ParseEffort(settings.EvaluationEffort),
                Format = new JsonOutputFormat { Schema = Schema },
            },
            System = new List<TextBlockParam>
            {
                new() { Text = prompt.Instructions, CacheControl = new CacheControlEphemeral() },
            },
            Messages = [new UserMessageParam { Content = prompt.Material }],
        };

        var response = await _anthropic.TryCreateAsync(parameters, Operation, cancellationToken);
        if (response is null)
            return EvaluationReply.Unavailable();

        var tokensUsed = _anthropic.CountTokens(response, Operation);

        if (response.StopReason == "refusal")
        {
            _logger.LogWarning("Session evaluation refused. Category: {Category}", response.StopDetails?.Category);
            return new EvaluationReply(AiCallOutcome.Refused, null, tokensUsed);
        }

        if (response.StopReason == "max_tokens")
        {
            _logger.LogWarning("Session evaluation truncated at max_tokens ({MaxTokens})", settings.EvaluationMaxTokens);
            return EvaluationReply.Unavailable(tokensUsed);
        }

        var json = AnthropicClientProvider.ExtractText(response);
        try
        {
            var parsed = JsonSerializer.Deserialize<EvaluationJson>(json, JsonOptions);
            if (parsed?.Scores is null)
                throw new JsonException("scores missing");

            return new EvaluationReply(
                AiCallOutcome.Success,
                new RawEvaluation(
                    parsed.Scores,
                    parsed.Strengths ?? [],
                    parsed.Improvements ?? [],
                    parsed.CoachNote ?? string.Empty,
                    parsed.IdealResponseExample ?? string.Empty),
                tokensUsed);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Session evaluation returned invalid JSON. Length: {Length}", json.Length);
            return EvaluationReply.Unavailable(tokensUsed);
        }
    }

    private static Dictionary<string, JsonElement> BuildSchema()
    {
        var scoreProperties = EvaluationPromptBuilder.CriterionKeys.Keys
            .ToDictionary(k => k, _ => (object)new { type = "integer" });

        var schema = new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["scores"] = new
                {
                    type = "object",
                    properties = scoreProperties,
                    required = EvaluationPromptBuilder.CriterionKeys.Keys.ToArray(),
                    additionalProperties = false,
                },
                ["strengths"] = new { type = "array", items = new { type = "string" } },
                ["improvements"] = new { type = "array", items = new { type = "string" } },
                ["coachNote"] = new { type = "string" },
                ["idealResponseExample"] = new { type = "string" },
            },
            required = new[] { "scores", "strengths", "improvements", "coachNote", "idealResponseExample" },
            additionalProperties = false,
        };

        // Anonim nesneyi üst seviye anahtar → JsonElement sözlüğüne çevir (SDK'nın beklediği şekil).
        var root = JsonSerializer.SerializeToElement(schema);
        return root.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone());
    }

    private sealed class EvaluationJson
    {
        public Dictionary<string, int>? Scores { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? Improvements { get; set; }
        public string? CoachNote { get; set; }
        public string? IdealResponseExample { get; set; }
    }
}
