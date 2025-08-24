using Manager.Constants;
using Manager.Models.Auth.Test;
using Microsoft.Extensions.Options;

namespace Manager.Helpers;
public class CookieHelper
{
    private readonly bool _isTest;

    public CookieHelper(IOptions<TestSettings> testOptions)
    {
        _isTest = testOptions.Value.IsTest;
    }

    public void SetCookies(HttpResponse response, string refreshToken)
    {
        // For testing the flow
        //var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        // -- Refresh Cookie --
        response.Cookies.Append(AuthSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true, // JavaScript can’t access the cookie (mitigates XSS). 
            Secure = !_isTest, // Sent only over HTTPS
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
            Secure = !_isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthSettings.CsrfTokenExpiryMinutes) // Short-lived, 30 minutes
        });
    }

    public void ClearCookies(HttpResponse response)
    {
        // For testing the flow
        //var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        // Clear the refresh token cookie
        response.Cookies.Delete(AuthSettings.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });

        // Clear the CSRF token cookie
        response.Cookies.Delete(AuthSettings.CsrfTokenCookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = !_isTest,
            // In the future when have domain or frontend, consider using SameSiteMode.Lax for better CSRF protection
            SameSite = SameSiteMode.None,
            Path = AuthSettings.CookiePath
        });
    }
}
