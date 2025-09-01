using System.Text.Json;
using DotQueue;
using Engine.Models.Sentences;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class SentencesService : ISentencesService
{
    private readonly Kernel _genKernel;

    public SentencesService([FromKeyedServices("gen")] Kernel genKernel)
    {
        _genKernel = genKernel;
    }

    public async Task<SentenceResponse> GenerateAsync(SentenceRequest req, CancellationToken ct = default)
    {
        var func = _genKernel.Plugins["Sentences"]["Generate"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            ResponseFormat = typeof(SentenceResponse)
        };

        var args = new KernelArguments(exec)
        {
            ["difficulty"] = req.Difficulty.ToString().ToLowerInvariant(),
            ["nikud"] = req.Nikud.ToString().ToLowerInvariant(),
            ["count"] = req.Count.ToString()
        };

        var result = await _genKernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();
        if (json != null)
        {
            var parsed = JsonSerializer.Deserialize<SentenceResponse>(json);

            if (parsed != null)
            {
                return parsed;
            }
        }
        else
        {
            throw new RetryableException("Error while generating sentences. The response is empty");
        }
    }
}
