using Manager.Constants;

namespace Manager.Helpers;
public static class CookieHelper
{

    // for now we return the csrfToken here but in the future we will use it in the header
    public static string SetCookies(HttpResponse response, string refreshToken)
    {
        // -- Refresh Cookie --
        response.Cookies.Append(AuthSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true, // JavaScript can’t access the cookie (mitigates XSS). 
            Secure = true, // Sent only over HTTPS
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None, // Allows the cookie in cross-site requests
            Path = AuthSettings.CookiePath, //Only sent to /api/auth, not the entire domain.
            Expires = DateTimeOffset.UtcNow.AddDays(AuthSettings.RefreshTokenExpiryDays) // Expires in 7 days
        });

        // -- CSRF Cookie --
        var csrfToken = Guid.NewGuid().ToString("N"); // Generate a CSRF token
        response.Cookies.Append(AuthSettings.CsrfTokenCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false, // Must be accessible to JS

            // Notice!! for now its sent over http but in prodeuxtion need to change to https !!! 
            Secure = false,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.Lax,
            Path = AuthSettings.CookiePath,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthSettings.CsrfTokenExpiryMinutes) // Short-lived, 30 minutes
        });

        return csrfToken;
    }

    public static void ClearCookies(HttpResponse response)
    {
        // Clear the refresh token cookie
        response.Cookies.Delete(AuthSettings.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });

        // Clear the CSRF token cookie
        response.Cookies.Delete(AuthSettings.CsrfTokenCookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });
    }
}