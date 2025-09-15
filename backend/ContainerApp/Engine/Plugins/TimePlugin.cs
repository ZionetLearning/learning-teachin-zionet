using System.ComponentModel;
using Microsoft.SemanticKernel;
using Engine.Services;
using Engine.Constants;

namespace Engine.Plugins;

public sealed class TimePlugin : ISemanticKernelPlugin
{
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<TimePlugin> _logger;

    public TimePlugin(IDateTimeProvider clock, ILogger<TimePlugin> logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [KernelFunction(PluginNames.CurrentTime)]
    [Description("Returns the current time in the format ISO-8601 (UTC).")]
    public string GetCurrentTime()
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("Plugin {PluginName} executed at {UtcNow}", nameof(TimePlugin), now);
        return now.ToString("O");
    }
}