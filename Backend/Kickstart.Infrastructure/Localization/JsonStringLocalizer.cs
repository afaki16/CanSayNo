using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Kickstart.Application.Common.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kickstart.Infrastructure.Localization;

/// <summary>
/// JSON file-based localization implementation.
///
/// Loads {culture}.json from /Resources directory at startup and caches in memory.
/// Supports hierarchical keys (e.g. "auth.invalidCredentials" → resources["auth"]["invalidCredentials"]).
///
/// Falls back through: requested culture → default culture (tr) → the key itself.
/// </summary>
public class JsonStringLocalizer : IJsonStringLocalizer
{
    private const string DefaultCulture = "tr";

    private readonly ILogger<JsonStringLocalizer> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources = new();

    public JsonStringLocalizer(IHostEnvironment env, ILogger<JsonStringLocalizer> logger)
    {
        _logger = logger;
        LoadResources(env);
    }

    private void LoadResources(IHostEnvironment env)
    {
        // Resources directory lives next to the Infrastructure assembly so it ships
        // with the build output. ContentRootPath is the app's working dir at runtime.
        var resourcesPath = Path.Combine(AppContext.BaseDirectory, "Resources");

        if (!Directory.Exists(resourcesPath))
        {
            _logger.LogWarning(
                "Localization resources directory not found at {Path}. Falling back to keys.",
                resourcesPath);
            return;
        }

        foreach (var file in Directory.GetFiles(resourcesPath, "*.json"))
        {
            var culture = Path.GetFileNameWithoutExtension(file);
            try
            {
                var json = File.ReadAllText(file);
                var flat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                using var doc = JsonDocument.Parse(json);
                Flatten(doc.RootElement, prefix: string.Empty, flat);
                _resources[culture] = flat;

                _logger.LogInformation(
                    "Loaded {Count} localization entries for culture {Culture} from {File}",
                    flat.Count, culture, file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to load localization file {File}. Skipping.",
                    file);
            }
        }
    }

    /// <summary>
    /// Recursively flattens nested JSON objects into dot-notation keys.
    /// { "auth": { "x": "v" } }  →  { "auth.x": "v" }
    /// </summary>
    private static void Flatten(JsonElement element, string prefix, Dictionary<string, string> output)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    Flatten(prop.Value, key, output);
                }
                break;
            case JsonValueKind.String:
                output[prefix] = element.GetString() ?? string.Empty;
                break;
            // Numbers, booleans, null intentionally skipped — error messages are strings.
        }
    }

    public string this[string key]
    {
        get
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return ResolveKey(key, culture);
        }
    }

    public string this[string key, params object[] arguments]
    {
        get
        {
            var template = this[key];
            return arguments.Length > 0
                ? string.Format(CultureInfo.CurrentCulture, template, arguments)
                : template;
        }
    }

    public string GetString(string key, string culture)
    {
        return ResolveKey(key, culture);
    }

    private string ResolveKey(string key, string culture)
    {
        // 1. Try requested culture
        if (_resources.TryGetValue(culture, out var primary) &&
            primary.TryGetValue(key, out var value))
        {
            return value;
        }

        // 2. Fallback to default culture
        if (culture != DefaultCulture &&
            _resources.TryGetValue(DefaultCulture, out var fallback) &&
            fallback.TryGetValue(key, out var fallbackValue))
        {
            return fallbackValue;
        }

        // 3. Last resort: return the key itself so missing translations are visible
        // in the UI rather than silently empty.
        _logger.LogWarning(
            "Missing localization key {Key} (culture: {Culture})",
            key, culture);
        return key;
    }
}
