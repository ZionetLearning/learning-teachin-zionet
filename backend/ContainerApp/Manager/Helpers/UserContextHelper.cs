using Manager.Constants;

namespace Manager.Helpers;

public static class UserContextHelper
{
    /// <summary>
    /// Extracts the user ID from HttpContext.User claims.
    /// </summary>
    public static Guid? GetUserId(HttpContext httpContext)
    {
        if (httpContext == null || httpContext.User == null)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirst(AuthSettings.NameClaimType);

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : null;
    }
}
