using System.Security.Cryptography;
using System.Text.Json;
using DotQueue;
using Engine.Models.Sentences;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class SentencesService : ISentencesService
{
    private readonly Kernel _genKernel;
    private readonly ILogger<SentencesService> _log;
    private const string EasyPath = "Constants/Words/hintsEasy.txt";
    private const string MediumPath = "Constants/Words/hintsMedium.txt";
    private const string HardPath = "Constants/Words/hintsHard.txt";

    private static readonly Lazy<string[]> EasyWords = new(() => LoadList(EasyPath));
    private static readonly Lazy<string[]> MediumWords = new(() => LoadList(MediumPath));
    private static readonly Lazy<string[]> HardWords = new(() => LoadList(HardPath));
    public SentencesService([FromKeyedServices("gen")] Kernel genKernel, ILogger<SentencesService> log)
    {
        _genKernel = genKernel;
        _log = log;
    }

    public async Task<SentenceResponse> GenerateAsync(SentenceRequest req, CancellationToken ct = default)
    {
        _log.LogInformation("Inside sentence generator service");

        var func = _genKernel.Plugins["Sentences"]["Generate"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            ResponseFormat = typeof(SentenceResponse)
        };
        var hints = GetRandomHints(req.Difficulty.ToString().ToLowerInvariant(), 3);
        var hintsStr = string.Join(", ", hints);

        var args = new KernelArguments(exec)
        {
            ["difficulty"] = req.Difficulty.ToString().ToLowerInvariant(),
            ["nikud"] = req.Nikud.ToString().ToLowerInvariant(),
            ["count"] = req.Count.ToString(),
            ["hints"] = hintsStr
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
            else
            {
                _log.LogError("Error while generating sentences. The response is empty");
                throw new RetryableException("Error while generating sentences. The response is empty");
            }
        }
        else
        {
            _log.LogError("Error while generating sentences. The response is empty");
            throw new RetryableException("Error while generating sentences. The response is empty");
        }
    }

    private static string[] GetRandomHints(string difficulty, int count)
    {
        var pool = difficulty switch
        {
            "easy" => EasyWords.Value,
            "medium" => MediumWords.Value,
            "hard" => HardWords.Value,
            _ => Array.Empty<string>()
        };

        if (pool.Length == 0 || count <= 0)
        {
            return Array.Empty<string>();
        }

        count = Math.Min(count, pool.Length);

        var idx = Enumerable.Range(0, pool.Length).ToArray();
        for (var i = idx.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (idx[i], idx[j]) = (idx[j], idx[i]);
        }

        return idx.Take(count).Select(i => pool[i]).ToArray();
    }

    private static string[] LoadList(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        return File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
