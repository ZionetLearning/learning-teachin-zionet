using System.ComponentModel;
using Microsoft.SemanticKernel;
using Engine.Services;
using Engine.Constants;

namespace Engine.Plugins;

public sealed class TimePlugin : ISemanticKernelPlugin
{
    private readonly IDateTimeProvider _clock;

    public TimePlugin(IDateTimeProvider clock) => _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    [KernelFunction(PluginNames.CurrentTime)]
    [Description("Returns the current time in the format ISO-8601 (UTC).")]
    public string GetCurrentTime() => _clock.UtcNow.ToString("O");
}