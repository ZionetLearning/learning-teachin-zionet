namespace Manager.Constants;

public sealed class AuthSettings
{
    public const int RefreshTokenExpiryDays = 7;
    public const string RefreshTokenCookieName = "refreshToken";
    public const string RefreshTokenCookiePath = "/api/auth";
    public const int ClockSkewBuffer = 2;
    public const string NameClaimType = "userId";
}
