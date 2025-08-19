namespace Accessor.Models.RefreshSessions;

public class RefreshSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public string IP { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
    public string? DeviceFingerprintHash { get; set; }

    public string? DeviceInfo => ParseDeviceFromUserAgent(UserAgent);

    private static string? ParseDeviceFromUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile Device";
        }

        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            return "Windows PC";
        }

        if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase))
        {
            return "Mac";
        }

        return "Unknown";
    }
}
