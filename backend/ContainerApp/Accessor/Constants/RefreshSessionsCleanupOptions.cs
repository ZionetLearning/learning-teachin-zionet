namespace Accessor.Constants;

public sealed class RefreshSessionsCleanupOptions
{
    public bool Enabled { get; set; } = true;
    public string TimeZone { get; set; } = "Asia/Jerusalem";
    public int Hour { get; set; } = 2;     // 02:30 local
    public int Minute { get; set; } = 30;
    public int BatchSize { get; set; } = 5000;
}
