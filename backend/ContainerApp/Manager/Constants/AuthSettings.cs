namespace Manager.Constants;

public sealed class AuthSettings
{
    public const int RefreshTokenExpiryDays = 7;
    public const int CsrfTokenExpiryMinutes = 30;
    public const string RefreshTokenCookieName = "refreshToken";
    public const string CsrfTokenCookieName = "csrfToken";
    // We need to find a better way to secure it
    public const string CookiePath = "/";
    public const int ClockSkewBuffer = 2;
    public const string UserIdClaimType = "userId";
    public const string RoleClaimType = "role";
    public const string RefreshTokenPolicyName = "RefreshTokenPolicy";
    public const string UnknownIpFallback = "unknown";
    public const string RefreshTokenPolicy = "RefreshTokenPolicy";
}
