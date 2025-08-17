using Manager.Constants;

namespace Manager.Helpers;
public static class CookieHelper
{
    public static void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
    {
        response.Cookies.Append(AuthSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true, // JavaScript can’t access the cookie (mitigates XSS). 
            Secure = true, // Sent only over HTTPS
            SameSite = SameSiteMode.None, // Allows the cookie in cross-site requests
            Path = AuthSettings.RefreshTokenCookiePath, //Only sent to /api/auth, not the entire domain.
            Expires = DateTimeOffset.UtcNow.AddDays(AuthSettings.RefreshTokenExpiryDays) // Expires in 7 days
        });
    }

    public static void ClearRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete(AuthSettings.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = AuthSettings.RefreshTokenCookiePath
        });
    }
}
