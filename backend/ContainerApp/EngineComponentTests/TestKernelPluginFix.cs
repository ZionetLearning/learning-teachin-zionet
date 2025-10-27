using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace EngineComponentTests;
public sealed class TestKernelPluginFix : IAsyncLifetime
{
    public Kernel Kernel { get; private set; } = default!;

    public Task InitializeAsync()
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = cfg["AzureOpenAI:Endpoint"];
        var apiKey = cfg["AzureOpenAI:ApiKey"];
        var deployment = cfg["AzureOpenAI:DeploymentName"];
        var path = "Plugins/Sentences";

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            throw new SkipException("No Azure OpenAI config -> skip 'gen' AI tests");
        }

        var pluginDir = ResolvePluginsDir(path);
        if (pluginDir is null || !Directory.Exists(pluginDir))
        {
            throw new SkipException($"Prompt plugin directory not found: {pluginDir ?? "(null)"}");
        }

        var kb = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deployment,
                endpoint: endpoint,
                apiKey: apiKey);

        kb.Plugins.AddFromPromptDirectory(pluginDir, "Sentences");

        Kernel = kb.Build();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static string? ResolvePluginsDir(string relative)
    {
        var current = AppContext.BaseDirectory;

        for (var i = 0; i < 6 && current is not null; i++)
        {
            var candidate = Path.Combine(current, relative);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(current);
            current = parent?.FullName;
        }

        return null;
    }
}