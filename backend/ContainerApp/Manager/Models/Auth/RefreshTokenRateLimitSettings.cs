namespace Manager.Models.Auth;
public class RefreshTokenRateLimitSettings
{
    public int PermitLimit { get; set; }
    public int WindowMinutes { get; set; }
    public int QueueLimit { get; set; }
    public int RejectionStatusCode { get; set; }
}
