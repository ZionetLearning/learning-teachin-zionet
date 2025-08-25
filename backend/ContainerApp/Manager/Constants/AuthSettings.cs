namespace Manager.Constants;

public sealed class AuthSettings
{
    public const int RefreshTokenExpiryDays = 7;
    public const int CsrfTokenExpiryMinutes = 30;
    public const string RefreshTokenCookieName = "refreshToken";
    public const string CsrfTokenCookieName = "csrfToken";
    public const string CookiePath = "/auth";
    public const int ClockSkewBuffer = 2;
    public const string NameClaimType = "userId";
    public const string RefreshTokenPolicyName = "RefreshTokenPolicy";
    public const string UnknownIpFallback = "unknown";
    public const string RefreshTokenPolicy = "RefreshTokenPolicy";

}
