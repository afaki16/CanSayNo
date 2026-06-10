namespace Kickstart.Application.Common.Localization;

/// <summary>
/// JSON-based localization interface for backend error messages.
/// Resolves keys like "auth.invalidCredentials" to localized strings based on
/// the current request culture (set by RequestLocalizationMiddleware).
///
/// Resource files are located in Kickstart.Infrastructure/Resources/{culture}.json.
/// </summary>
public interface IJsonStringLocalizer
{
    /// <summary>
    /// Gets the localized string for the given key.
    /// Falls back to the key itself if no translation is found.
    /// </summary>
    string this[string key] { get; }

    /// <summary>
    /// Gets the localized string with format arguments.
    /// Example: localizer["auth.welcomeUser", userName]
    /// </summary>
    string this[string key, params object[] arguments] { get; }

    /// <summary>
    /// Gets the localized string for an explicit culture (ignoring current request culture).
    /// Useful for email content where the recipient's preference is known.
    /// </summary>
    string GetString(string key, string culture);
}
