using Manager.Constants;

namespace Manager.Helpers;
public static class CookieHelper
{
    public static void SetCookies(HttpResponse response, string refreshToken)
    {
        // For testing the flow
        var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        // -- Refresh Cookie --
        response.Cookies.Append(AuthSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true, // JavaScript can’t access the cookie (mitigates XSS). 
            Secure = !isTest, // Sent only over HTTPS
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
            Secure = !isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthSettings.CsrfTokenExpiryMinutes) // Short-lived, 30 minutes
        });
    }

    public static void ClearCookies(HttpResponse response)
    {
        // For testing the flow
        var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        // Clear the refresh token cookie
        response.Cookies.Delete(AuthSettings.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });

        // Clear the CSRF token cookie
        response.Cookies.Delete(AuthSettings.CsrfTokenCookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = !isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });
    }
}
