using System.Globalization;

namespace Accessor;

public static class InternalConfiguration
{
    public static readonly Dictionary<string, string> Default = new()
    {
        //Cache default configuration:

        // Cache expiration: 10 minutes
        ["TaskCache:TTLInSeconds"] = TimeSpan.FromMinutes(10).TotalSeconds.ToString(CultureInfo.InvariantCulture),

        // Retry policies (optional for the future)
        ["TaskCache:MaxRetries"] = "3",
        ["TaskCache:RetryBackoffMs"] = "200"
    };
}
