using Engine.Models.Sentences;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class SentencesService
{
    private readonly Kernel _kernel;

    public SentencesService(Kernel kernel) => _kernel = kernel;

    public async Task<string> GenerateAsync(SentenceRequest req, CancellationToken ct = default)
    {
        var func = _kernel.Plugins["Sentences"]["Generate"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
        };

        var args = new KernelArguments(exec)
        {
            ["difficulty"] = req.Difficulty.ToString(),
            ["nikud"] = req.Nikud,
            ["count"] = req.Count
        };

        var result = await _kernel.InvokeAsync(func, args, ct);
        return result.ToString() ?? "{}";
    }
}
