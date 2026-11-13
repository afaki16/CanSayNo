using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Sessions.Common;

/// <summary>
/// Karakter cevabının yapılandırılmış biçimi: <c>{"messages": ["...", "..."], "mood": "insistent"}</c>.
/// Şema sağlayıcıya (structured outputs) bununla verilir; gelen JSON burada okunup sınırlanır.
/// Saf: DB'ye ve ağa dokunmaz.
/// </summary>
public static class CharacterReplyFormat
{
    /// <summary>Bir cevapta en fazla bu kadar balon gösterilir; fazlası sondakiyle birleştirilir.</summary>
    public const int MaxParts = 3;

    /// <summary>JSON'daki ruh hali anahtarları (camelCase, API'deki enum yazımıyla aynı).</summary>
    public static readonly IReadOnlyDictionary<string, CharacterMood> MoodKeys = new Dictionary<string, CharacterMood>
    {
        ["calm"] = CharacterMood.Calm,
        ["insistent"] = CharacterMood.Insistent,
        ["hurt"] = CharacterMood.Hurt,
        ["annoyed"] = CharacterMood.Annoyed,
        ["yielding"] = CharacterMood.Yielding,
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Sağlayıcının beklediği şekil: üst seviye anahtar → JSON şema düğümü.</summary>
    public static Dictionary<string, JsonElement> BuildSchema()
    {
        var schema = new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["messages"] = new { type = "array", items = new { type = "string" } },
                ["mood"] = new { type = "string", @enum = MoodKeys.Keys.ToArray() },
            },
            required = new[] { "messages", "mood" },
            additionalProperties = false,
        };

        var root = JsonSerializer.SerializeToElement(schema);
        return root.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone());
    }

    /// <summary>
    /// Modelin JSON'unu okur. Boş balonlar atılır, en fazla <see cref="MaxParts"/> balon kalır
    /// (fazlası sonuncuya eklenir — söz kaybolmaz). Kullanılabilir metin yoksa ya da JSON bozuksa
    /// null. Bilinmeyen ruh hali <see cref="CharacterMood.Insistent"/> sayılır.
    /// </summary>
    public static ParsedCharacterReply? Parse(string json)
    {
        ReplyJson? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ReplyJson>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        var parts = (parsed?.Messages ?? [])
            .Select(m => m?.Trim() ?? string.Empty)
            .Where(m => m.Length > 0)
            .ToList();

        if (parts.Count == 0)
            return null;

        if (parts.Count > MaxParts)
        {
            var tail = string.Join(" ", parts.Skip(MaxParts - 1));
            parts = [.. parts.Take(MaxParts - 1), tail];
        }

        var mood = parsed?.Mood is { } key && MoodKeys.TryGetValue(key.Trim().ToLowerInvariant(), out var m)
            ? m
            : CharacterMood.Insistent;

        return new ParsedCharacterReply(parts, mood);
    }

    /// <summary>Geçmişe/değerlendirmeye giden düz metin: balonlar satır satır.</summary>
    public static string Join(IEnumerable<string> parts) => string.Join("\n", parts);

    private sealed class ReplyJson
    {
        public List<string?>? Messages { get; set; }
        public string? Mood { get; set; }
    }
}

public sealed record ParsedCharacterReply(IReadOnlyList<string> Parts, CharacterMood Mood);
