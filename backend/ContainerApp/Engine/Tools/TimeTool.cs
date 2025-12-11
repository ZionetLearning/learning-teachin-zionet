using System.ComponentModel;
using Engine.Services;

namespace Engine.Tools;

public sealed class TimeTools
{
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<TimeTools> _logger;

    public TimeTools(IDateTimeProvider clock, ILogger<TimeTools> logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Description("Returns the current time in ISO-8601 (UTC).")]
    public string GetCurrentTime()
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("Agent tool {Tool} executed at {UtcNow}", nameof(TimeTools), now);
        return now.ToString("O");
    }
}
