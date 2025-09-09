using Manager.Models.Users;

namespace Manager.Helpers;

public static class UserDefaultsHelper
{
    /// <summary>
    /// Parses an Accept-Language header into a SupportedLanguage enum.
    /// Defaults to English if invalid or missing.
    /// </summary>
    public static SupportedLanguage ParsePreferredLanguage(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return SupportedLanguage.en;

        var first = header.Split(',').FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first))
            return SupportedLanguage.en;

        // Handle values like "he-IL;q=0.9"
        var lang = first.Split('-')[0].Split(';')[0].ToLowerInvariant();

        return Enum.TryParse<SupportedLanguage>(lang, true, out var parsed)
            ? parsed
            : SupportedLanguage.en;
    }

    /// <summary>
    /// Returns the default Hebrew level based on the user role.
    /// Only students have a default value; others get null.
    /// </summary>
    public static HebrewLevel? GetDefaultHebrewLevel(Role role)
    {
        return role == Role.Student
            ? HebrewLevel.beginner
            : null;
    }
}
