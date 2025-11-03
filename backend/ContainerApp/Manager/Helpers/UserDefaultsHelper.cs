using System.Security.Claims;
using Manager.Constants;
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
        {
            return SupportedLanguage.en;
        }

        var first = header.Split(',').FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first))
        {
            return SupportedLanguage.en;
        }

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

    /// <summary>
    /// Checks whether the current user is authorized to perform an action on the specified user.
    /// Returns true if the caller is either an administrator or the same user as the routeUserId.
    /// </summary>
    public static bool IsSelfOrAdmin(HttpContext http, Guid routeUserId)
    {
        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            return false;
        }

        var isAdmin = string.Equals(callerRole, Role.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
        return isAdmin || callerId == routeUserId;
    }
}
