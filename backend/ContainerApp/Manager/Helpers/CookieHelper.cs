namespace Manager.Helpers;

public static class CookieHelper
{
    public static void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
    {
        response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true, // JavaScript can’t access the cookie (mitigates XSS). 
            Secure = true, // Sent only over HTTPS
            SameSite = SameSiteMode.Strict, //Only sent in same-site requests
            Path = "/api/auth", //Only sent to /api/auth, not the entire domain.
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Expires in 7 days
        });
    }

    public static void ClearRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }
}

