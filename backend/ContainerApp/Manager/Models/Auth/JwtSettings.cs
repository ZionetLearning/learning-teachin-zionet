namespace Manager.Models.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string RefreshTokenHashKey { get; set; } = string.Empty;
    public int RefreshTokenTTL { get; set; } = 60;
    // nedd to change it in here back to 15
    public int AccessTokenTTL { get; set; } = 2;

}