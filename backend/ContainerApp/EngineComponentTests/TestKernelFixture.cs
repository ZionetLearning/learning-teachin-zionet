using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace EngineComponentTests;
public sealed class TestKernelFixture : IAsyncLifetime
{
    public Kernel Kernel { get; private set; } = default!;

    public Task InitializeAsync()
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = cfg["AzureOpenAI:Endpoint"];
        var apiKey = cfg["AzureOpenAI:ApiKey"];
        var deployment = cfg["AzureOpenAI:DeploymentName"];

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            throw new SkipException("No Azure OpenAI config -> skip AI tests");
        }

        Kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey)
                       .Build();

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}