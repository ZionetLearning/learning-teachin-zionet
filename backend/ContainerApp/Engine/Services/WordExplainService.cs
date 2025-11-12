using System.Text.Json;
using DotQueue;
using Engine.Models.Words;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public interface IWordExplainService
{
    Task<WordExplainResponse> ExplainAsync(WordExplainRequest req, CancellationToken ct = default);
}

public class WordExplainService : IWordExplainService
{
    private readonly Kernel _kernel;
    private readonly ILogger<WordExplainService> _log;

    public WordExplainService([FromKeyedServices("gen")] Kernel kernel, ILogger<WordExplainService> log)
    {
        _kernel = kernel;
        _log = log;
    }

    public async Task<WordExplainResponse> ExplainAsync(WordExplainRequest req, CancellationToken ct = default)
    {
        _log.LogInformation("Inside word explain service");

        var func = _kernel.Plugins["WordExplain"]["Explain"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.2,
            ResponseFormat = typeof(WordExplainResponse)
        };

        var args = new KernelArguments(exec)
        {
            ["word"] = req.Word,
            ["context"] = req.Context
        };

        var result = await _kernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogError("Empty response for word {Word}", req.Word);
            throw new RetryableException($"Empty response for word {req.Word}");
        }

        var parsed = JsonSerializer.Deserialize<WordExplainResponse>(json);
        if (parsed is null)
        {
            _log.LogError("Invalid JSON for word {Word}: {Json}", req.Word, json);
            throw new RetryableException("Invalid JSON format from model");
        }

        return parsed;
    }
}
